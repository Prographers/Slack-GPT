using GptCore;
using GptCore.Database;
using GptCore.Settings;
using Microsoft.Extensions.Options;
using SlackGptSocket.Settings;
using SlackGptSocket.Utilities.LiteDB;

namespace SlackGptSocket.GptApi;

/// <summary>
///     Represents a client for interacting with GPT-based AI models.
/// </summary>
public class GptClient
{
    private readonly ILogger _log;
    private readonly GptDefaults _gptDefaults;
    private readonly IGptClientResolver _resolver;

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
        _log = log;
        _gptDefaults = gptDefaults.Value;
        _resolver = new GptClientResolver(
            customCommands,
            _gptDefaults,
            userCommandDb, 
            settings.Value.OpenAIKey, 
            httpClient);
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
        var prompt = GptRequest.Default(_gptDefaults);
        prompt.UserId = userId;
        prompt.Prompt = userPrompt.Content;

        var chatRequest = _resolver.ParseRequest(chatPrompts, prompt);

        try
        {
            var result = await _resolver.GetCompletionAsync(chatRequest);
            _log.LogInformation("GPT response: {Response}", result.FirstChoice);

            return new GptResponse
            {
                Message = result.FirstChoice,
                Model = result.Model,
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
}