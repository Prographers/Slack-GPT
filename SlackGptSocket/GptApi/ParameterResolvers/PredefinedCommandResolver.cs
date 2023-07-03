using System.Text;
using SlackGptSocket.Settings;

namespace SlackGptSocket.GptApi.ParameterResolvers;

/// <summary>
///     Represents a parameter resolver for the command parameter predefined in appsettings.json.
/// </summary>
public class PredefinedCommandResolver : IParameterResolver
{
    private readonly GptCustomCommands _customCommands;

    public PredefinedCommandResolver(GptCustomCommands customCommands)
    {
        _customCommands = customCommands;
    }

    /// <inheritdoc />
    public static string[] Names { get; } = { "-command" };

    /// <inheritdoc />
    public string Name => "-command";

    /// <inheritdoc />
    public string BuildShortHelpText(GptDefaults gptDefaults, string userId)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Predefined commands:");
        foreach (var command in _customCommands.Commands.Commands)
        {
            if (command.AsSystem) sb.AppendLine("[S]");
            sb.AppendLine($"\t{command.Command}: {command.Description}");
        }

        if (_customCommands.Commands.Commands.Count == 0)
        {
            sb.AppendLine("\tNo predefined commands found.");
        }

        return sb.ToString();
    }

    /// <inheritdoc />
    public string BuildHelpText(GptDefaults gptDefaults, string commandName, string userId)
    {
        if (_customCommands.TryResolveCommand(commandName, out var command))
        {
            return $"{command!.Command} - {command.Description}" +
                   $"\nPrompt:\n> {command.Prompt}" +
                   $"\n\nIs executed as system command: {command.AsSystem}";
        }

        return $"Command {commandName} not found.";

    }

    /// <inheritdoc />
    public bool CanHandle(ParameterEventArgs args)
    {
        var commands = _customCommands.Commands.Commands;
        return commands.Select(x => x.Command.ToLower()).Contains(args.Name.ToLower());
    }

    /// <inheritdoc />
    public void Resolve(GptRequest input, ParameterEventArgs args)
    {
        if (!_customCommands.TryResolveCommand(args.Name, out var prompt)) return;
        
        if (prompt!.AsSystem)
        {
            input.System.Append(prompt.Prompt);
        }
        else
        {
            input.Prompt = prompt.Prompt + "\n" + input.Prompt;
        }

        args.HasValue = false;
    }
}