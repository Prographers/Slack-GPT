using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace Slack_GPT_Socket;

/// <summary>
///     Represents a client for interacting with GPT-based AI models.
/// </summary>
public class GptClient
{
    private static readonly ModelInfo[] Models;

    private readonly OpenAIClient _api;
    private readonly ILogger _log;

    /// <summary>
    ///     Initializes static members of the <see cref="GptClient" /> class.
    /// </summary>
    static GptClient()
    {
        Models = new ModelInfo[]
        {
            new("gpt-4", "gpt4"),
            new("gpt-3.5-turbo", "chatGPT", "gpt-3", "gpt3", "turbo")
        };
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="GptClient" /> class.
    /// </summary>
    /// <param name="log">The logger instance.</param>
    /// <param name="settings">The API settings.</param>
    public GptClient(ILogger<GptClient> log, IOptions<ApiSettings> settings)
    {
        var httpClient = new HttpClient()
        {
            Timeout = TimeSpan.FromMinutes(10),
        };
        _api = new OpenAIClient(settings.Value.OpenAIKey, OpenAIClientSettings.Default, httpClient);
        _log = log;
    }

    /// <summary>
    ///     Generates a response based on the given chat prompts.
    /// </summary>
    /// <param name="chatPrompts">The list of chat prompts.</param>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A task representing the asynchronous operation, with a result of the generated response.</returns>
    public async Task<GptResponse> GeneratePrompt(List<WritableChatPrompt> chatPrompts, string userId)
    {
        // get the last prompt
        var userPrompt = chatPrompts.Last(chatPrompt => chatPrompt.Role == "user");
        var prompt = new GptRequest
        {
            UserId = userId,
            Prompt = userPrompt.Content
        };

        var chatRequest = ParseRequest(chatPrompts, prompt);

        try
        {
            var result = await _api.ChatEndpoint.GetCompletionAsync(chatRequest);
            _log.LogInformation("GPT-3 response: {Response}", result.FirstChoice);

            return new GptResponse
            {
                Message = result.FirstChoice,
                Model = prompt.Model,
                Usage = result.Usage,
                ProcessingTime = result.ProcessingTime
            };
        }
        catch (Exception e)
        {
            return new GptResponse
            {
                Model = chatRequest.Model,
                Error = e.Message
            };
        }
    }

    /// <summary>
    ///     Parses the given request and generates a ChatRequest instance.
    /// </summary>
    /// <param name="chatPrompts">The list of chat prompts.</param>
    /// <param name="request">The GPT request.</param>
    /// <returns>A ChatRequest instance.</returns>
    private ChatRequest ParseRequest(List<WritableChatPrompt> chatPrompts, GptRequest request)
    {
        foreach (var chatPrompt in chatPrompts)
        {
            var content = new GptRequest { Prompt = chatPrompt.Content };
            ResolveModel(ref content);
            ResolveParameters(ref content);
            chatPrompt.Content = content.Prompt;
        }

        ResolveModel(ref request);
        ResolveParameters(ref request);

        WritableChatPrompt system;
        if (request.System != null)
            system = new WritableChatPrompt("system", request.System);
        else
        {
            system = new WritableChatPrompt("system",
                $"You are a helpful assistant. Today is {DateTime.Now:yyyy-MM-ddTHH:mm:ssZ}");
        }

        var requestPrompts = new List<WritableChatPrompt>();
        requestPrompts.Add(system);
        requestPrompts.AddRange(chatPrompts);

        // Map request to ChatRequest ctor
        var chatRequest = new ChatRequest(
            requestPrompts.Select(p => new ChatPrompt(p.Role, p.Content)).ToList(),
            maxTokens: request.MaxTokens,
            temperature: request.Temperature,
            topP: request.TopP,
            presencePenalty: request.PresencePenalty,
            frequencyPenalty: request.FrequencyPenalty,
            model: request.Model,
            user: request.UserId
        );

        return chatRequest;
    }

    /// <summary>
    ///     Resolves the model to be used for the given input.
    /// </summary>
    /// <param name="input">The GPT request input.</param>
    public static void ResolveModel(ref GptRequest input)
    {
        var promptWords = input.Prompt.Split(' ');
        var firstWord = promptWords[0]; // extract first word of the prompt
        var modelFound = false;

        // remove special characters from the end of the first word
        firstWord = new string(firstWord.Where(c => char.IsLetterOrDigit(c)).ToArray());

        for (var i = 0; i < Models.Length && !modelFound; i++)
        {
            var modelInfo = Models[i];
            var model = modelInfo.Model;
            var aliases = modelInfo.Aliases;

            if (model == firstWord || aliases.Contains(firstWord))
            {
                // remove resolved model name from prompt and set model property of input
                promptWords = promptWords.Skip(1).ToArray();
                input.Prompt = string.Join(" ", promptWords);
                input.Model = model;
                modelFound = true;
            }
        }

        if (!modelFound)
        {
            // if no match is found, set model property of input to the model of the first item in the models array
            input.Model = Models[0].Model;
        }
    }

    /// <summary>
    ///     Resolves additional parameters for the given input.
    /// </summary>
    /// <param name="input">The GPT request input.</param>
    public static void ResolveParameters(ref GptRequest input)
    {
        var parameterRegex = new Regex(@"(-\w+)((\s+""[^""]+"")|\s+\S+)?");
        var inputPrompt = input.Prompt;

        var lastIndex = 0;
        Match match;

        while ((match = parameterRegex.Match(inputPrompt)).Success)
        {
            var paramName = match.Groups[1].Value;
            var paramValueTrim = match.Groups[2]?.Value.Trim() ?? string.Empty;
            var paramValue = paramValueTrim.Trim('"');

            var paramNameIndex = inputPrompt.IndexOf(paramName, StringComparison.InvariantCultureIgnoreCase);
            var paramEndIndex = paramNameIndex + paramName.Length + paramValueTrim.Length + 2;

            if (lastIndex + 5 < paramNameIndex) break;

            lastIndex = paramEndIndex;
            input.Prompt = input.Prompt.Replace(paramName + " " + paramValueTrim, "").Trim();

            switch (paramName)
            {
                case "-maxTokens":
                    input.MaxTokens = int.Parse(paramValue);
                    break;
                case "-temperature":
                    input.Temperature = float.Parse(paramValue);
                    break;
                case "-topP":
                    input.TopP = float.Parse(paramValue);
                    break;
                case "-presencePenalty":
                    input.PresencePenalty = float.Parse(paramValue);
                    break;
                case "-frequencyPenalty":
                    input.FrequencyPenalty = float.Parse(paramValue);
                    break;
                case "-model":
                    input.Model = paramValue;
                    break;
                case "-system":
                    input.System = paramValue;
                    break;
                default:
                    Console.WriteLine($"Unrecognized parameter: {paramName}");
                    break;
            }

            inputPrompt = input.Prompt;
        }
    }

    /// <summary>
    ///     Represents information about a specific AI model.
    /// </summary>
    public class ModelInfo
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ModelInfo" /> class.
        /// </summary>
        /// <param name="model">The model name.</param>
        /// <param name="aliases">The model aliases.</param>
        public ModelInfo(string model, params string[] aliases)
        {
            Model = model;
            Aliases = aliases;
        }

        /// <summary>
        ///     Gets or sets the model name.
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        ///     Gets or sets the model aliases.
        /// </summary>
        public string[] Aliases { get; set; }
    }
}

/// <summary>
///     Represents a writable chat prompt used for generating AI responses.
/// </summary>
public sealed class WritableChatPrompt
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="WritableChatPrompt" /> class.
    /// </summary>
    /// <param name="role">The role of the chat prompt (e.g., "user" or "system").</param>
    /// <param name="content">The content of the chat prompt.</param>
    public WritableChatPrompt(string role, string content)
    {
        Role = role;
        Content = content;
    }

    /// <summary>
    ///     Gets or sets the role of the chat prompt.
    /// </summary>
    public string Role { get; set; }

    /// <summary>
    ///     Gets or sets the content of the chat prompt.
    /// </summary>
    public string Content { get; set; }
}