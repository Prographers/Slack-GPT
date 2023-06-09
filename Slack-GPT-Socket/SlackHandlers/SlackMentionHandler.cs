﻿using Microsoft.Extensions.Options;
using Slack_GPT_Socket.GptApi;
using Slack_GPT_Socket.Settings;
using SlackNet;
using SlackNet.Blocks;
using SlackNet.Events;
using SlackNet.WebApi;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Slack_GPT_Socket;

/// <summary>
///     A simple event handler that says pong when you say ping.
/// </summary>
internal class SlackMentionHandler : IEventHandler<MessageEventBase>
{
    private readonly ISlackApiClient _slack;
    private readonly ILogger _log;
    private readonly GptClient _gptClient;
    private readonly SlackBotInfo _botInfo;
    private readonly IOptions<SlackSettings> _slackSettings;

    private readonly SlackMessageEventBaseHandler _handler;

    public SlackMentionHandler(ISlackApiClient slack, ILogger<SlackMentionHandler> log, GptClient gptClient,
        SlackBotInfo botInfo, IOptions<SlackSettings> slackSettings)
    { 
        _slack = slack;
        _log = log;
        _gptClient = gptClient;
        _botInfo = botInfo;
        
        _handler = new SlackMessageEventBaseHandler(slack, log, gptClient, botInfo, 
            slackSettings.Value);
    }

    /// <summary>
    ///     Handles incoming MessageEventBase events and responds accordingly.
    /// </summary>
    /// <param name="slackEvent">The MessageEventBase event.</param>
    public async Task Handle(MessageEventBase slackEvent)
    {
        await _handler.MentionHandler(slackEvent);
    }

}