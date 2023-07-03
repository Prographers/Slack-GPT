namespace SlackGptSocket.GptApi.ParameterResolvers;

/// <summary>
///     Arguments of a parameter resolver.
/// </summary>
public class ParameterEventArgs : EventArgs
{
    /// <summary>
    ///     Name of the parameter to resolve
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    ///     User ID of the user who sent the request
    /// </summary>
    public required string UserId { get; init; }
    
    /// <summary>
    ///     Value of the parameter to resolve
    /// </summary>
    public required string Value { get; init; }
    
    /// <summary>
    ///     Raw value of the parameter to resolve with or without double quotes
    /// </summary>
    public required string ValueRaw { get; init; }

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