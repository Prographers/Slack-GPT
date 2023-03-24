using SlackNet;
using SlackNet.Blocks;
using SlackNet.Events;
using SlackNet.WebApi;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Slack_GPT_Socket;

/// <summary>
///     A simple event handler that says pong when you say ping.
/// </summary>
internal class SlackMentionHandler : IEventHandler<AppMention>
{
    private readonly ISlackApiClient _slack;
    private readonly ILogger _log;
    private readonly GptClient _gptClient;
    private readonly SlackBotInfo _botInfo;

    public SlackMentionHandler(ISlackApiClient slack, ILogger<SlackMentionHandler> log, GptClient gptClient,
        SlackBotInfo botInfo)
    {
        _slack = slack;
        _log = log;
        _gptClient = gptClient;
        _botInfo = botInfo;
    }

    /// <summary>
    ///     Handles incoming AppMention events and responds accordingly.
    /// </summary>
    /// <param name="slackEvent">The AppMention event.</param>
    public async Task Handle(AppMention slackEvent)
    {
        if (IsBot(slackEvent)) return;

        RemoveMentionsFromText(slackEvent);

        var context = await ResolveConversationContext(slackEvent);

        await PostLoadingMessage(slackEvent);

        try
        {
            var text = await GeneratePrompt(slackEvent, context, slackEvent.User);

            if (HasError(text))
            {
                await PostErrorEphemeralMessage("GptClient",slackEvent, text.Error);
                return;
            }

            await PostGptResponseMessage(slackEvent, text);
        }
        catch (SlackException e)
        {
            await PostErrorEphemeralMessage("SlackException" ,slackEvent, e.Message, string.Join("\n\t", e.ErrorMessages));
        }
        catch (Exception e)
        {
            await PostErrorEphemeralMessage("Unexpected", slackEvent, e.Message, e.StackTrace);
        }
    }

    /// <summary>
    ///     Checks if the event is triggered by a bot.
    /// </summary>
    /// <param name="slackEvent">The AppMention event.</param>
    /// <returns>True if the event is triggered by a bot, false otherwise.</returns>
    private bool IsBot(AppMention slackEvent)
    {
        return slackEvent.User == _botInfo.BotInfo.UserId;
    }

    /// <summary>
    ///     Removes mentions from the text in the Slack event, so that the text doesn't contain the bot's name.
    ///     This is so that the bot will not be confused by its own name.
    /// </summary>
    /// <param name="slackEvent">The AppMention event.</param>
    private void RemoveMentionsFromText(AppMention slackEvent)
    {
        slackEvent.Text = slackEvent.Text.Replace("<@" + _botInfo.BotInfo.UserId + ">", "").Trim();
    }

    /// <summary>
    ///     Resolves the conversation context for the given Slack event.
    /// </summary>
    /// <param name="slackEvent">The AppMention event.</param>
    /// <returns>A list of WritableChatPrompt instances representing the conversation context.</returns>
    private async Task<List<WritableChatPrompt>> ResolveConversationContext(AppMention slackEvent)
    {
        var context = new List<WritableChatPrompt>();

        if (slackEvent.ThreadTs != null)
        {
            var replies = await _slack.Conversations.Replies(slackEvent.Channel, slackEvent.ThreadTs, slackEvent.Ts);
            foreach (var reply in replies.Messages)
            {
                if (IsBotReply(reply))
                {
                    var response = SlackParserUtils.RemoveContextBlockFromResponses(reply);
                    if (response != null) context.Add(new WritableChatPrompt("assistant", response));
                }
                else if (IsUserReply(reply))
                {
                    if (slackEvent.Ts == reply.Ts)
                        continue;
                    var response = reply.Text.Replace("<@" + _botInfo.BotInfo.UserId + ">", "").Trim();
                    context.Add(new WritableChatPrompt("user", response));
                }
            }
        }

        context.Add(new WritableChatPrompt("user", slackEvent.Text));

        return context;
    }



    /// <summary>
    ///     Checks if the given message is a reply from the bot.
    /// </summary>
    /// <param name="reply">The message to check.</param>
    /// <returns>True if the message is a bot reply, false otherwise.</returns>
    private bool IsBotReply(MessageEvent reply)
    {
        return reply.User == _botInfo.BotInfo.UserId;
    }

