using GptCore;
using GptCore.Utilities;
using SlackGptSocket.BotInfo;
using SlackGptSocket.Utilities;
using SlackNet;
using SlackNet.Blocks;
using SlackNet.Events;
using SlackNet.WebApi;

namespace SlackGptSocket.SlackHandlers.Utilities;

public static class SlackMessageUtilities
{
    /// <summary>
    ///     Checks if the given message is a reply from the bot.
    /// </summary>
    /// <param name="message">The message to check.</param>
    /// <param name="botInfo">Information about the bot</param>
    /// <returns>True if the message is a bot reply, false otherwise.</returns>
    public static bool IsBotReply(this MessageEvent message, SlackBotInfo botInfo)
    {
        return message.User == botInfo.BotInfo.UserId;
    }

    /// <summary>
    ///     Checks if the given message is a reply from the user that mentioned the bot.
    /// </summary>
    /// <param name="message">The message to check.</param>
    /// <param name="botInfo">Information about the bot</param>
    /// <returns>True if the message is a user reply, false otherwise.</returns>
    public static bool HasBotsMention(this MessageEvent message, SlackBotInfo botInfo)
    {
        return message.Text.Contains("<@" + botInfo.BotInfo.UserId + ">");
    }
}