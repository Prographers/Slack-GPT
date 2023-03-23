using SlackNet;

namespace Slack_GPT_Socket;

/// <summary>
///     Application builder to add SlackBotInfo to the application
/// </summary>
public static class SlackBotInfoApplicationBuilder
{
    /// <summary>
    ///     Add SlackBotInfo to the application
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IServiceCollection AddSlackBotInfo(this IServiceCollection builder)
    {
        builder.AddSingleton<SlackBotInfo>();
        return builder;
    }
    
    /// <summary>
    ///     Initialize SlackBotInfo with the bot info
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseSlackBotInfo(this IApplicationBuilder builder)
    {
        var slackApiClient = builder.ApplicationServices.GetRequiredService<ISlackApiClient>();
        var slackBotInfo = builder.ApplicationServices.GetRequiredService<SlackBotInfo>();
        var botInfoTask =  slackApiClient.Auth.Test();
        botInfoTask.Wait();
        slackBotInfo.BotInfo = botInfoTask.Result;
        return builder;
    }
}