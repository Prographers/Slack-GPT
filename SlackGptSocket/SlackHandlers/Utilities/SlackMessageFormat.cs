using GptCore;
using GptCore.Utilities;
using SlackGptSocket.Utilities;
using SlackNet;
using SlackNet.Blocks;
using SlackNet.Events;
using SlackNet.WebApi;

namespace SlackGptSocket.SlackHandlers.Utilities;

public static class SlackMessageFormat
{
    /// <summary>
    ///     Posts a ephemeral generic message to the Slack channel.
    /// </summary>
    /// <param name="slack">Slack API Client</param>
    /// <param name="slackEvent">Slack event to determine destination of the message based on previous messages</param>
    /// <param name="text">Text of the ephemeral message</param>
    public static async Task PostEphemeralMessage(ISlackApiClient slack, MessageEventBase slackEvent, string text)
    {
        await slack.Chat.PostEphemeral(slackEvent.User, new Message
        {
            Text = text,
            Channel = slackEvent.Channel,
            ThreadTs = slackEvent.ThreadTs ?? slackEvent.Ts
        });
    }
    
    /// <summary>
    ///     Posts a ephemeral loading message to the Slack channel.
    /// </summary>
    /// <param name="slack">Slack API Client</param>
    /// <param name="slackEvent">Slack event to determine destination of the message based on previous messages</param>
    public static async Task PostLoadingMessage(ISlackApiClient slack, MessageEventBase slackEvent)
    {
        await slack.Chat.PostEphemeral(slackEvent.User, new Message
        {
            Text = SlackLoadingMessage.GetRandomLoadingMessage(),
            Channel = slackEvent.Channel,
            ThreadTs = slackEvent.ThreadTs ?? slackEvent.Ts
        });
    }

    /// <summary>
    ///     Posts aa ephemeral loading message to the Slack channel this usually shows when we are waiting for a long time.
    /// </summary>
    /// <param name="slack">Slack API Client</param>
    /// <param name="slackEvent">Slack event to determine destination of the message based on previous messages</param>
    public static async Task PostLongWaitMessage(ISlackApiClient slack, MessageEventBase slackEvent)
    {
        await slack.Chat.PostEphemeral(slackEvent.User, new Message
        {
            Text = SlackLoadingMessage.GetRandomLongWaitMessage(),
            Channel = slackEvent.Channel,
            ThreadTs = slackEvent.ThreadTs ?? slackEvent.Ts
        });
    }

    /// <summary>
    ///     Posts a ephemeral error message to the Slack channel for user to know that OpenAI returned an error.
    /// </summary>
    /// <param name="slack">Slack API Client</param>
    /// <param name="slackEvent">Input slack event</param>
    public static async Task PostGptAvailableWarningMessage(ISlackApiClient slack, MessageEventBase slackEvent)
    {
        await slack.Chat.PostEphemeral(slackEvent.User, new Message
        {
            Text =
                "OpenAI returned an error, that suggests high demand on theirs servers. We will retry a few times for you.",
            Channel = slackEvent.Channel,
            ThreadTs = slackEvent.ThreadTs ?? slackEvent.Ts
        });
    }

    /// <summary>
    ///     Periodically calls SendMessageProcessing() every 120 seconds.
    /// </summary>
    /// <param name="slack">Slack API Client</param>
    /// <param name="slackEvent">Slack event to determine destination of the message based on previous messages</param>
    /// <param name="cancellationToken"></param>
    public static async Task PeriodicSendMessageProcessing(
        ISlackApiClient slack,
        MessageEventBase slackEvent,
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(120), cancellationToken);
            if (cancellationToken.IsCancellationRequested)
                break;
            await PostLongWaitMessage(slack, slackEvent);
        }
    }

    /// <summary>
    ///     Posts the generated GPT response message to the Slack channel.
    /// </summary>
    /// <param name="slack">Slack API Client</param>
    /// <param name="slackEvent">Slack event to determine destination of the message based on previous messages</param>
    /// <param name="text">The GPT response containing the message to post.</param>
    /// <param name="asEphemeral">Whether to post the message as an ephemeral message.</param>
    public static async Task PostGptResponseMessage(
        ISlackApiClient slack,
        MessageEventBase slackEvent,
        GptResponse text,
        bool asEphemeral = false)
    {
        // split the message into chunks of 2800 characters
        if (text.Message.Length > 2800)
        {
            var chunks = text.Message.SplitInParts(2800);

            for (var i = 0; i < chunks.Count; i++)
            {
                // post chunk message for all but last chunk
                if (i < chunks.Count - 1)
                    await PostSlackChunkMessage(slack, slackEvent, chunks[i]);
                else
                {
                    text.Message = chunks[i];
                    if (asEphemeral)
                        await PostEphemeralMessage(slack, slackEvent, text.Message);
                    else
                        await PostGptResponseMessage(slack, slackEvent, text);
                }
            }
        }
        else
        {
            if (asEphemeral)
                await PostEphemeralMessage(slack, slackEvent, text.Message);
            else
                await PostGptResponseMessage(slack, slackEvent, text);
        }
    }

    /// <summary>
    ///     Posts a chunk of the GPT response message to the Slack channel.
    /// </summary>
    /// <param name="slackEvent">Slack event to determine destination of the message based on previous messages</param>
    /// <param name="slack">Slack API Client</param>
    /// <param name="text">The chunk of the GPT response message to post.</param>
    public static async Task PostSlackChunkMessage(
        ISlackApiClient slack,
        MessageEventBase slackEvent,
        string text)
    {
        await slack.Chat.PostMessage(new Message
        {
            Blocks = SlackParserUtils.ConvertTextToBlocks(text),
            Channel = slackEvent.Channel,
            ThreadTs = slackEvent.ThreadTs ?? slackEvent.Ts
        });
    }

    /// <summary>
    ///     Posts the GPT response message to the Slack channel with additional context information.
    /// </summary>
    /// <param name="slackEvent">Slack event to determine destination of the message based on previous messages</param>
    /// <param name="slack">Slack API Client</param>
    /// <param name="text">The GPT response containing the message and context information to post.</param>
    public static async Task PostGptResponseMessage(
        ISlackApiClient slack,
        GptResponse text,
        MessageEventBase slackEvent)
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
                             $"({Application.VersionString})")
            }
        });
        await slack.Chat.PostMessage(new Message
        {
            Channel = slackEvent.Channel,
            ThreadTs = slackEvent.ThreadTs ?? slackEvent.Ts,
            Blocks = blocks
        });
    }

    /// <summary>
    ///     Posts an error message to the Slack channel as an ephemeral message.
    /// </summary>
    /// <param name="slack">Slack API Client</param>
    /// <param name="slackEvent">Slack event to determine destination of the message based on previous messages</param>
    /// <param name="errorMessage">The error message to post.</param>
    /// <param name="details">Optional additional details about the error.</param>
    public static async Task PostErrorEphemeralMessage(
        ISlackApiClient slack,
        string source,
        MessageEventBase slackEvent,
        string errorMessage,
        string? details = null)
    {
        await slack.Chat.PostEphemeral(slackEvent.User, new Message
        {
            Text =
                $":rotating_light: [{source}] {errorMessage} :rotating_light: \n\t{(details != null ? $"\n\t{details}" : "")}",
            Channel = slackEvent.Channel,
            ThreadTs = slackEvent.ThreadTs ?? slackEvent.Ts
        });
    }
}