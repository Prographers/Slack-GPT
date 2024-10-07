using OpenAI;

namespace Slack_GPT_Socket.GptApi;

public enum Role
{
    User,
    Assistant,
    System,
    Tool,
}

/// <summary>
///     Represents a writable chat prompt used for generating AI responses.
/// </summary>
public sealed class WritableMessage
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="WritableMessage" /> class.
    /// </summary>
    /// <param name="role">The role of the chat prompt (e.g., "user" or "system").</param>
    /// <param name="userId">The userID that sent the message</param>
    /// <param name="content">The content of the chat prompt.</param>
    public WritableMessage(Role role, string userId, string content)
    {
        UserId = userId;
        Role = role;
        Content = content;
    }

    /// <summary>
    ///     Gets or sets the role of the chat prompt.
    /// </summary>
    public Role Role { get; set; }
    
    /// <summary>
    ///     Gets or sets the user identifier, that sent the message.
    /// </summary>
    public string UserId { get; set; }

    /// <summary>
    ///     Gets or sets the content of the chat prompt.
    /// </summary>
    public string Content { get; set; }
}