namespace Slack_GPT_Socket;

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
    public bool ShoudReplace { get; private set; }

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
        ShoudReplace = true;
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