using OpenAI.Chat;
using Slack_GPT_Socket;
using Slack_GPT_Socket.GptApi;
using Slack_GPT_Socket.Settings;

namespace Slack_GPT_Tests.GptApi;

public static class GptClientResolverTestsUtils
{
    /// <summary>
    ///     Resolves a list of strings to a list of chat prompts and a GPT request. This is only for testing purposes.
    /// </summary>
    /// <param name="resolver"></param>
    /// <param name="prompts"></param>
    /// <returns></returns>
    public static ChatRequest TestParseRequest(this GptClientResolver resolver, params (string user, string prompt)[] prompts)
    {
        var gptRequest = GptRequest.Default(new GptDefaults());
        var chatPrompts = new List<WritableChatPrompt>();
        foreach (var (user, prompt) in prompts)
        {
            chatPrompts.Add(new WritableChatPrompt(user, prompt));
        }
        gptRequest.Prompt = chatPrompts.Last(chatPrompt => chatPrompt.Role == "user").Content;
        return resolver.ParseRequest(chatPrompts, gptRequest);
    }
    
    /// <summary>
    ///     Resolves a list of strings to a list of chat prompts and a GPT request.
    /// </summary>
    /// <param name="prompts"></param>
    /// <param name="gptDefaults"></param>
    /// <param name="chatPrompts"></param>
    /// <param name="request"></param>
    public static void ResolveStringArrayToPromptsAndRequest(List<string> prompts,
        GptDefaults gptDefaults,
        out List<WritableChatPrompt> chatPrompts,
        out GptRequest request)
    {
        var context = new List<WritableChatPrompt>();
        var botMention = "@GPT";
        foreach (var prompt in prompts)
        {
            if (prompt.StartsWith(botMention))
            {
                var promptText = prompt.Replace(botMention, string.Empty).Trim();
                context.Add(new WritableChatPrompt("assistant", promptText));
            }
            else context.Add(new WritableChatPrompt("user", prompt));
        }

        chatPrompts = context;
        var userPrompt = chatPrompts.Last(chatPrompt => chatPrompt.Role == "user");
        request = GptRequest.Default(gptDefaults);
        request.UserId = "test";
        request.Prompt = userPrompt.Content;
    }
}