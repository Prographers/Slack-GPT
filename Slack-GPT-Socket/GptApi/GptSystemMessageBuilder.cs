namespace Slack_GPT_Socket.GptApi;

/// <summary>
///     Represents a system message builder
/// </summary>
public class GptSystemMessageBuilder
{
    private List<string> _messages = new();

    /// <summary>
    ///     List of system messages, to append to the main prompt.
    /// </summary>
    public IReadOnlyCollection<string> Messages
    {
        get => _messages;
    }
    
    /// <summary>
    ///     Should we replace the system message with list of messages?
    /// </summary>
    public bool ShouldReplace { get; private set; }
    
    /// <summary>
    ///     Context message is treated as a system message, that is carried over to the next request.
    ///     Those can be overwritten by the next manual system request, or by the next context message.
    ///     After the next manual system request, the context message is kept until cleared manually.
    /// </summary>
    public bool IsContextMessage { get; set; }

    /// <summary>
    ///     Adds a system message to the list.
    /// </summary>
    /// <param name="message">Message to append</param>
    public void Append(string message)
    {
        _messages.Add(message);
    }

    /// <summary>
    ///     Adds a system message to the list. But marks it as a replacement for the original message.
    /// </summary>
    /// <param name="message">Message to append</param>
    public void Replace(string message)
    {
        _messages.Add(message);
        ShouldReplace = true;
    }

    /// <summary>
    ///     Builds the system message.
    /// </summary>
    /// <returns>Returns list of messages as a system message</returns>
    public string Build()
    {
        return string.Join(" ", _messages);
    }
}