namespace Slack_GPT_Socket.GptApi;

/// <summary>
///    The result of the call expression.
/// </summary>
public class CallExpressionResult
{
    /// <summary>
    ///     Plain text response.
    /// </summary>
    public string? TextResponse { get; set; }
    
    /// <summary>
    ///     List of images 
    /// </summary>
    public List<FileAttachment> Files { get; set; } = new();
}