    /// <summary>
    ///     Checks if the given message is a reply from the user.
    /// </summary>
    /// <param name="reply">The message to check.</param>
    /// <returns>True if the message is a user reply, false otherwise.</returns>
    private bool IsUserReply(MessageEvent reply)
    {
        return reply.Text.Contains("<@" + _botInfo.BotInfo.UserId + ">");
    }

    /// <summary>
    ///     Posts a loading message to the Slack channel.
    /// </summary>
    /// <param name="slackEvent">The AppMention event.</param>
    private async Task PostLoadingMessage(AppMention slackEvent)
    {
        await _slack.Chat.PostEphemeral(slackEvent.User, new Message
        {
            Text = SlackLoadingMessage.GetRandomLoadingMessage(),
            Channel = slackEvent.Channel,
            ThreadTs = slackEvent.ThreadTs ?? slackEvent.Ts
        });
    }
    
    /// <summary>
    ///     Posts a loading message to the Slack channel this ussualy shows when we are waiting for a long time.
    /// </summary>
    /// <param name="slackEvent"></param>
    private async Task PostLongWaitMessage(AppMention slackEvent)
    {
        await _slack.Chat.PostEphemeral(slackEvent.User, new Message
        {
            Text = SlackLoadingMessage.GetRandomLongWaitMessage(),
            Channel = slackEvent.Channel,
            ThreadTs = slackEvent.ThreadTs ?? slackEvent.Ts
        });
    }
    
    /// <summary>
    ///     Posts a loading message to the Slack channel this ussualy shows when we are waiting for a long time.
    /// </summary>
    /// <param name="slackEvent">Input slack event</param>
    private async Task PostGptAvailableWarningMessage(AppMention slackEvent)
    {
        await _slack.Chat.PostEphemeral(slackEvent.User, new Message
        {
            Text = "OpenAI returned an error, that suggests high demand on servers. We will retry in your name a few times.",
            Channel = slackEvent.Channel,
            ThreadTs = slackEvent.ThreadTs ?? slackEvent.Ts
        });
    }

    /// <summary>
    ///     Generates a prompt using the GPT client.
    /// </summary>
    /// <param name="slackEvent">Input slack event</param>
    /// <param name="context">The chat context to be used in generating the prompt.</param>
    /// <param name="userId">The user ID to be used in generating the prompt.</param>
    /// <returns>A GPTResponse instance containing the generated prompt.</returns>
    private async Task<GptResponse> GeneratePrompt(AppMention slackEvent, List<WritableChatPrompt> context, string userId)
    {
        // Start the periodic SendMessageProcessing task
        CancellationTokenSource cts = new CancellationTokenSource();
        Task periodicTask = PeriodicSendMessageProcessing(slackEvent, cts.Token);

        var result = await GeneratePromptRetry(slackEvent, context, userId);
        
        // Cancel the periodic task once the long running method returns
        cts.Cancel();

        // Ensure the periodic task has completed before proceeding
        try
        {
            await periodicTask;
        }
        catch (TaskCanceledException)
        {
            // Ignore CTS CancelledException
        }

        return result;
    }

    /// <summary>
    ///     Generates a prompt using the GPT client, and retries if the server is busy.
    /// </summary>
    /// <param name="slackEvent">Input slack event</param>
    /// <param name="context">The chat context to be used in generating the prompt.</param>
    /// <param name="userId">The user ID to be used in generating the prompt.</param>
    /// <returns>A GPTResponse instance containing the generated prompt.</returns>
    private async Task<GptResponse> GeneratePromptRetry(AppMention slackEvent, List<WritableChatPrompt> context, string userId)
    {
        var errorsCount = 0;
        while (true)
        {
            var result = await _gptClient.GeneratePrompt(context, userId);

            if (HasError(result) && result.Error!.Contains("The server had an error while processing your request. Sorry about that"))
            {
                if (errorsCount == 0)
                {
                    await PostGptAvailableWarningMessage(slackEvent);
                }
                errorsCount++;
                if (errorsCount == 5)
                {
                    return result;
                }
                
                // Wait 2 seconds times the number of errors
                await Task.Delay(TimeSpan.FromSeconds(2 * errorsCount));
            }
            else
            {
                return result;
            }
        }
    }

