using System.Globalization;
using GptCore.Utilities;
using SlackGptSocket.Settings;

namespace SlackGptSocket.GptApi.ParameterResolvers;

/// <summary>
///     Represents a parameter resolver for the top-p parameter.
/// </summary>
public class TopPResolver : IParameterResolver
{
    /// <inheritdoc />
    public static string[] Names { get; } =
    {
        "-top_p",
        "-top-p",
        "-topp"
    };

    /// <inheritdoc />
    public string Name => "-topP";

    /// <inheritdoc />
    public string BuildShortHelpText(GptDefaults gptDefaults, string userId)
    {
        return $"{Name}: filters token choices, default {gptDefaults.TopP?.ToString() ?? "1"}";
    }

    /// <inheritdoc />
    public string BuildHelpText(GptDefaults gptDefaults, string commandName, string userId)
    {
        return $"{Name}: filters token choices, default {gptDefaults.TopP?.ToString() ?? "1"}";
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
        if (float.TryParse(valueSanitized, CultureInfo.InvariantCulture, out var topP))
            input.TopP = topP;
    }
}