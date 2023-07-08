using OpenAI.Chat;

namespace GptCore;

/// <summary>
///     Represents a response from the ChatGPT API.
/// </summary>
public interface IGptClientResolver
{
    /// <summary>
    ///     Parses the given request and generates a ChatRequest instance.
    /// </summary>
    /// <param name="chatPrompts">The list of chat prompts.</param>
    /// <param name="request">The GPT request.</param>
    /// <returns>A ChatRequest instance.</returns>
    ChatRequest ParseRequest(List<WritableChatPrompt> chatPrompts, GptRequest request);

    /// <summary>
    ///     Resolves the parameters in the given GPT request.
    /// </summary>
    /// <param name="chatRequest"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ChatResponse> GetCompletionAsync(
        ChatRequest chatRequest, 
        CancellationToken cancellationToken = default(CancellationToken));
}