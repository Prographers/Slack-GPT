using SlackNet;

namespace SlackGptSocket.BotInfo;

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
        var task = UseSlackBotInfoAsync(builder);
        task.Wait();
        return builder;
    }

    private static async Task UseSlackBotInfoAsync(IApplicationBuilder builder)
    {
        var logger = builder.ApplicationServices.GetRequiredService<ILogger<SlackBotInfo>>();
        var slackApiClient = builder.ApplicationServices.GetRequiredService<ISlackApiClient>();
        var slackBotInfo = builder.ApplicationServices.GetRequiredService<SlackBotInfo>();
        
        Task.WaitAll(new[]
        {
            SetBotInfo(slackApiClient, slackBotInfo, logger),
        });
    }

    private static async Task SetBotInfo(ISlackApiClient slackApiClient, SlackBotInfo slackBotInfo, ILogger<SlackBotInfo> logger)
    {
        var botInfoTask = await slackApiClient.Auth.Test();
        slackBotInfo.BotInfo = botInfoTask;
    }
}