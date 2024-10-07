using Slack_GPT_Socket.GptApi;
using Slack_GPT_Socket.Settings;
using SlackNet;
using SlackNet.Blocks;
using SlackNet.Events;
using SlackNet.Interaction;
using SlackNet.WebApi;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Slack_GPT_Socket;

/// <summary>
///     Base event handler for MessageEventBase events.
/// </summary>
public class SlackMessageEventBaseHandler
{
    private readonly ISlackApiClient _slack;
    private readonly ILogger _log;
    private readonly GptClient _gptClient;
    private readonly SlackBotInfo _botInfo;
    private readonly SlackSettings _slackSettings;

    public SlackMessageEventBaseHandler(
        ISlackApiClient slack,
        ILogger log,
        GptClient gptClient,
        SlackBotInfo botInfo,
        SlackSettings slackSettings)
    {
        _slack = slack;
        _log = log;
        _gptClient = gptClient;
        _botInfo = botInfo;
        _slackSettings = slackSettings;
    }

    /// <summary>
    ///     Handles SlashCommand events and responds as if this is a message in bot's chat that will generate an answer form
    ///     OpenAI GPT.
    /// </summary>
    /// <param name="slashCommand"></param>
    public async Task CommandHandler(SlashCommand slashCommand)
    {
        // Convert SlashCommand to MessageEventBase
        var slackEvent = new AppMention
        {
            Channel = slashCommand.ChannelId,
            User = slashCommand.UserId,
            Text = slashCommand.Text,
            Team = slashCommand.TeamId,
            EventTs = slashCommand.TriggerId,
            Ts = slashCommand.TriggerId
        };

        RemoveMentionsFromText(slackEvent);

        var context = await ResolveConversationContext(slackEvent);

        try
        {
            var text = await GeneratePrompt(slackEvent, context, slackEvent.User);

            if (HasError(text))
            {
                await PostErrorEphemeralMessage("GptClient", slackEvent, text.Error);
                return;
            }

            await PostGptResponseMessage(slackEvent, text, true);
        }
        catch (SlackException e)
        {
            await PostErrorEphemeralMessage("SlackException", slackEvent, e.Message,
                string.Join("\n\t", e.ErrorMessages));
        }
        catch (Exception e)
        {
            await PostErrorEphemeralMessage("Unexpected", slackEvent, e.Message, e.StackTrace);
        }
    }

    /// <summary>
    ///     Handles incoming MessageEventBase events and responds accordingly as if this is a message in bot's chat.
    /// </summary>
    /// <param name="slackEvent"></param>
    public async Task MessageHandler(MessageEventBase slackEvent)
    {
        if (IsBot(slackEvent)) return;
        if (!IsBotChannel(slackEvent) && _slackSettings.OnlyMentionsOutsideBotChannel) return;

        RemoveMentionsFromText(slackEvent);

        var context = await ResolveConversationContext(slackEvent);

        await PostLoadingMessage(slackEvent);

        try
        {
            var text = await GeneratePrompt(slackEvent, context, slackEvent.User);

            if (HasError(text))
            {
                await PostErrorEphemeralMessage("GptClient", slackEvent, text.Error);
                return;
            }

            await PostGptResponseMessage(slackEvent, text);
        }
        catch (SlackException e)
        {
            await PostErrorEphemeralMessage("SlackException", slackEvent, e.Message,
                string.Join("\n\t", e.ErrorMessages));
        }
        catch (Exception e)
        {
            await PostErrorEphemeralMessage("Unexpected", slackEvent, e.Message, e.StackTrace);
        }
    }

