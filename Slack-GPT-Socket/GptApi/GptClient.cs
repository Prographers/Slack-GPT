using System.Diagnostics;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OpenAI;
using OpenAI.Chat;
using Slack_GPT_Socket.Settings;
using Slack_GPT_Socket.Utilities.LiteDB;
using SlackNet.Blocks;
using SlackNet.Events;

namespace Slack_GPT_Socket.GptApi;

/// <summary>
///     Represents a client for interacting with GPT-based AI models.
/// </summary>
public class GptClient
{
    private readonly OpenAIClient _api;
    private readonly ILogger _log;
    private readonly IOptions<ApiSettings> _settings;
    private readonly GptDefaults _gptDefaults;
    private readonly GptClientResolver _resolver;
    private readonly IHttpClientFactory _httpClientFactory;

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
        IOptions<ApiSettings> settings,
        IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _api = new OpenAIClient(settings.Value.OpenAIKey);
        _log = log;
        _settings = settings;
        _gptDefaults = gptDefaults.Value;
        _resolver = new GptClientResolver(customCommands, _gptDefaults, userCommandDb);
    }

    /// <summary>
    ///     Generates a response based on the given chat prompts.
    /// </summary>
    /// <param name="slackEvent">Input slack event</param>
    /// <param name="chatPrompts">The list of chat prompts.</param>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A task representing the asynchronous operation, with a result of the generated response.</returns>
    public async Task<GptResponse> GeneratePrompt(MessageEventBase slackEvent, List<WritableMessage> chatPrompts,
        string userId)
    {
        // get the last prompt
        var userPrompt = chatPrompts.Last(chatPrompt => chatPrompt.Role == Role.User);
        var prompt = GptRequest.Default(_gptDefaults);
        prompt.UserId = userId;
        prompt.Prompt = userPrompt.Content;

        // TODO: Refactor me!!!
        
        var files = new List<ChatMessageContentPart>();
        foreach (var file in slackEvent.Files)
        {
            var fileUrl = file.UrlPrivateDownload ?? file.UrlPrivate;
            if (string.IsNullOrEmpty(fileUrl))
            {
                return new GptResponse
                {
                    Error = "Requested file to process with this request, but it doesn't have a download URL"
                };
            }

            var httpClient = _httpClientFactory.CreateClient();
            // configure httpClient to allow images and other files
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(file.Mimetype));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.Value.SlackBotToken);
            var fileRequest = await httpClient.GetAsync(fileUrl);
            if (!fileRequest.IsSuccessStatusCode)
            {
                return new GptResponse
                {
                    Error = "Requested file to process with this request, but it couldn't be downloaded successfully"
                };
            }
            var fileContent = await fileRequest.Content.ReadAsStreamAsync();
            var headers = fileRequest.Content.Headers;
            
            // check if headers contain the mimetype
            if (!headers.TryGetValues("Content-Type", out var contentTypes))
            {
                return new GptResponse
                {
                    Error = "Requested file to process with this request, but it doesn't have a mimetype"
                };
            }
            var contentType = contentTypes.FirstOrDefault();
            if (contentType == null)
            {
                return new GptResponse
                {
                    Error = "Requested file to process with this request, but it doesn't have a mimetype"
                };
            }
            // check if the mimetype is equal to the file mimetype
            if (contentType != file.Mimetype)
            {
                return new GptResponse
                {
                    Error = "Requested file to process with this request, but the mimetype doesn't match the file mimetype " +
                            $"expected {file.Mimetype} but got {contentType}"
                };
            }
            
            using var memoryStream = new MemoryStream();
            await fileContent.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            var chatPart = ChatMessageContentPart.CreateImagePart(
                await BinaryData.FromStreamAsync(memoryStream), file.Mimetype);
            files.Add(chatPart);
        }

        // TODO: Refactor me!!!
        
        if (slackEvent.Blocks != null)
        {
            foreach (var block in slackEvent.Blocks)
            {
                if (block is not RichTextBlock rtb) continue;
                foreach (var element in rtb.Elements)
                {
                    if (element is not RichTextSection rts) continue;
                    foreach (var innerElement in rts.Elements)
                    {
                        if (innerElement is not RichTextLink rtl) continue;

                        var uri = new Uri(rtl.Url);
                        if (uri.Scheme == "http" || uri.Scheme == "https")
                        {
                            var httpClient = _httpClientFactory.CreateClient();
                            var response = await httpClient.GetAsync(uri);
                            if (response.IsSuccessStatusCode &&
                                response.Content.Headers.ContentType!.MediaType!.StartsWith("image"))
                            {
                                var chatPart = ChatMessageContentPart.CreateImagePart(uri);
                                files.Add(chatPart);
                            }
                        }
                    }
                }
            }
        }

        var chatRequest = _resolver.ParseRequest(chatPrompts, prompt, files);

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