using LiteDB;
using Slack_GPT_Socket.Settings;

namespace Slack_GPT_Socket.Utilities.LiteDB;

public class UserCommandDb : IUserCommandDb
{
    private readonly ILiteDatabase _database;
    private readonly ILiteCollection<GptUserCommand> _commands;

    public UserCommandDb(ILiteDatabase database)
    {
        _database = database;
        _commands = database.GetCollection<GptUserCommand>("Commands");
        _commands.EnsureIndex(x => x.Command);
        _commands.EnsureIndex(x => x.UserId);
    }
    
    /// <inheritdoc />
    public GptUserCommand? FindCommand(string command, string? userId = null)
    {
        var userCommand = _commands.FindOne(x => x.Command == command && x.UserId == userId);
        var globalCommand = _commands.FindOne(x => x.Command == command && x.UserId == null);
        return userCommand ?? globalCommand;
    }

    /// <inheritdoc />
    public bool AddCommand(GptUserCommand command)
    {
        var dbCommand = FindCommand(command.Command, command.UserId);
        if (dbCommand != null) return false;
        _commands.Insert(command);
        return true;
    }

    /// <inheritdoc />
    public void RemoveCommand(GptUserCommand command)
    {
        // Find the command in the database to ensure that the ID is correct.
        var dbCommand = FindCommand(command.Command, command.UserId);
        
        // If we do that, first remove command will remove local commands, then global commands.
        // Instead all global and user at once.
        _commands.DeleteMany(x => x.Command == dbCommand.Command && x.UserId == dbCommand.UserId);
    }

    /// <inheritdoc />
    public GptUserCommand[] GetAllCommands(string userId = null)
    {
        return _commands.Find(x => x.UserId == userId).ToArray();
    }
}