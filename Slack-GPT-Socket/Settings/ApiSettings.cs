namespace Slack_GPT_Socket.Settings;

/// <summary>
///     Settings for the API.
/// </summary>
public class ApiSettings
{
    /// <summary>
    ///     The Slack bot token used for responding to messages and accessing history.
    /// </summary>
    public string SlackBotToken { get; set; } = null!;
    /// <summary>
    ///     The Slack app token used for sockets.
    /// </summary>
    public string SlackAppToken { get; set; } = null!;
    
    /// <summary>
    ///     The Slack signing secret used for verifying requests. Not used for sockets.
    /// </summary>
    public string SlackSigningSecret { get; set; } = null!;
    
    /// <summary>
    ///     The OpenAI API key.
    /// </summary>
    public string OpenAIKey { get; set; } = null!;
}