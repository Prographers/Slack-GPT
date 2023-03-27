using System.Text;
using SlackNet;
using SlackNet.Blocks;
using SlackNet.Interaction;
using SlackNet.WebApi;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Slack_GPT_Socket;

/// <summary>
///     Handles the slash commands sent to the bot.
/// </summary>
public class SlackCommandHandler : ISlashCommandHandler
{
    private readonly GptCustomCommands _customCommands;
    private readonly SlackBotInfo _botInfo;
    private readonly ISlackApiClient _slack;
    private readonly ILogger _log;

    public SlackCommandHandler(GptCustomCommands customCommands, SlackBotInfo botInfo, ISlackApiClient slack,
        ILogger<SlackCommandHandler> log)
    {
        _customCommands = customCommands;
        _botInfo = botInfo;
        _slack = slack;
        _log = log;
    }

    /// <summary>
    ///     Handles the slash command.
    /// </summary>
    /// <param name="command">Command that came from the user</param>
    /// <returns></returns>
    public async Task<SlashCommandResponse> Handle(SlashCommand command)
    {
        if (command.Text == "help") return SlashCommandResponse(GeneralHelpText(command));
        if (command.Text == "status") return SlashCommandResponse(GetStatus());

        if (command.Text.StartsWith("help"))
        {
            var commandName = command.Text.Substring(4).Trim();

            if (_customCommands.TryResolveCommand(commandName, out var result))
            {
                var text = $"{result.Command} - {result.Description}\nPrompt:\n> {result.Prompt}" +
                           $"\n\nIs executed as system command: {result.AsSystem}";
                
                return SlashCommandResponse(text);
            }

            return SlashCommandResponse($"Command {commandName} not found.");
        }

        var response = SlashCommandResponse(
            GeneralHelpText(command));
        return response;
    }

    /// <summary>
    ///     Returns the status of the bot.
    /// </summary>
    /// <returns></returns>
    private static string GetStatus()
    {
        var sb = new StringBuilder();
        sb.AppendLine("I'm Online!");
        sb.AppendLine();
        sb.AppendLine($"Version {Application.Version}");

        return sb.ToString();
    }

    /// <summary>
    ///     Returns the help text for the model parameters.
    /// </summary>
    /// <returns></returns>
    private string ModelParametersHelpText()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Model parameters:");
        sb.AppendLine("-maxTokens: limits tokens in output, default 2048 (GPT-3.5: 4000, GPT-4: 8000);");
        sb.AppendLine("-temperature: controls randomness, default 0.7;");
        sb.AppendLine("-topP: filters token choices, default 1;");
        sb.AppendLine("-presencePenalty: penalizes repeated tokens, default 0;");
        sb.AppendLine("-frequencyPenalty: discourages frequent tokens, default 0;");
        sb.AppendLine("-model: specifies model, default GPT-4, options: GPT-4, GPT-3.5-turbo;");
        sb.AppendLine(
            "-system: custom system message, default \"You are a helpful assistant. Today is {Current Date}\".");

        return sb.ToString();
    }

    /// <summary>
    ///     Returns the general help text.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    private string GeneralHelpText(SlashCommand command)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"To use this bot, please send a message tagging <@{_botInfo.BotInfo.UserId}> " +
                      $"on a channel where he is invited.")
            .AppendLine($"Use {command.Command} help to display this help.")
            .AppendLine($"Use {command.Command} help -[command] to display prompt of the command.")
            .AppendLine($"Use {command.Command} status to display bot status.")
            .AppendLine("\n")
            .AppendLine(ModelParametersHelpText())
            .AppendLine("\n")
            .AppendLine($"{_customCommands.GetHelp()}");

        return sb.ToString();
    }

    /// <summary>
    ///     Returns the response to the user.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    private SlashCommandResponse SlashCommandResponse(string text)
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