using SlackNet;
using SlackNet.WebApi;

namespace Slack_GPT_Socket;

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