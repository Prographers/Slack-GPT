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
    public string Name => "-command";
    
    /// <inheritdoc />
    public bool CanHandle(ParameterEventArgs args)
    {
        return _userCommandDb.FindCommand(args.Name.ToLower()) != null;
    }

    /// <inheritdoc />
    public void Resolve(GptRequest input, ParameterEventArgs args)
    {
        var command = _userCommandDb.FindCommand(args.Name.ToLower());
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