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

    public GptClientResolver(GptCustomCommands customCommands, GptDefaults gptDefaults, IUserCommandDb userCommandDb)
    {
        _gptDefaults = gptDefaults;

        _parameterManager = new ParameterManager(customCommands, gptDefaults, userCommandDb);
    }

    /// <summary>
    ///     Parses the given request and generates a ChatRequest instance.
    /// </summary>
    /// <param name="chatPrompts">The list of chat prompts.</param>
    /// <param name="request">The GPT request.</param>
    /// <returns>A ChatRequest instance.</returns>
    public ChatRequest ParseRequest(List<WritableChatPrompt> chatPrompts, GptRequest request)
    {
        GptSystemMessageBuilder? contextMessage = null;
        foreach (var chatPrompt in chatPrompts)
        {
            var content = GptRequest.Default(_gptDefaults);
            content.UserId = chatPrompt.UserId;
            content.Prompt = chatPrompt.Content;
            ResolveModel(ref content);
            ResolveParameters(ref content);
            chatPrompt.Content = content.Prompt;

            // TODO Refactor this into a separate resolver.
            if (content.System.IsContextMessage == ContextMessageStatus.Set) contextMessage = content.System;
            else if (content.System.IsContextMessage == ContextMessageStatus.Cleared) contextMessage = null;
        }

        ResolveModel(ref request);
        ResolveParameters(ref request);

        if (contextMessage != null && !request.System.IsModified)
            request.System = contextMessage;

        WritableChatPrompt system;
        if (request.System.ShouldReplace)
            system = new WritableChatPrompt("system", "__system__", request.System.Build());
        else
        {
            system = new WritableChatPrompt("system", "__system__",
                $"You are a helpful assistant. Today is {DateTime.Now:yyyy-MM-ddTHH:mm:ssZ} " + request.System.Build());
        }

        var requestPrompts = new List<WritableChatPrompt>();
        requestPrompts.Add(system);
        requestPrompts.AddRange(chatPrompts);

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

        if (modelFound) return;

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
        Match match;

        while ((match = ParameterRegex.Match(input.Prompt)).Success)
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
        }
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

        int paramEndIndex;
        string searchString;
        // Determine if the parameter has a value, because if it doesn't we don't want to remove it from the prompt!
        if (args.HasValue)
        {
            paramEndIndex = paramNameIndex + args.Name.Length + args.Value.Length + 2;
            searchString = args.Name + " " + args.ValueRaw + " ";
        }
        else
        {
            paramEndIndex = paramNameIndex + args.Name.Length + 2;
            searchString = args.Name + " ";
        }

        // Update last index to check if we've reached the end of the parameters
        lastIndex = paramEndIndex;
        input.Prompt = input.Prompt.Replace(searchString, "").Trim();
    }
}