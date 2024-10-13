using System.Text.RegularExpressions;
using OpenAI.Chat;
using Slack_GPT_Socket.GptApi.ParameterResolvers;
using Slack_GPT_Socket.Settings;
using Slack_GPT_Socket.Utilities.LiteDB;

namespace Slack_GPT_Socket.GptApi;

/// <summary>
///     Represents a GPT client resolver. Provide list of chat prompts and a GPT request to parse to.
/// </summary>
public class GptClientResolver
{
    private static readonly Regex ParameterRegex =
        new("""(-(?>\w|-)+)((\s+"[^"]+")|\s+\S+)?""", RegexOptions.Compiled, TimeSpan.FromSeconds(1));


    private readonly ParameterManager _parameterManager;
    private readonly GptDefaults _gptDefaults;
    private readonly IServiceProvider _serviceProvider;
    private Dictionary<string, BaseGptTool>? _tools;

    public GptClientResolver(GptCustomCommands customCommands, GptDefaults gptDefaults, IUserCommandDb userCommandDb,
        IServiceProvider serviceProvider)
    {
        _gptDefaults = gptDefaults;
        _serviceProvider = serviceProvider;

        _parameterManager = new ParameterManager(customCommands, gptDefaults, userCommandDb);
    }

    /// <summary>
    ///     Parses the given request and generates a ChatRequest instance.
    /// </summary>
    /// <param name="chatPrompts">The list of chat prompts.</param>
    /// <param name="request">The GPT request.</param>
    /// <param name="files">List of files attached to this prompt</param>
    /// <returns>A ChatRequest instance.</returns>
    public (List<ChatMessage> Messages, ChatCompletionOptions Options, string Model, List<BaseGptTool> UsedTools)
        ParseRequest(
            List<WritableMessage> chatPrompts, GptRequest request, List<ChatMessageContentPart>? files = null,
            IReadOnlyCollection<BaseGptTool>? tools = null)
    {
        foreach (var chatPrompt in chatPrompts)
        {
            var content = GptRequest.Default(_gptDefaults);
            content.UserId = chatPrompt.UserId;
            content.Prompt = chatPrompt.Content;
            ResolveModel(ref content);
            ResolveParameters(ref content);
            chatPrompt.Content = content.Prompt;
        }

        ResolveModel(ref request);
        ResolveParameters(ref request);
        var reqTools = ResolveTools(ref request);

        var requestPrompts = new List<WritableMessage>();
        requestPrompts.AddRange(chatPrompts);

        var messages = new List<ChatMessage>();
        var options = new ChatCompletionOptions
        {
            MaxOutputTokenCount = request.MaxTokens,
            Temperature = request.Temperature,
            TopP = request.TopP,
            PresencePenalty = request.PresencePenalty,
            FrequencyPenalty = request.FrequencyPenalty,
            EndUserId = request.UserId
        };

        if (!request.NoTools)
        {
            foreach (var tool in reqTools)
            {
                options.Tools.Add(BaseGptTool.ToChatTool(tool));
            }
        }


        chatPrompts.Last().Files = files ?? [];

        foreach (var chatPrompt in chatPrompts)
        {
            messages.Add(chatPrompt.ToChatMessage());
        }

        return (messages, options, request.Model, reqTools);
    }

