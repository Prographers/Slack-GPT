using OpenAI;

namespace Slack_GPT_Socket;

/// <summary>
///     Represents a response from the ChatGPT API.
/// </summary>
public class GptResponse
{
    /// <summary>
    ///     Gets or sets the generated message (optional).
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    ///     Gets or sets the model used for generating the response.
    /// </summary>
    public string Model { get; set; }

    /// <summary>
    ///     Gets or sets the error message, if any (optional).
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    ///     Gets or sets the usage information (optional).
    /// </summary>
    public Usage? Usage { get; set; }

    /// <summary>
    ///     Gets or sets the processing time of the response (optional).
    /// </summary>
    public TimeSpan? ProcessingTime { get; set; }
}