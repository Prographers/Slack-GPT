using SlackNet;
using SlackNet.Events;
using SlackNet.Interaction;
using SlackNet.WebApi;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Slack_GPT_Socket;

public class SlackCommandHandler : ISlashCommandHandler
{
    private readonly SlackBotInfo _botInfo;
    private readonly ISlackApiClient _slack;
    private readonly ILogger _log;
    
    private static List<Task> _tasks = new List<Task>();

    public SlackCommandHandler(SlackBotInfo botInfo, ISlackApiClient slack, ILogger<SlackCommandHandler> log)
    {
        _botInfo = botInfo;
        _slack = slack;
        _log = log;
    }

    public async Task<SlashCommandResponse> Handle(SlashCommand command)
    {
        var response = new SlashCommandResponse
        {
            Message = new Message()
            {
                Text = $"/gpt is not implemented. Please use @{_botInfo.BotInfo.User} instead"
            },
            ResponseType = ResponseType.Ephemeral
        };
        return response;
    }
}