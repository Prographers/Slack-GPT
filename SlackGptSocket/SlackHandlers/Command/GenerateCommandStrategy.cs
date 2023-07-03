using SlackNet.Interaction;

namespace SlackGptSocket.SlackHandlers.Command;

public class GenerateCommandStrategy : ICommandStrategy
{
    private SlackMessageEventBaseHandler _handler;

    public GenerateCommandStrategy(SlackMessageEventBaseHandler handler)
    {
        _handler = handler;
    }

    public string Command => "generate";
    public async Task<SlashCommandResponse> Execute(SlashCommand command)
    {
        command.Text = command.Text.Substring(Command.Length).Trim();
        
        await _handler.CommandHandler(command);

        return CommandStrategyUtils.SlashCommandResponse(SlackLoadingMessage.GetRandomLoadingMessage());
    }
}