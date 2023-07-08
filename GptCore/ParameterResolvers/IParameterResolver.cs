using GptCore.ParameterResolvers.Common;
using GptCore.Settings;

namespace GptCore.ParameterResolvers;

/// <summary>
///     Represents a parameter resolver for a GPT request.
/// </summary>
public interface IParameterResolver
{
    /// <summary>
    ///     All names that the resolver can handle.
    /// </summary>
    static virtual string[] Names { get; } = Array.Empty<string>();
    
    /// <summary>
    ///     Main name of the resolver.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     Short help text for the resolver. Bundled in general help command.
    /// </summary>
    /// <param name="gptDefaults"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    string BuildShortHelpText(GptDefaults gptDefaults, string userId);
    
    /// <summary>
    ///     Help text for the resolver. Showed when details of command are requested.
    /// </summary>
    string BuildHelpText(GptDefaults gptDefaults, string commandName, string userId);
    
    /// <summary>
    ///     Can this resolver handle the given parameter name?
    /// </summary>
    /// <param name="args">Arguments of the resolver</param>
    /// <returns>True if can resolve</returns>
    bool CanHandle(ParameterEventArgs args);

    /// <summary>
    ///     Resolves the parameter.
    /// </summary>
    /// <param name="input">Request input</param>
    /// <param name="args">Arguments of the resolver</param>
    void Resolve(GptRequest input, ParameterEventArgs args);
}
