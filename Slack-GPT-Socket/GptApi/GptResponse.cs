using OpenAI.Chat;

namespace Slack_GPT_Socket.GptApi;

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
    ///     Gets or sets the list of file attachments (optional).
    /// </summary>
    public List<FileAttachment> FileAttachments { get; set; } = new();

    /// <summary>
    ///     Gets or sets the usage information (optional).
    /// </summary>
    public TokenUsage? Usage { get; set; }

    /// <summary>
    ///     Gets or sets the processing time of the response (optional).
    /// </summary>
    public TimeSpan? ProcessingTime { get; set; }
}

public class TokenUsage
{
    public TokenUsage(ChatTokenUsage chatTokenUsage)
    {
        Add(chatTokenUsage);
    }

    /// <summary>
    ///     The combined number of output tokens in the generated completion, as consumed by the model.
    /// </summary>
    /// <remarks>
    ///     When using a model that supports <see cref="ReasoningTokens" /> such as <c>o1-mini</c>, this value represents
    ///     the sum of those reasoning tokens and conventional, displayed output tokens.
    /// </remarks>
    public int OutputTokenCount { get; set; }

    /// <summary>
    ///     The number of tokens in the request message input, spanning all message content items.
    /// </summary>
    public int InputTokenCount { get; set; }

    /// <summary>
    ///     The total number of combined input (prompt) and output (completion) tokens used by a chat completion operation.
    /// </summary>
    public int TotalTokenCount { get; set; }

    /// <summary>
    ///     Additional information about the tokens represented by <see cref="OutputTokenCount" />, including the count of
    ///     consumed reasoning tokens by supported models.
    /// </summary>
    public DetailedTokenUsage OutputTokenDetails { get; } = new();

    /// <summary>
    ///     Adds the token usage of another <see cref="ChatTokenUsage" /> to this instance.
    /// </summary>
    /// <param name="chatTokenUsage"></param>
    /// <returns></returns>
    public void Add(ChatTokenUsage chatTokenUsage)
    {
        OutputTokenCount += chatTokenUsage.OutputTokenCount;
        InputTokenCount += chatTokenUsage.InputTokenCount;
        TotalTokenCount += chatTokenUsage.TotalTokenCount;
        OutputTokenDetails.ReasoningTokenCount += chatTokenUsage.OutputTokenDetails.ReasoningTokenCount;
    }
}

public class DetailedTokenUsage
{
    /// <summary>
    ///     The number of internally-consumed output tokens used for integrated reasoning with a supported model.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This is currently only applicable to <c>o1</c> models.
    ///     </para>
    ///     <para>
    ///         <see cref="ReasoningTokenCount" /> is part of the total <see cref="ChatTokenUsage.OutputTokenCount" /> and will
    ///         thus always be less than or equal to this parent number.
    ///     </para>
    /// </remarks>
    public int ReasoningTokenCount { get; set; }
}

public class FileAttachment
{
    /// <summary>
    ///     Image in base64 format.
    /// </summary>
    public byte[] Data { get; set; }
    public string MimeType { get; set; }
    public string Name { get; set; }
    public string Title { get; set; }
    
    public bool IsImage => MimeType.StartsWith("image/");
}