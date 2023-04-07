using System.Globalization;

namespace Slack_GPT_Socket.GptApi.ParameterResolvers;

/// <summary>
///     Represents a parameter resolver for the top-p parameter.
/// </summary>
public class TopPResolver : IParameterResolver
{
    /// <inheritdoc />
    public string Name => "-topP";

    /// <inheritdoc />
    public bool CanHandle(ParameterEventArgs args)
    {
        var names = new[]
        {
            "-top_p",
            "-top-p",
            "-topp",
            "-top-p",
            "-top_p"
        };
        return names.Contains(args.Name.ToLower());
    }

    /// <inheritdoc />
    public void Resolve(GptRequest input, ParameterEventArgs args)
    {
        var valueSanitized = args.Value.SanitizeNumber();
        if (float.TryParse(valueSanitized, CultureInfo.InvariantCulture, out var topP))
            input.TopP = topP;
    }
}