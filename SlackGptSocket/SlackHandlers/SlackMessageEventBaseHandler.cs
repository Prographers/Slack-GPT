using GptCore;
using GptCore.Utilities;
using SlackGptSocket.BotInfo;
using SlackGptSocket.GptApi;
using SlackGptSocket.Settings;
using SlackGptSocket.SlackHandlers.Utilities;
using SlackGptSocket.Utilities;
using SlackNet;
using SlackNet.Events;
using SlackNet.Interaction;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace SlackGptSocket.SlackHandlers;

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
                await SlackMessageFormat.PostErrorEphemeralMessage(_slack, "GptClient", slackEvent, text.Error);
                return;
            }

            await SlackMessageFormat.PostGptResponseMessage(_slack, slackEvent, text, true);
        }
        catch (SlackException e)
        {
            await SlackMessageFormat.PostErrorEphemeralMessage(_slack, "SlackException", slackEvent, e.Message,
                string.Join("\n\t", e.ErrorMessages));
        }
        catch (Exception e)
        {
            await SlackMessageFormat.PostErrorEphemeralMessage(_slack, "Unexpected", slackEvent, e.Message,
                e.StackTrace);
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

        await SlackMessageFormat.PostLoadingMessage(_slack, slackEvent);

        try
        {
            var text = await GeneratePrompt(slackEvent, context, slackEvent.User);

            if (HasError(text))
            {
                await SlackMessageFormat.PostErrorEphemeralMessage(_slack, "GptClient", slackEvent, text.Error);
                return;
            }

            await SlackMessageFormat.PostGptResponseMessage(_slack, slackEvent, text);
        }
        catch (SlackException e)
        {
            await SlackMessageFormat.PostErrorEphemeralMessage(_slack, "SlackException", slackEvent, e.Message,
                string.Join("\n\t", e.ErrorMessages));
        }
        catch (Exception e)
        {
            await SlackMessageFormat.PostErrorEphemeralMessage(_slack, "Unexpected", slackEvent, e.Message,
                e.StackTrace);
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

        await SlackMessageFormat.PostLoadingMessage(_slack, slackEvent);

        try
        {
            var text = await GeneratePrompt(slackEvent, context, slackEvent.User);

            if (HasError(text))
            {
                await SlackMessageFormat.PostErrorEphemeralMessage(_slack, "GptClient", slackEvent, text.Error);
                return;
            }

            await SlackMessageFormat.PostGptResponseMessage(_slack, slackEvent, text);
        }
        catch (SlackException e)
        {
            await SlackMessageFormat.PostErrorEphemeralMessage(_slack, "SlackException", slackEvent, e.Message,
                string.Join("\n\t", e.ErrorMessages));
        }
        catch (Exception e)
        {
            await SlackMessageFormat.PostErrorEphemeralMessage(_slack, "Unexpected", slackEvent, e.Message,
                e.StackTrace);
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
    /// <returns>A list of WritableChatPrompt instances representing the conversation context.</returns>
    public async Task<List<WritableChatPrompt>> ResolveConversationContext(MessageEventBase slackEvent)
    {
        var context = new List<WritableChatPrompt>();

        if (slackEvent.ThreadTs != null)
        {
            var replies = await _slack.Conversations.Replies(slackEvent.Channel, slackEvent.ThreadTs, slackEvent.Ts);
            foreach (var reply in replies.Messages)
            {
                if (reply.IsBotReply(_botInfo))
                {
                    var response = SlackParserUtils.RemoveContextBlockFromResponses(reply);
                    if (response != null) context.Add(new WritableChatPrompt("assistant", "__assistant__", response));
                }
                else if (reply.HasBotsMention(_botInfo))
                {
                    if (slackEvent.Ts == reply.Ts)
                        continue;
                    var response = reply.Text.Replace("<@" + _botInfo.BotInfo.UserId + ">", "").Trim();
                    context.Add(new WritableChatPrompt("user", reply.User, response));
                }
            }
        }

        context.Add(new WritableChatPrompt("user", slackEvent.User, slackEvent.Text));

        return context;
    }


    /// <summary>
    ///     Generates a prompt using the GPT client.
    /// </summary>
    /// <param name="slackEvent">Input slack event</param>
    /// <param name="context">The chat context to be used in generating the prompt.</param>
    /// <param name="userId">The user ID to be used in generating the prompt.</param>
    /// <returns>A GPTResponse instance containing the generated prompt.</returns>
    private async Task<GptResponse> GeneratePrompt(MessageEventBase slackEvent, List<WritableChatPrompt> context,
        string userId)
    {
        // Start the periodic SendMessageProcessing task
        var cts = new CancellationTokenSource();
        var periodicTask = SlackMessageFormat.PeriodicSendMessageProcessing(_slack, slackEvent, cts.Token);

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
    private async Task<GptResponse> GeneratePromptRetry(MessageEventBase slackEvent, List<WritableChatPrompt> context,
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
                if (errorsCount == 0) await SlackMessageFormat.PostGptAvailableWarningMessage(_slack, slackEvent);
                errorsCount++;
                if (errorsCount == 5) return result;

                // Wait 2 seconds times the number of errors
                await Task.Delay(TimeSpan.FromSeconds(2 * errorsCount));
            }
            else return result;
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
}