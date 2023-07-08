using System.Globalization;
using GptCore.ParameterResolvers.Common;
using GptCore.Settings;
using GptCore.Utilities;

namespace GptCore.ParameterResolvers;

/// <summary>
///     Represents a parameter resolver for the presence penalty parameter.
/// </summary>
public class PresencePenaltyResolver : IParameterResolver
{
    /// <inheritdoc />
    public static string[] Names { get; } =
    {
        "-presence_penalty",
        "-presence-penalty",
        "-presencepenalty",
    };

    /// <inheritdoc />
    public string Name => "-presencePenalty";

    /// <inheritdoc />
    public string BuildShortHelpText(GptDefaults gptDefaults, string userId)
    {
        return $"{Name}: penalizes repeated tokens, default {gptDefaults.PresencePenalty?.ToString() ?? "0"}";
    }

    /// <inheritdoc />
    public string BuildHelpText(GptDefaults gptDefaults, string commandName, string userId)
    {
        return $"{Name}: penalizes repeated tokens, default {gptDefaults.PresencePenalty?.ToString() ?? "0"}";
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
        if (float.TryParse(valueSanitized, CultureInfo.InvariantCulture, out var presencePenalty)) 
            input.PresencePenalty = presencePenalty;
    }
}