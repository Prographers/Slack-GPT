namespace Slack_GPT_Socket.Settings;

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
    
    /// <summary>
    ///     Should this prompt be added as a system prompt?
    /// </summary>
    public bool AsSystem { get; set; }
}