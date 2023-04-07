using System.Globalization;

namespace Slack_GPT_Socket.GptApi.ParameterResolvers;

/// <summary>
///     Represents a parameter resolver for the frequency penalty parameter.
/// </summary>
public class FrequencyPenaltyResolver : IParameterResolver
{
    /// <inheritdoc />
    public string Name => "-frequencyPenalty";

    /// <inheritdoc />
    public bool CanHandle(ParameterEventArgs args)
    {
        var names = new[]
        {
            "-frequency_penalty",
            "-frequency-penalty",
            "-frequencypenalty",
        };
        return names.Contains(args.Name.ToLower());
    }

    /// <inheritdoc />
    public void Resolve(GptRequest input, ParameterEventArgs args)
    {
        var valueSanitized = args.Value.SanitizeNumber();
        if (float.TryParse(valueSanitized, CultureInfo.InvariantCulture, out var frequencyPenalty)) 
            input.FrequencyPenalty = frequencyPenalty;
    }
}