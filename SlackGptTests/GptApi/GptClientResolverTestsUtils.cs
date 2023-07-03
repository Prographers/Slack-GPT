using OpenAI.Chat;
using SlackGptSocket.GptApi;
using SlackGptSocket.Settings;

namespace SlackGptTests.GptApi;

public static class GptClientResolverTestsUtils
{
    /// <summary>
    ///     Resolves a list of strings to a list of chat prompts and a GPT request. This is only for testing purposes.
    /// </summary>
    /// <param name="resolver"></param>
    /// <param name="userId"></param>
    /// <param name="prompts"></param>
    /// <returns></returns>
    public static ChatRequest TestParseRequest(this GptClientResolver resolver, string? userId,
        params (string user, string prompt)[] prompts)
    {
        var gptRequest = GptRequest.Default(new GptDefaults());
        var chatPrompts = new List<WritableChatPrompt>();
        foreach (var (user, prompt) in prompts)
        {
            chatPrompts.Add(new WritableChatPrompt(user, userId, prompt));
        }

        gptRequest.UserId = userId;
        gptRequest.Prompt = chatPrompts.Last(chatPrompt => chatPrompt.Role == "user").Content;
        return resolver.ParseRequest(chatPrompts, gptRequest);
    }
    
    /// <summary>
    ///     Resolves a list of strings to a list of chat prompts and a GPT request. This is only for testing purposes.
    ///     By default, the user ID is "U123ID".
    /// </summary>
    /// <param name="resolver"></param>
    /// <param name="prompts"></param>
    /// <returns></returns>
    public static ChatRequest TestParseRequest(this GptClientResolver resolver,
        params (string user, string prompt)[] prompts)
    {
        return TestParseRequest(resolver, "U123ID", prompts);
    }

}