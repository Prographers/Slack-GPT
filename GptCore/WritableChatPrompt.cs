using OpenAI.Chat;

namespace GptCore;

/// <summary>
///     Represents a writable chat prompt used for generating AI responses.
/// </summary>
/// <remarks>
///     This class is an open replacement for <see cref="ChatPrompt"/> from <see cref="OpenAI.Chat"/> to allow for
///     in-flight modifications via property setters.
/// </remarks>
public sealed class WritableChatPrompt
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="WritableChatPrompt" /> class.
    /// </summary>
    /// <param name="role">The role of the chat prompt (e.g., "user" or "system").</param>
    /// <param name="userId">The userID that sent the message</param>
    /// <param name="content">The content of the chat prompt.</param>
    public WritableChatPrompt(string role, string userId, string content)
    {
        UserId = userId;
        Role = role;
        Content = content;
    }

    /// <summary>
    ///     Gets or sets the role of the chat prompt.
    /// </summary>
    public string Role { get; set; }
    
    /// <summary>
    ///     Gets or sets the user identifier, that sent the message.
    /// </summary>
    public string UserId { get; set; }

    /// <summary>
    ///     Gets or sets the content of the chat prompt.
    /// </summary>
    public string Content { get; set; }
}