using SlackNet.Blocks;
using SlackNet.Interaction;
using SlackNet.WebApi;

namespace SlackGptSocket.SlackHandlers.Command;

public static class CommandStrategyUtils
{
    /// <summary>
    ///     Returns the response to the user as Ephemeral.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static SlashCommandResponse SlashCommandResponse(string text)
    {
        var response = new SlashCommandResponse
        {
            Message = new Message
            {
                Blocks = new[]
                {
                    new SectionBlock
                    {
                        Text = new Markdown
                        {
                            Text = text
                        }
                    }
                }
            },
            ResponseType = ResponseType.Ephemeral
        };
        return response;
    }
}