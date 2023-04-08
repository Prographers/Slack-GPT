namespace Slack_GPT_Socket.GptApi;

/// <summary>
///     Represents a status of the context message.
/// </summary>
public enum ContextMessageStatus
{
    /// <summary>
    ///     Context message is not set.
    /// </summary>
    None,
    /// <summary>
    ///     Context message is set, but not yet cleared.
    /// </summary>
    Set,
    /// <summary>
    ///     Context message is set, and cleared.
    /// </summary>
    Cleared
}