namespace Slack_GPT_Socket;

/// <summary>
///     Settings for the API.
/// </summary>
public class ApiSettings
{
    /// <summary>
    ///     The Slack bot token used for responding to messages and accessing history.
    /// </summary>
    public string SlackBotToken { get; set; }
    /// <summary>
    ///     The Slack app token used for sockets.
    /// </summary>
    public string SlackAppToken { get; set; }
    
    /// <summary>
    ///     The Slack signing secret used for verifying requests. Not used for sockets.
    /// </summary>
    public string SlackSigningSecret { get; set; }
    
    /// <summary>
    ///     The OpenAI API key.
    /// </summary>
    public string OpenAIKey { get; set; }
}