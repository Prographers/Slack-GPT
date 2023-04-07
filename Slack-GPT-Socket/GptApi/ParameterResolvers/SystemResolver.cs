namespace Slack_GPT_Socket.GptApi.ParameterResolvers;

/// <summary>
///     Represents a parameter resolver for the system parameter.
/// </summary>
public class SystemResolver : IParameterResolver
{
    /// <inheritdoc />
    public string Name => "-system";

    /// <inheritdoc />
    public bool CanHandle(ParameterEventArgs args)
    {
        var names = new[]
        {
            "-system",
            "-s"
        };
        return names.Contains(args.Name.ToLower());
    }

    /// <inheritdoc />
    public void Resolve(GptRequest input, ParameterEventArgs args)
    {
        input.System.Replace(args.Value);
    }
}