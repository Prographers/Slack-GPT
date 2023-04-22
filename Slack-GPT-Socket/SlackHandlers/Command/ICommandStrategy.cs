using SlackNet.Interaction;

namespace Slack_GPT_Socket.Command;

public interface ICommandStrategy
{
    string Command { get; }

    bool CanHandle(SlashCommand command)
    {
        return command.Text.StartsWith(Command);
    }

    Task<SlashCommandResponse> Execute(SlashCommand command);
}

public class CommandsCommandStrategy : ICommandStrategy
{
    public string Command => "commands";

    public bool CanHandle(SlashCommand command)
    {
        return false;
    }

    public async Task<SlashCommandResponse> Execute(SlashCommand command)
    {
        return CommandStrategyUtils.SlashCommandResponse("ok");
    }
}