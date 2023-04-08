namespace Slack_GPT_Socket.GptApi.ParameterResolvers;

/// <summary>
///     Represents a parameter resolver for the context parameter.
///     Context parameter is passed to the next request, unless it is cleared.
/// </summary>
public class ContextResolver : IParameterResolver
{
    /// <inheritdoc />
    public string Name => "-context";

    /// <inheritdoc />
    public bool CanHandle(ParameterEventArgs args)
    {
        var names = new[]
        {
            "-context"
        };
        return names.Contains(args.Name.ToLower());
    }

    /// <inheritdoc />
    public void Resolve(GptRequest input, ParameterEventArgs args)
    {
        if (args.Value is "clear" or "null" or "none" or "reset" or "empty" or "")
        {
            input.System.IsContextMessage = ContextMessageStatus.Cleared;
            return;
        }

        input.System.IsContextMessage = ContextMessageStatus.Set;
        input.System.Replace(args.Value);
    }
}