using Microsoft.Extensions.Options;
using Slack_GPT_Socket.Command;
using Slack_GPT_Socket.GptApi;
using Slack_GPT_Socket.Settings;
using Slack_GPT_Socket.Utilities.LiteDB;
using SlackNet;
using SlackNet.Interaction;

namespace Slack_GPT_Socket;

/// <summary>
///     Handles the slash commands sent to the bot.
/// </summary>
public class SlackCommandHandler : ISlashCommandHandler
{
    private readonly CommandManager _commandManager;
    
    public SlackCommandHandler(
        GptCustomCommands customCommands,
        SlackBotInfo botInfo,
        IUserCommandDb userCommandDb,
        IOptions<GptDefaults> gptDefaults,
        ILogger<SlackCommandHandler> log)
    {
        _commandManager =
            new CommandManager(customCommands, botInfo, userCommandDb, gptDefaults.Value, log);
    }


    /// <summary>
    ///     Handles the slash command.
    /// </summary>
    /// <param name="command">Command that came from the user</param>
    /// <returns></returns>
    public async Task<SlashCommandResponse> Handle(SlashCommand command)
    {
        return await _commandManager.Execute(command);
    }
}