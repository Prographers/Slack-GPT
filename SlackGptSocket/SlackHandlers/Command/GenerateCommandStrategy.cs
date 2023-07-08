using SlackNet.Events;
using SlackNet.Interaction;

namespace SlackGptSocket.SlackHandlers.Command;

public class GenerateCommandStrategy : ICommandStrategy
{
    private readonly SlackMessageEventBaseHandler _handler;

    public GenerateCommandStrategy(SlackMessageEventBaseHandler handler)
    {
        _handler = handler;
    }

    public string Command => "generate";

    public async Task<SlashCommandResponse> Execute(SlashCommand command)
    {
        command.Text = command.Text.Substring(Command.Length).Trim();

        await CommandHandler(command);

        return CommandStrategyUtils.SlashCommandResponse(SlackLoadingMessage.GetRandomLoadingMessage());
    }

    /// <summary>
    ///     Handles SlashCommand events and responds as if this is a message in bot's chat that will generate an answer form
    ///     OpenAI GPT.
    /// </summary>
    /// <param name="slashCommand"></param>
    public async Task CommandHandler(SlashCommand slashCommand)
    {
        // Convert SlashCommand to MessageEventBase
        var slackEvent = new AppMention
        {
            Channel = slashCommand.ChannelId,
            User = slashCommand.UserId,
            Text = slashCommand.Text,
            Team = slashCommand.TeamId,
            EventTs = slashCommand.TriggerId,
            Ts = slashCommand.TriggerId
        };

        _handler.RemoveMentionsFromText(slackEvent);

        var context = await _handler.ResolveConversationContextWithMentions(slackEvent);

        await _handler.HandleNewGptRequest(slackEvent, context);
    }
}