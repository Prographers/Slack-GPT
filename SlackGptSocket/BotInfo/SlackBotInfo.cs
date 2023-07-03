using SlackNet.WebApi;

namespace SlackGptSocket.BotInfo;

/// <summary>
///     Information about our bot
/// </summary>
public class SlackBotInfo
{
    /// <summary>
    ///     Information about our bot from SlackAPI
    /// </summary>
    public AuthTestResponse BotInfo { get; set; }
}