namespace Slack_GPT_Socket.GptApi.ParameterResolvers;

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
    public string Name => "-command";

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