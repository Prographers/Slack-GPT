using LiteDB;
using Slack_GPT_Socket;
using Slack_GPT_Socket.GptApi;
using Slack_GPT_Socket.Settings;
using Slack_GPT_Socket.Utilities.LiteDB;
using SlackNet.AspNetCore;
using SlackNet.Events;

var builder = WebApplication.CreateBuilder(args);

var settings = builder.Configuration.GetSection("Api").Get<ApiSettings>()!;
builder.Services.AddOptions<ApiSettings>().Bind(builder.Configuration.GetSection("Api"));
builder.Services.Configure<GptCommands>(builder.Configuration.GetSection("GptCommands"));
builder.Services.Configure<GptDefaults>(builder.Configuration.GetSection("GptDefaults"));
builder.Services.Configure<SlackSettings>(builder.Configuration.GetSection("Slack"));

builder.Services.AddSingleton<GptClient>();
builder.Services.AddSingleton<GptCustomCommands>();
builder.Services.AddSingleton<ILiteDatabase>(x => 
    new LiteDatabase(builder.Configuration.GetConnectionString("LiteDB") ?? "Filename=:memory:;Mode=Memory;Cache=Shared")
);

builder.Services.AddSingleton<IUserCommandDb, UserCommandDb>();

builder.Services.AddSlackNet(c => c
    .UseApiToken(settings.SlackBotToken)
    .UseAppLevelToken(settings.SlackAppToken)
    .RegisterEventHandler<AppMention, SlackMentionHandler>()
    .RegisterEventHandler<MessageEvent, SlackMessageHandler>()
    .RegisterSlashCommandHandler<SlackCommandHandler>("/gpt")
);

builder.Services.AddSlackBotInfo();

var app = builder.Build();
app.UseSlackNet(c =>
    c.UseSigningSecret(settings.SlackSigningSecret)
        .UseSocketMode(true));

app.UseSlackBotInfo();

app.MapGet("/", () => "Hello Slack!");

Console.WriteLine(Application.VersionString);

app.Run();