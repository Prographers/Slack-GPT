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
    public void AddCommand(GptUserCommand command)
    {
        _commands.Insert(command);
    }

    /// <inheritdoc />
    public void RemoveCommand(GptUserCommand command)
    {
        _commands.DeleteMany(x => x.Command == command.Command && x.UserId == command.UserId);
    }

    /// <inheritdoc />
    public GptUserCommand[] GetAllCommands(string userId = null)
    {
        return _commands.Find(x => x.UserId == userId).ToArray();
    }
}