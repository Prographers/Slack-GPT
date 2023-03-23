namespace Slack_GPT_Socket;

/// <summary>
///     Represents a request to the ChatGPT API.
/// </summary>
public class GptRequest
{
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
    public string? System { get; set; }
}