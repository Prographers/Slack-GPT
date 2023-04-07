using System.Globalization;

namespace Slack_GPT_Socket.GptApi.ParameterResolvers;

/// <summary>
///     Represents a parameter resolver for the temperature parameter.
/// </summary>
public class TemperatureResolver : IParameterResolver
{
    /// <inheritdoc />
    public string Name => "-temperature";

    /// <inheritdoc />
    public bool CanHandle(ParameterEventArgs args)
    {
        var names = new[]
        {
            "-temperature",
            "-temp",
            "-t"
        };
        return names.Contains(args.Name.ToLower());
    }

    /// <inheritdoc />
    public void Resolve(GptRequest input, ParameterEventArgs args)
    {
        var valueSanitized = args.Value.SanitizeNumber();
        if (float.TryParse(valueSanitized, CultureInfo.InvariantCulture, out var temperature))
            input.Temperature = temperature;
    }
}