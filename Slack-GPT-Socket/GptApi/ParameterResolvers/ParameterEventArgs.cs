namespace Slack_GPT_Socket.GptApi.ParameterResolvers;

/// <summary>
///     Arguments of a parameter resolver.
/// </summary>
public class ParameterEventArgs : EventArgs
{
    /// <summary>
    ///     Name of the parameter to resolve
    /// </summary>
    public string Name { get; init; }
    
    /// <summary>
    ///     Value of the parameter to resolve
    /// </summary>
    public string Value { get; init; }
    
    /// <summary>
    ///     Raw value of the parameter to resolve with or without double quotes
    /// </summary>
    public string ValueRaw { get; init; }

    /// <summary>
    ///     Is the parameter value present?
    ///     If false, the assumed value will not be removed from the input.
    /// </summary>
    public bool HasValue { get; set; }
    
    /// <summary>
    ///     Should we find more handlers for this parameter?
    /// </summary>
    public bool PassThrough { get; set; }
}