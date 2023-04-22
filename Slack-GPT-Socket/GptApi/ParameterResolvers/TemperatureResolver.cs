using System.Globalization;
using Slack_GPT_Socket.Settings;

namespace Slack_GPT_Socket.GptApi.ParameterResolvers;

/// <summary>
///     Represents a parameter resolver for the temperature parameter.
/// </summary>
public class TemperatureResolver : IParameterResolver
{
    /// <inheritdoc />
    public static string[] Names { get; } =
    {
        "-temperature",
        "-temp",
        "-t"
    };

    /// <inheritdoc />
    public string Name => "-temperature";

    /// <inheritdoc />
    public string BuildShortHelpText(GptDefaults gptDefaults, string userId)
    {
        return $"{Name}: controls randomness, default {gptDefaults.Temperature?.ToString() ?? "0.7"}";
    }

    /// <inheritdoc />
    public string BuildHelpText(GptDefaults gptDefaults, string commandName, string userId)
    {
        var names = string.Join(", ", Names);
        return $"{names}: controls randomness, default {gptDefaults.Temperature?.ToString() ?? "0.7"}";
    }

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