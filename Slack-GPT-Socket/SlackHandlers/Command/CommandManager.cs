using Slack_GPT_Socket.GptApi;
using Slack_GPT_Socket.GptApi.ParameterResolvers;
using Slack_GPT_Socket.Settings;
using Slack_GPT_Socket.Utilities.LiteDB;
using SlackNet.Interaction;

namespace Slack_GPT_Socket.Command;

public class CommandManager
{
    private readonly List<ICommandStrategy> _commandStrategies = new();

    /// <summary>
    ///     Initializes the command manager.
    /// </summary>
    /// <param name="customCommands"></param>
    /// <param name="botInfo"></param>
    /// <param name="userCommandDb"></param>
    /// <param name="gptDefaults"></param>
    /// <param name="log"></param>
    public CommandManager(
        GptCustomCommands customCommands,
        SlackBotInfo botInfo,
        IUserCommandDb userCommandDb,
        GptDefaults gptDefaults,
        ILogger log)
    {
        var parameterManager = new ParameterManager(customCommands, gptDefaults, userCommandDb);

        AddCommandStrategy(new HelpCommandStrategy(gptDefaults, botInfo, customCommands, userCommandDb,
            parameterManager));
        AddCommandStrategy(new StatusCommandStrategy());
        AddCommandStrategy(new CommandsCommandStrategy(userCommandDb));
        AddCommandStrategy(new WhatsNewCommandStrategy());
    }


    public Task<SlashCommandResponse> Execute(SlashCommand command)
    {
        foreach (var strategy in _commandStrategies)
        {
            if (strategy.CanHandle(command)) return strategy.Execute(command);
        }

        return Task.FromResult(CommandStrategyUtils.SlashCommandResponse("Command not found."));
    }

    private void AddCommandStrategy(ICommandStrategy commandStrategy)
    {
        _commandStrategies.Add(commandStrategy);
    }
}