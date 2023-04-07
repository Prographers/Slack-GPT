namespace Slack_GPT_Socket.Settings;

/// <summary>
///     Default values for the GPT API.
/// </summary>
public class GptDefaults
{
    /// <summary>
    ///     Default value for max tokens for the response
    /// </summary>
    public int? MaxTokens { get; set; }
    
    /// <summary>
    ///     Default value for temperature for the response
    /// </summary>
    public float? Temperature { get; set; }
    
    /// <summary>
    ///     Default value for Top-P sampling for the response
    /// </summary>
    public float? TopP { get; set; }
    
    /// <summary>
    ///     Default value for presence penalty for the response
    /// </summary>
    public float? PresencePenalty { get; set; }
    
    /// <summary>
    ///     Default value for frequency penalty for the response
    /// </summary>
    public float? FrequencyPenalty { get; set; }
    
    /// <summary>
    ///     Default value for model used for the response
    /// </summary>
    public string? Model { get; set; }
    
    /// <summary>
    ///     Default value for system identifier (optional)
    /// </summary>
    public string? System { get; set; }
}