    /// <summary>
    ///     Periodically calls SendMessageProcessing() every 100 seconds.
    /// </summary>
    /// <param name="cancellationToken"></param>
    private async Task PeriodicSendMessageProcessing(AppMention slackEvent, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(120), cancellationToken);
            if(cancellationToken.IsCancellationRequested)
                break;
            await PostLongWaitMessage(slackEvent);
        }
    }

    /// <summary>
    ///     Checks if the given GPT response has an error.
    /// </summary>
    /// <param name="text">The GPT response to check.</param>
    /// <returns>True if the response has an error, false otherwise.</returns>
    private bool HasError(GptResponse text)
    {
        return text.Error != null;
    }

    /// <summary>
    ///     Posts an error message to the Slack channel as an ephemeral message.
    /// </summary>
    /// <param name="slackEvent">The AppMention event.</param>
    /// <param name="errorMessage">The error message to post.</param>
    /// <param name="details">Optional additional details about the error.</param>
    private async Task PostErrorEphemeralMessage(string source, AppMention slackEvent, string errorMessage, string? details = null)
    {
        await _slack.Chat.PostEphemeral(slackEvent.User, new Message
        {
            Text = $":rotating_light: [{source}] {errorMessage} :rotating_light: \n\t{(details != null ? $"\n\t{details}" : "")}",
            Channel = slackEvent.Channel,
            ThreadTs = slackEvent.ThreadTs ?? slackEvent.Ts
        });
    }

    /// <summary>
    ///     Posts the generated GPT response message to the Slack channel.
    /// </summary>
    /// <param name="slackEvent">The AppMention event.</param>
    /// <param name="text">The GPT response containing the message to post.</param>
    private async Task PostGptResponseMessage(AppMention slackEvent, GptResponse text)
    {
        // split the message into chunks of 2800 characters
        if (text.Message.Length > 2800)
        {
            var chunks = text.Message.SplitInParts(2800);

            for (var i = 0; i < chunks.Count; i++)
            {
                // post chunk message for all but last chunk
                if (i < chunks.Count - 1)
                    await PostSlackChunkMessage(slackEvent, chunks[i]);
                else
                {
                    text.Message = chunks[i];
                    await PostSlackMessage(slackEvent, text);
                }
            }
        }
        else await PostSlackMessage(slackEvent, text);
    }

    /// <summary>
    ///     Posts a chunk of the GPT response message to the Slack channel.
    /// </summary>
    /// <param name="slackEvent">The AppMention event.</param>
    /// <param name="text">The chunk of the GPT response message to post.</param>
    private async Task PostSlackChunkMessage(AppMention slackEvent, string text)
    {
        await _slack.Chat.PostMessage(new Message
        {
            Blocks = SlackParserUtils.ConvertTextToBlocks(text),
            Channel = slackEvent.Channel,
            ThreadTs = slackEvent.ThreadTs ?? slackEvent.Ts
        });
    }

    /// <summary>
    ///     Posts the GPT response message to the Slack channel with additional context information.
    /// </summary>
    /// <param name="slackEvent">The AppMention event.</param>
    /// <param name="text">The GPT response containing the message and context information to post.</param>
    private async Task PostSlackMessage(AppMention slackEvent, GptResponse text)
    {
        var blocks = SlackParserUtils.ConvertTextToBlocks(text.Message);
        blocks.Add(new ContextBlock
        {
            Elements = new[]
            {
                new Markdown($"by <@{slackEvent.User}> " +
                             $"using {text.Model} " +
                             $"in {text.ProcessingTime:hh':'mm':'ss} " +
                             $"with {text.Usage?.TotalTokens.ToString() ?? "undefined"} tokens " +
                             $"({Application.Version:v0.0.0})")
            }
        });
        await _slack.Chat.PostMessage(new Message
        {
            Channel = slackEvent.Channel,
            ThreadTs = slackEvent.ThreadTs ?? slackEvent.Ts,
            Blocks = blocks
        });
    }
}