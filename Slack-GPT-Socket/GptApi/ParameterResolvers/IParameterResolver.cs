namespace Slack_GPT_Socket.GptApi.ParameterResolvers;

/// <summary>
///     Represents a parameter resolver for a GPT request.
/// </summary>
public interface IParameterResolver
{
    /// <summary>
    ///     Main name of the resolver.
    /// </summary>
    string Name { get; }
    
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
