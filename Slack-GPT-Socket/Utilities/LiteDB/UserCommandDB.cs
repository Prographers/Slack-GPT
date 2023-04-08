using LiteDB;
using Slack_GPT_Socket.Settings;

namespace Slack_GPT_Socket.Utilities.LiteDB;

public interface IUserCommandDb
{
    GptUserCommand? FindCommand(string command, string? userId = null);
}

public class UserCommandDb : IUserCommandDb
{
    private readonly ILiteDatabase _database;
    private readonly ILiteCollection<GptUserCommand> _commands;

    public UserCommandDb(ILiteDatabase database)
    {
        _database = database;
        _commands = database.GetCollection<GptUserCommand>("Commands");
        _commands.EnsureIndex(x => x.Command);
    }

    public GptUserCommand? FindCommand(string command, string? userId = null)
    {
        var result = _commands.FindOne(x => x.Command == command
                                            && (x.UserId == userId || x.UserId == null));
        return result;
    }
}