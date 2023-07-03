namespace SlackGptSocket.Settings;

/// <summary>
///     A custom command.
/// </summary>
public class GptCommand
{
    private string _command;

    /// <summary>
    ///     The command to trigger the custom command.
    /// </summary>
    public string Command
    {
        get => _command;
        set => _command = value?.ToLower();
    }

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

/// <summary>
///     A custom command that can only be used by a specific user.
/// </summary>
public class GptUserCommand : GptCommand
{
    /// <summary>
    ///     The user who created the command. If null, everyone can use it.
    /// </summary>
    public string? UserId { get; set; }
}