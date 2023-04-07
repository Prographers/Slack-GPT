using Slack_GPT_Socket.Settings;

namespace Slack_GPT_Socket.GptApi;

/// <summary>
///     Represents a request to the ChatGPT API.
/// </summary>
public class GptRequest
{
    /// <summary>
    ///     Creates a new GPT request with default values.
    /// </summary>
    /// <param name="defaults"></param>
    /// <returns></returns>
    public static GptRequest Default(GptDefaults defaults)
    {
        var request = new GptRequest();
        if (defaults.MaxTokens.HasValue)
            request.MaxTokens = defaults.MaxTokens.Value;
        if (defaults.Temperature.HasValue)
            request.Temperature = defaults.Temperature.Value;
        if (defaults.TopP.HasValue)
            request.TopP = defaults.TopP.Value;
        if (defaults.PresencePenalty.HasValue)
            request.PresencePenalty = defaults.PresencePenalty.Value;
        if (defaults.FrequencyPenalty.HasValue)
            request.FrequencyPenalty = defaults.FrequencyPenalty.Value;
        if (defaults.Model != null)
            request.Model = defaults.Model;
        if (defaults.System != null)
            request.System.Replace(defaults.System);
        return request;
    }
    
    /// <summary>
    ///     Hide the default constructor.
    /// </summary>
    private GptRequest(){}
    
    /// <summary>
    ///     Gets or sets the user identifier.
    /// </summary>
    public string UserId { get; set; }

    /// <summary>
    ///     Gets or sets the chat prompt.
    /// </summary>
    public string Prompt { get; set; }

    /// <summary>
    ///     Gets or sets the maximum number of tokens in the generated response.
    /// </summary>
    public int MaxTokens { get; set; } = 4000;

    /// <summary>
    ///     Gets or sets the temperature for randomness in the response.
    /// </summary>
    public float Temperature { get; set; } = 0.7f;

    /// <summary>
    ///     Gets or sets the Top-P sampling value for the response.
    /// </summary>
    public float TopP { get; set; } = 1f;

    /// <summary>
    ///     Gets or sets the presence penalty for the generated response.
    /// </summary>
    public float PresencePenalty { get; set; } = 0f;

    /// <summary>
    ///     Gets or sets the frequency penalty for the generated response.
    /// </summary>
    public float FrequencyPenalty { get; set; } = 0f;

    /// <summary>
    ///     Gets or sets the model used for generating the response.
    /// </summary>
    public string Model { get; set; } = "gpt-4";

    /// <summary>
    ///     Gets or sets the system identifier (optional).
    /// </summary>
    public GptSystemMessageBuilder System { get; set; } = new();
}