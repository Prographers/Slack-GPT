using System.Text;
using SlackGptSocket.Utilities;
using SlackNet.Interaction;

namespace SlackGptSocket.SlackHandlers.Command;

/// <summary>
///     Returns the status of the bot.
/// </summary>
public class StatusCommandStrategy : ICommandStrategy
{
    public string Command => "status";

    public async Task<SlashCommandResponse> Execute(SlashCommand command)
    {
        return CommandStrategyUtils.SlashCommandResponse(GetStatus());
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
}