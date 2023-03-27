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

/// <summary>
///     Custom pre-defined commands for the bot.
/// </summary>
public class GptCommands
{
    /// <summary>
    ///     The custom commands.
    /// </summary>
    public List<GptCommand> Commands { get; set; }
}

/// <summary>
///     A custom command.
/// </summary>
public class GptCommand
{
    /// <summary>
    ///     The command to trigger the custom command.
    /// </summary>
    public string Command { get; set; }
    
    /// <summary>
    ///     The description of the command to display in the help.
    /// </summary>
    public string Description { get; set; }
    
    /// <summary>
    ///     The prompt to add to the request.
    /// </summary>
    public string Prompt { get; set; }
}