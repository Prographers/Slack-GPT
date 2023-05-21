namespace Slack_GPT_Socket.Settings;

/// <summary>
///     Settings for the Slack API
/// </summary>
public class SlackSettings
{
    /// <summary>
    ///     Only mentions will trigger the bot outside of bot's owned channel.
    ///     Otherwise he will respond to any message, in a thread where he was mentioned.
    /// </summary>
    public bool OnlyMentionsOutsideBotChannel { get; set; } = true;
}