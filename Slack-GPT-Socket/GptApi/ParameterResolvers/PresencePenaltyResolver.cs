using System.Globalization;

namespace Slack_GPT_Socket.GptApi.ParameterResolvers;

/// <summary>
///     Represents a parameter resolver for the presence penalty parameter.
/// </summary>
public class PresencePenaltyResolver : IParameterResolver
{
    /// <inheritdoc />
    public string Name => "-presencePenalty";

    /// <inheritdoc />
    public bool CanHandle(ParameterEventArgs args)
    {
        var names = new[]
        {
            "-presence_penalty",
            "-presence-penalty",
            "-presencepenalty",
        };
        return names.Contains(args.Name.ToLower());
    }

    /// <inheritdoc />
    public void Resolve(GptRequest input, ParameterEventArgs args)
    {
        var valueSanitized = args.Value.SanitizeNumber();
        if (float.TryParse(valueSanitized, CultureInfo.InvariantCulture, out var presencePenalty)) 
            input.PresencePenalty = presencePenalty;
    }
}