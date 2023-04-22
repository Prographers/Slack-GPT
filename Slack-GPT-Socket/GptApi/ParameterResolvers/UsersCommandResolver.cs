using System.Text;
using Slack_GPT_Socket.Settings;
using Slack_GPT_Socket.Utilities.LiteDB;

namespace Slack_GPT_Socket.GptApi.ParameterResolvers;

/// <summary>
///     Represents a parameter resolver for user-defined commands.
/// </summary>
public class UsersCommandResolver : IParameterResolver
{
    private readonly IUserCommandDb _userCommandDb;

    public UsersCommandResolver(IUserCommandDb userCommandDb)
    {
        _userCommandDb = userCommandDb;
    }

    /// <inheritdoc />
    public static string[] Names { get; } = { "-command" };

    /// <inheritdoc />
    public string Name => "-command";

    /// <inheritdoc />
    public string BuildShortHelpText(GptDefaults gptDefaults, string userId)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Global commands:");
        var globalCommands = _userCommandDb.GetAllCommands();
        foreach (var command in globalCommands)
        {
            if (command.AsSystem) sb.AppendLine("[S]");
            sb.AppendLine($"\t{command.Command}: {command.Description}");
        }
        
        if (!globalCommands.Any())
        {
            sb.AppendLine("\tNo predefined global commands found.");
        }
        
        sb.AppendLine("Your personal commands:");
        var userCommands = _userCommandDb.GetAllCommands(userId);
        foreach (var command in userCommands)
        {
            if (command.AsSystem) sb.AppendLine("[S]");
            sb.AppendLine($"\t{command.Command}: {command.Description}");
        }
        
        if (!userCommands.Any())
        {
            sb.AppendLine("\tNo predefined personal commands found.");
        }

        return sb.ToString();
    }

    /// <inheritdoc />
    public string BuildHelpText(GptDefaults gptDefaults, string commandName, string userId)
    {
        var command = _userCommandDb.FindCommand(commandName.ToLower(), userId);
        if (command == null) return $"Command {commandName} not found.";
        
        return $"{command.Command} - {command.Description}" +
               $"\nPrompt:\n> {command.Prompt}" +
               $"\n\nIs executed as system command: {command.AsSystem}";
    }

    /// <inheritdoc />
    public bool CanHandle(ParameterEventArgs args)
    {
        return _userCommandDb.FindCommand(args.Name.ToLower(), args.UserId) != null;
    }

    /// <inheritdoc />
    public void Resolve(GptRequest input, ParameterEventArgs args)
    {
        var command = _userCommandDb.FindCommand(args.Name.ToLower(), args.UserId);
        args.HasValue = false;
        
        if (command == null) return;
        
        if (command.AsSystem)
        {
            input.System.Append(command.Prompt);
        }
        else
        {
            input.Prompt = command.Prompt + "\n" + input.Prompt;
        }

    }
}