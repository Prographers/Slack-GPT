using System.Text;
using GptCore;
using GptCore.Database;
using GptCore.ParameterResolvers.Common;
using GptCore.Settings;
using SlackGptSocket.BotInfo;
using SlackNet.Interaction;

namespace SlackGptSocket.SlackHandlers.Command;

/// <summary>
///     Handles the help command.
/// </summary>
public class HelpCommandStrategy : ICommandStrategy
{
    private readonly GptDefaults _gptDefaults;
    private readonly SlackBotInfo _botInfo;
    private readonly GptCustomCommands _customCommands;
    private readonly IUserCommandDb _userCommandDb;
    private readonly ParameterManager _parameterManager;

    public HelpCommandStrategy(GptDefaults gptDefaults, SlackBotInfo botInfo, GptCustomCommands customCommands,
        IUserCommandDb userCommandDb, ParameterManager parameterManager)
    {
        _gptDefaults = gptDefaults;
        _botInfo = botInfo;
        _customCommands = customCommands;
        _userCommandDb = userCommandDb;
        _parameterManager = parameterManager;
    }

    public string Command => "help";

    public async Task<SlashCommandResponse> Execute(SlashCommand command)
    {
        // If command is just "help", return general help text
        if (command.Text == "help") return CommandStrategyUtils.SlashCommandResponse(GeneralHelpText(command));

        // If command is "help <command>", return help text for that command
        var commandName = command.Text.Substring(4).Trim();

        foreach (var parameter in _parameterManager)
        {
            var args = new ParameterEventArgs
            {
                Name = commandName,
                UserId = command.UserId,
                Value = string.Empty,
                ValueRaw = string.Empty
            };

            if (parameter.CanHandle(args))
            {
                return CommandStrategyUtils.SlashCommandResponse(parameter.BuildHelpText(_gptDefaults,
                    commandName, command.UserId));
            }
        }

        return CommandStrategyUtils.SlashCommandResponse($"Command {commandName} not found.");
    }


    /// <summary>
    ///     Returns the general help text.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    private string GeneralHelpText(SlashCommand command)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Here are the commands you can use with the model");
        sb.AppendLine("Commands are only accepted if put at the beginning of the prompt eg:");
        sb.AppendLine("`-command <prompt>`");
        sb.AppendLine($"`@{_botInfo.BotInfo.User} -command <prompt>`");
        sb.AppendLine("`/gpt -command <prompt>`");
        sb.AppendLine();
        sb.AppendLine("You can also use the following commands:");
        sb.AppendLine(" - `/gpt help` - Display this help");
        sb.AppendLine(" - `/gpt generate <prompt>` - Generate text from the prompt. Very limited, please use chat channel or mention instead.");
        sb.AppendLine(" - `/gpt status` - Get the status of the bot");
        sb.AppendLine(" - `/gpt commands` - List all commands");
        sb.AppendLine(" - `/gpt whatsNew` - List the latest changes");
        sb.AppendLine(" - `/gpt help <command>` - List help for a specific parameter command");
        
        sb.AppendLine();
        foreach (var parameter in _parameterManager)
        {
            sb.AppendLine(parameter.BuildShortHelpText(_gptDefaults, command.UserId));
        }

        return sb.ToString();
    }
}