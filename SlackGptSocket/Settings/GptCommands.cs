namespace SlackGptSocket.Settings;

/// <summary>
///     Custom pre-defined commands for the bot.
/// </summary>
public class GptCommands
{
    /// <summary>
    ///     The custom commands.
    /// </summary>
    public List<GptCommand> Commands { get; set; } = new();
}