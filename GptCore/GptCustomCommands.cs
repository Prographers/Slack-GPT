using SlackGptSocket.Settings;

namespace SlackGptSocket.GptApi;

/// <summary>
///     Custom pre-defined commands for the bot. These are defined in the appsettings.json file.
/// </summary>
public class GptCustomCommands
{
    /// <summary>
    ///     The custom commands defined in the appsettings.json file.
    /// </summary>
    private readonly GptCommands? _gptCommands;

    public GptCustomCommands(GptCommands? gptCommands)
    {
        _gptCommands = gptCommands;
    }

    /// <summary>
    ///     Returns the custom commands.
    /// </summary>
    public GptCommands Commands => _gptCommands;

    /// <summary>
    ///     Checks if the command is a custom command and returns the prompt if it is.
    /// </summary>
    /// <param name="command">Command to resolve</param>
    /// <param name="result">GptCommand object</param>
    /// <returns>True if found</returns>
    public bool TryResolveCommand(string command, out GptCommand? result)
    {
        result = null;
        foreach (var gptCommand in _gptCommands.Commands)
        {
            if (!string.Equals(gptCommand.Command, command, StringComparison.CurrentCultureIgnoreCase)) continue;

            result = gptCommand;
            return true;
        }

        return false;
    }

    /// <summary>
    ///     Returns a string containing all the custom commands and their descriptions.
    /// </summary>
    /// <returns></returns>
    public string GetHelp()
    {
        var help = "Available custom commands for the bot:";
        if (_gptCommands?.Commands == null)
            return help + "\nNo custom commands found. Please add them to the appsettings.json config.";
        foreach (var gptCommand in _gptCommands.Commands)
        {
            help += $"\n{gptCommand.Command} - {gptCommand.Description}";
        }

        return help;
    }
}