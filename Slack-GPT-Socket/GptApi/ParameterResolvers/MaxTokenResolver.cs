using System.Globalization;

namespace Slack_GPT_Socket.GptApi.ParameterResolvers;

/// <summary>
///     Represents a parameter resolver for the max tokens parameter.
/// </summary>
public class MaxTokenResolver : IParameterResolver
{
    /// <inheritdoc />
    public string Name => "-maxTokens";

    /// <inheritdoc />
    public bool CanHandle(ParameterEventArgs args)
    {
        var names = new[]
        {
            "-max_tokens",
            "-max-tokens",
            "-maxtokens",
            "-maxtoken",
            "-max-token",
            "-max_token"
        };
        return names.Contains(args.Name.ToLower());
    }

    /// <inheritdoc />
    public void Resolve(GptRequest input, ParameterEventArgs args)
    {
        var valueSanitized = args.Value.SanitizeNumber();
        if (int.TryParse(valueSanitized, CultureInfo.InvariantCulture, out var maxTokens)) 
            input.MaxTokens = maxTokens;
    }
}