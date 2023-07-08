using System.Globalization;
using GptCore.ParameterResolvers.Common;
using GptCore.Settings;
using GptCore.Utilities;

namespace GptCore.ParameterResolvers;

/// <summary>
///     Represents a parameter resolver for the frequency penalty parameter.
/// </summary>
public class FrequencyPenaltyResolver : IParameterResolver
{
    /// <inheritdoc />
    public static string[] Names { get; } =
    {
        "-frequency_penalty",
        "-frequency-penalty",
        "-frequencypenalty",
    };

    /// <inheritdoc />
    public string Name => "-frequencyPenalty";

    /// <inheritdoc />
    public string BuildShortHelpText(GptDefaults gptDefaults, string userId)
    {
        return $"{Name}: discourages frequent tokens, default {gptDefaults.FrequencyPenalty?.ToString() ?? "0"}";
    }

    /// <inheritdoc />
    public string BuildHelpText(GptDefaults gptDefaults, string commandName, string userId)
    {
        return $"{Name}: discourages frequent tokens, default {gptDefaults.FrequencyPenalty?.ToString() ?? "0"}";
    }

    /// <inheritdoc />
    public bool CanHandle(ParameterEventArgs args)
    {
        return Names.Contains(args.Name.ToLower());
    }

    /// <inheritdoc />
    public void Resolve(GptRequest input, ParameterEventArgs args)
    {
        var valueSanitized = args.Value.SanitizeNumber();
        if (float.TryParse(valueSanitized, CultureInfo.InvariantCulture, out var frequencyPenalty)) 
            input.FrequencyPenalty = frequencyPenalty;
    }
}