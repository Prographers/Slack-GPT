namespace Slack_GPT_Socket.GptApi;

/// <summary>
///     Represents a writable chat prompt used for generating AI responses.
/// </summary>
public sealed class WritableChatPrompt
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="WritableChatPrompt" /> class.
    /// </summary>
    /// <param name="role">The role of the chat prompt (e.g., "user" or "system").</param>
    /// <param name="content">The content of the chat prompt.</param>
    public WritableChatPrompt(string role, string content)
    {
        Role = role;
        Content = content;
    }

    /// <summary>
    ///     Gets or sets the role of the chat prompt.
    /// </summary>
    public string Role { get; set; }

    /// <summary>
    ///     Gets or sets the content of the chat prompt.
    /// </summary>
    public string Content { get; set; }
}