    /// <summary>
    ///     Handles incoming MessageEventBase events and responds accordingly as if this is a mention.
    /// </summary>
    /// <param name="slackEvent">Data about the message</param>
    public async Task MentionHandler(MessageEventBase slackEvent)
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
                await PostErrorEphemeralMessage("GptClient", slackEvent, text.Error);
                return;
            }

            await PostGptResponseMessage(slackEvent, text);
        }
        catch (SlackException e)
        {
            await PostErrorEphemeralMessage("SlackException", slackEvent, e.Message,
                string.Join("\n\t", e.ErrorMessages));
        }
        catch (Exception e)
        {
            await PostErrorEphemeralMessage("Unexpected", slackEvent, e.Message, e.StackTrace);
        }
    }

    /// <summary>
    ///     Checks if the event is triggered by a bot.
    /// </summary>
    /// <param name="slackEvent">The MessageEventBase event.</param>
    /// <returns>True if the event is triggered by a bot, false otherwise.</returns>
    private bool IsBot(MessageEventBase slackEvent)
    {
        return slackEvent.User == _botInfo.BotInfo.UserId;
    }

    /// <summary>
    ///     Checks if the event is triggered on the bot's channel.
    /// </summary>
    /// <param name="slackEvent">The MessageEventBase event.</param>
    /// <returns>True if the event is triggered on a bot channel, false otherwise.</returns>
    private bool IsBotChannel(MessageEventBase slackEvent)
    {
        return slackEvent switch
        {
            MessageEvent { ChannelType: "im" } => true,
            _ => false
        };
    }

    /// <summary>
    ///     Removes mentions from the text in the Slack event, so that the text doesn't contain the bot's name.
    ///     This is so that the bot will not be confused by its own name.
    /// </summary>
    /// <param name="slackEvent">The MessageEventBase event.</param>
    public void RemoveMentionsFromText(MessageEventBase slackEvent)
    {
        slackEvent.Text = slackEvent.Text.Replace("<@" + _botInfo.BotInfo.UserId + ">", "").Trim();
    }

    /// <summary>
    ///     Resolves the conversation context for the given Slack event.
    /// </summary>
    /// <param name="slackEvent">The MessageEventBase event.</param>
    /// <returns>A list of WritableMessage instances representing the conversation context.</returns>
    public async Task<List<WritableMessage>> ResolveConversationContext(MessageEventBase slackEvent)
    {
        var context = new List<WritableMessage>();

        if (slackEvent.ThreadTs != null)
        {
            var replies = await _slack.Conversations.Replies(slackEvent.Channel, slackEvent.ThreadTs, slackEvent.Ts);
            foreach (var reply in replies.Messages)
            {
                if (IsBotReply(reply))
                {
                    var response = SlackParserUtils.RemoveContextBlockFromResponses(reply);
                    if (response != null) context.Add(new WritableMessage(Role.Assistant, "__assistant__", response));
                }
                else if (IsUserReply(reply))
                {
                    if (slackEvent.Ts == reply.Ts)
                        continue;
                    var response = reply.Text.Replace("<@" + _botInfo.BotInfo.UserId + ">", "").Trim();
                    context.Add(new WritableMessage(Role.User, reply.User, response));
                }
            }
        }

        context.Add(new WritableMessage(Role.User, slackEvent.User, slackEvent.Text));

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
    /// <param name="slackEvent">The MessageEventBase event.</param>
    public async Task PostLoadingMessage(MessageEventBase slackEvent)
    {
        await _slack.Chat.PostEphemeral(slackEvent.User, new Message
        {
            Text = SlackLoadingMessage.GetRandomLoadingMessage(),
            Channel = slackEvent.Channel,
            ThreadTs = slackEvent.ThreadTs ?? slackEvent.Ts
        });
    }

    /// <summary>
    ///     Posts a ephemeral message to the Slack channel.
    /// </summary>
    /// <param name="slackEvent"></param>
    /// <param name="text"></param>
    public async Task PostEphemeralMessage(MessageEventBase slackEvent, string text)
    {
        await _slack.Chat.PostEphemeral(slackEvent.User, new Message
        {
            Text = text,
            Channel = slackEvent.Channel,
            ThreadTs = slackEvent.ThreadTs ?? slackEvent.Ts
        });
    }

    /// <summary>
    ///     Posts a loading message to the Slack channel this ussualy shows when we are waiting for a long time.
    /// </summary>
    /// <param name="slackEvent"></param>
    public async Task PostLongWaitMessage(MessageEventBase slackEvent)
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
    public async Task PostGptAvailableWarningMessage(MessageEventBase slackEvent)
    {
        await _slack.Chat.PostEphemeral(slackEvent.User, new Message
        {
            Text =
                "OpenAI returned an error, that suggests high demand on servers. We will retry in your name a few times.",
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
    private async Task<GptResponse> GeneratePrompt(MessageEventBase slackEvent, List<WritableMessage> context,
        string userId)
    {
        // Start the periodic SendMessageProcessing task
        var cts = new CancellationTokenSource();
        var periodicTask = PeriodicSendMessageProcessing(slackEvent, cts.Token);

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
    private async Task<GptResponse> GeneratePromptRetry(MessageEventBase slackEvent, List<WritableMessage> context,
        string userId)
    {
        var errorsCount = 0;
        while (true)
        {
            var result = await _gptClient.GeneratePrompt(context, userId);

            var repeatOnErrorsArray = new[]
            {
                "The server had an error while processing your request",
                "That model is currently overloaded with other requests",
                "The server is currently overloaded with other requests"
            };

            if (HasError(result) && result.Error!.Contains(repeatOnErrorsArray))
            {
                if (errorsCount == 0) await PostGptAvailableWarningMessage(slackEvent);
                errorsCount++;
                if (errorsCount == 5) return result;

                // Wait 2 seconds times the number of errors
                await Task.Delay(TimeSpan.FromSeconds(2 * errorsCount));
            }
            else return result;
        }
    }

    /// <summary>
    ///     Periodically calls SendMessageProcessing() every 100 seconds.
    /// </summary>
    /// <param name="cancellationToken"></param>
    private async Task PeriodicSendMessageProcessing(MessageEventBase slackEvent, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(120), cancellationToken);
            if (cancellationToken.IsCancellationRequested)
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
    /// <param name="slackEvent">The MessageEventBase event.</param>
    /// <param name="errorMessage">The error message to post.</param>
    /// <param name="details">Optional additional details about the error.</param>
    private async Task PostErrorEphemeralMessage(string source, MessageEventBase slackEvent, string errorMessage,
        string? details = null)
    {
        await _slack.Chat.PostEphemeral(slackEvent.User, new Message
        {
            Text =
                $":rotating_light: [{source}] {errorMessage} :rotating_light: \n\t{(details != null ? $"\n\t{details}" : "")}",
            Channel = slackEvent.Channel,
            ThreadTs = slackEvent.ThreadTs ?? slackEvent.Ts
        });
    }

    /// <summary>
    ///     Posts the generated GPT response message to the Slack channel.
    /// </summary>
    /// <param name="slackEvent">The MessageEventBase event.</param>
    /// <param name="text">The GPT response containing the message to post.</param>
    /// <param name="asEphemeral">Whether to post the message as an ephemeral message.</param>
    private async Task PostGptResponseMessage(MessageEventBase slackEvent, GptResponse text, bool asEphemeral = false)
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
                    if (asEphemeral)
                        await PostEphemeralMessage(slackEvent, text.Message);
                    else
                        await PostSlackMessage(slackEvent, text);
                }
            }
        }
        else
        {
            if (asEphemeral)
                await PostEphemeralMessage(slackEvent, text.Message);
            else
                await PostSlackMessage(slackEvent, text);
        }
    }

    /// <summary>
    ///     Posts a chunk of the GPT response message to the Slack channel.
    /// </summary>
    /// <param name="slackEvent">The MessageEventBase event.</param>
    /// <param name="text">The chunk of the GPT response message to post.</param>
    private async Task PostSlackChunkMessage(MessageEventBase slackEvent, string text)
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
    /// <param name="slackEvent">The MessageEventBase event.</param>
    /// <param name="text">The GPT response containing the message and context information to post.</param>
    private async Task PostSlackMessage(MessageEventBase slackEvent, GptResponse text)
    {
        var blocks = SlackParserUtils.ConvertTextToBlocks(text.Message);
        blocks.Add(new ContextBlock
        {
            Elements = new[]
            {
                new Markdown($"by <@{slackEvent.User}> " +
                             $"using {text.Model} " +
                             $"in {text.ProcessingTime:hh':'mm':'ss} " +
                             $"with {text.Usage?.TotalTokenCount} ({text.Usage?.InputTokenCount}/{text.Usage?.OutputTokenCount}) tokens " +
                             $"({Application.VersionString})")
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