    /// <summary>
    ///     Resolves the model to be used for the given input.
    /// </summary>
    /// <param name="input">The GPT request input.</param>
    private void ResolveModel(ref GptRequest input)
    {
        var promptWords = input.Prompt.Split(' ');
        var firstWord = promptWords[0].ToLower(); // extract first word of the prompt
        var modelFound = false;

        for (var i = 0; i < _parameterManager.Models.Length && !modelFound; i++)
        {
            var modelInfo = _parameterManager.Models[i];
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

        if (!modelFound) return;

        var inputModel = input.Model;
        // check if current model is valid
        if (_parameterManager.Models.All(modelInfo => modelInfo.Model != inputModel))
        {
            // if not, set model property of input to the model of the first item in the models array
            input.Model = _parameterManager.Models[0].Model;
        }
    }


    /// <summary>
    ///     Resolves additional parameters for the given input.
    /// </summary>
    /// <param name="input">The GPT request input.</param>
    private void ResolveParameters(ref GptRequest input)
    {
        var lastIndex = 0;
        var match = ParameterRegex.Match(input.Prompt);

        if (!match.Success) return;

        do
        {
            var paramName = match.Groups[1].Value;
            var paramValueTrim = match.Groups[2]?.Value.Trim() ?? string.Empty;
            var paramValue = paramValueTrim.Trim('"');

            var shouldBreak = false;
            try
            {
                var args = new ParameterEventArgs
                {
                    Name = paramName,
                    UserId = input.UserId,
                    Value = paramValue,
                    ValueRaw = paramValueTrim,
                    HasValue = true
                };

                var resolved = _parameterManager.TryResolve(input, args);

                if (!resolved)
                {
                    Console.WriteLine($"Unrecognized parameter: {paramName}");
                    shouldBreak = true;
                }

                if (shouldBreak) break;

                TrimInputFromParameter(input, args, ref lastIndex);
            }
            catch (Exception e)
            {
                // if we get an exception, we'll just ignore the parameter and move on
                break;
            }
        } while ((match = match.NextMatch()).Success);
    }

    /// <summary>
    ///     Resolves the tools to be used for the given input.
    /// </summary>
    private List<BaseGptTool> ResolveTools(ref GptRequest input)
    {
        if (_tools == null)
        {
            var tools = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOf(typeof(BaseGptTool)))
                .Select(type => (BaseGptTool)ActivatorUtilities.CreateInstance(_serviceProvider, type))
                .ToList();

            _tools = new Dictionary<string, BaseGptTool>();
            foreach (var tool in tools)
            {
                _tools.Add(tool.Name, tool);
            }
        }

        // Match tool to input
        var resultTools = new List<BaseGptTool>();
        foreach (var requestedTool in input.Tools)
        {
            // Check if the requested tool is in the list of tools as is
            if (_tools.TryGetValue(requestedTool, out var tool))
            {
                resultTools.Add(tool);
                continue;
            }

            foreach (var (_, value) in _tools)
            {
                var normalizedRequestedTool = requestedTool.GetNormalizedParameter();
                // normalize the alias to match the requested tool
                if (value.Aliases.All(alias => alias.GetNormalizedParameter() != normalizedRequestedTool)) continue;
                resultTools.Add(value);
                break;
            }
        }

        return resultTools;
    }

    /// <summary>
    ///     Trims the input prompt from the parameter. This is to ensure that the parameter is not passed to the model
    ///     directly.
    /// </summary>
    /// <param name="input">Input that will be processed</param>
    /// <param name="args">Command solver arguments</param>
    /// <param name="lastIndex">Last input index for tracking how far we are into the processing</param>
    private static void TrimInputFromParameter(GptRequest input, ParameterEventArgs args, ref int lastIndex)
    {
        // Trim the input Prompt to remove the parameter,
        // update last index to check if we've reached the end of the parameters
        var paramNameIndex = input.Prompt.IndexOf(args.Name, StringComparison.InvariantCultureIgnoreCase);

        string searchString;
        // Determine if the parameter has a value, because if it doesn't we don't want to remove it from the prompt!
        if (args.HasValue)
        {
            // Find last index of this value args.ValueRaw
            var paramValueIndex = input.Prompt.IndexOf(args.ValueRaw, StringComparison.InvariantCultureIgnoreCase) +
                                  args.ValueRaw.Length + 1;
            lastIndex = paramValueIndex;
            input.Prompt = input.Prompt.Substring(paramValueIndex, input.Prompt.Length - paramValueIndex).Trim();
            return;
        }

        lastIndex = paramNameIndex + args.Name.Length + 2;
        searchString = args.Name + " ";
        input.Prompt = input.Prompt.Replace(searchString, "").Trim();
    }
}