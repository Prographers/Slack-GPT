using Slack_GPT_Socket.Settings;

namespace Slack_GPT_Socket.Utilities.LiteDB;

/// <summary>
///     A database for user-defined commands.
/// </summary>
public interface IUserCommandDb
{
    /// <summary>
    ///     Finds a command in the database.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    GptUserCommand? FindCommand(string command, string? userId = null);
    
    /// <summary>
    ///     Adds a command to the database.
    /// </summary>
    /// <param name="command"></param>
    void AddCommand(GptUserCommand command);
    
    /// <summary>
    ///     Removes a command from the database.
    /// </summary>
    /// <param name="command"></param>
    void RemoveCommand(GptUserCommand command);
    
    /// <summary>
    ///     Gets all commands in the database.
    ///     If a user ID is provided, only commands for that user will be returned.
    ///     Otherwise, global commands will be returned.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    GptUserCommand[] GetAllCommands(string userId = null);
}