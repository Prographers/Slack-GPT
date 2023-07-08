using GptCore.ParameterResolvers.Common;
using GptCore.Settings;

namespace GptCore.ParameterResolvers;

/// <summary>
///     Represents a parameter resolver for the system parameter.
/// </summary>
public class SystemResolver : IParameterResolver
{
    /// <inheritdoc />
    public static string[] Names { get; } =
    {
        "-system",
        "-s"
    };

    /// <inheritdoc />
    public string Name => "-system";

    /// <inheritdoc />
    public string BuildShortHelpText(GptDefaults gptDefaults, string userId)
    {
        return
            $"{Name}: custom system message, default \"{gptDefaults.Model ?? "You are a helpful assistant. Today is {Current Date}"}\".";
    }

    /// <inheritdoc />
    public string BuildHelpText(GptDefaults gptDefaults, string commandName, string userId)
    {
        return
            $"{Name}: custom system message, default \"{gptDefaults.Model ?? "You are a helpful assistant. Today is {Current Date}"}\".";
    }

    /// <inheritdoc />
    public bool CanHandle(ParameterEventArgs args)
    {
        return Names.Contains(args.Name.ToLower());
    }

    /// <inheritdoc />
    public void Resolve(GptRequest input, ParameterEventArgs args)
    {
        input.System.Replace(args.Value);
    }
}