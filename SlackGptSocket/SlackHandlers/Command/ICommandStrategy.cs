using SlackNet.Interaction;

namespace SlackGptSocket.SlackHandlers.Command;

/// <summary>
///     Interface for a strategy that will handle commands eg: /gpt help
/// </summary>
public interface ICommandStrategy
{
    /// <summary>
    ///     Command to execute
    /// </summary>
    string Command { get; }

    /// <summary>
    ///     Can this command be handled by this strategy?
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    bool CanHandle(SlashCommand command)
    {
        return command.Text.ToLower().StartsWith(Command.ToLower());
    }

    /// <summary>
    ///     Execute the command
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    Task<SlashCommandResponse> Execute(SlashCommand command);
}