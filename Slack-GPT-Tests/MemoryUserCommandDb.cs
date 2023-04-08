using Slack_GPT_Socket.Settings;
using Slack_GPT_Socket.Utilities.LiteDB;

namespace Slack_GPT_Tests;

public class MemoryUserCommandDb : IUserCommandDb
{
    public List<GptUserCommand> Commands { get; set; } = new();

    public GptUserCommand? FindCommand(string command, string? userId = null)
    {
        return Commands.FirstOrDefault(x => string.Equals(x.Command, command,
            StringComparison.InvariantCultureIgnoreCase) && (x.UserId == userId || x.UserId == null));
    }
}