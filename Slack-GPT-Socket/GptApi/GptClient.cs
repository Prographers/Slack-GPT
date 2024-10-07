using System.Diagnostics;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OpenAI;
using OpenAI.Chat;
using Slack_GPT_Socket.Settings;
using Slack_GPT_Socket.Utilities.LiteDB;

namespace Slack_GPT_Socket.GptApi;

/// <summary>
///     Represents a client for interacting with GPT-based AI models.
/// </summary>
public class GptClient
{
    private readonly OpenAIClient _api;
    private readonly ILogger _log;
    private readonly GptDefaults _gptDefaults;
    private readonly GptClientResolver _resolver;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GptClient" /> class.
    /// </summary>
    /// <param name="customCommands">Custom commands handler</param>
    /// <param name="log">The logger instance.</param>
    /// <param name="settings">The API settings.</param>
    public GptClient(
        GptCustomCommands customCommands, 
        IUserCommandDb userCommandDb,
        ILogger<GptClient> log, 
        IOptions<GptDefaults> gptDefaults,
        IOptions<ApiSettings> settings)
    {
        var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(10)
        };
        _api = new OpenAIClient(settings.Value.OpenAIKey);
        _log = log;
        _gptDefaults = gptDefaults.Value;
        _resolver = new GptClientResolver(customCommands, _gptDefaults, userCommandDb);
    }

    /// <summary>
    ///     Generates a response based on the given chat prompts.
    /// </summary>
    /// <param name="chatPrompts">The list of chat prompts.</param>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A task representing the asynchronous operation, with a result of the generated response.</returns>
    public async Task<GptResponse> GeneratePrompt(List<WritableMessage> chatPrompts, string userId)
    {
        // get the last prompt
        var userPrompt = chatPrompts.Last(chatPrompt => chatPrompt.Role == Role.User);
        var prompt = GptRequest.Default(_gptDefaults);
        prompt.UserId = userId;
        prompt.Prompt = userPrompt.Content;

        var chatRequest = _resolver.ParseRequest(chatPrompts, prompt);

        try
        {
            var sw = Stopwatch.StartNew();
            var model = _api.GetChatClient(chatRequest.Model);
            var result = await model.CompleteChatAsync(chatRequest.Messages, chatRequest.Options);
            var chatCompletion = result.Value;
            _log.LogInformation("GPT response: {Response}", JsonConvert.SerializeObject(chatCompletion));

            return new GptResponse
            {
                Message = chatCompletion.Content.Last().Text,
                Model = chatCompletion.Model,
                Usage = chatCompletion.Usage,
                ProcessingTime = sw.Elapsed
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
}