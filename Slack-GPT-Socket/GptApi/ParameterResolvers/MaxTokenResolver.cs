using System.Globalization;
using Slack_GPT_Socket.Settings;

namespace Slack_GPT_Socket.GptApi.ParameterResolvers;

/// <summary>
///     Represents a parameter resolver for the max tokens parameter.
/// </summary>
public class MaxTokenResolver : IParameterResolver
{
    /// <inheritdoc />
    public static string[] Names { get; } =
    {
        "-max_tokens",
        "-max-tokens",
        "-maxtokens",
        "-maxtoken",
        "-max-token",
        "-max_token"
    };

    /// <inheritdoc />
    public string Name => "-maxTokens";

    /// <inheritdoc />
    public string BuildShortHelpText(GptDefaults gptDefaults, string userId)
    {
        return
            $"{Name}: limits tokens in output, default {gptDefaults.MaxTokens?.ToString() ?? "4000"} (GPT-3.5: 4000, GPT-4: 8000)";
    }

    /// <inheritdoc />
    public string BuildHelpText(GptDefaults gptDefaults, string commandName, string userId)
    {
        var names = string.Join("\n", Names);
        return
            $"{names}\n\t: limits tokens in output, default {gptDefaults.MaxTokens?.ToString() ?? "4000"} (GPT-3.5: 4000, GPT-4: 8000)\n";
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
        if (int.TryParse(valueSanitized, CultureInfo.InvariantCulture, out var maxTokens)) 
            input.MaxTokens = maxTokens;
    }
}