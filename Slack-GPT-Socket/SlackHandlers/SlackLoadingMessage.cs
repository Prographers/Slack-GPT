namespace Slack_GPT_Socket;

/// <summary>
///     Messages to display while loading
/// </summary>
public static class SlackLoadingMessage
{
    /// <summary>
    ///     Messages to display while starting to load
    /// </summary>
    private static readonly string[] LoadingMessages = new []
    {
        "Let me think about that for a second...",
        "Crunching, crunching, crunching...",
        "Let me see...",
        "Hold on a second, please.",
        "Loading... It's not you, it's me. I'm always this slow.",
        "Hold tight! I'm working to find the best response for you.",
        "I'm thinking, please wait a moment.",
        "Just a few more seconds, I am is processing your query.",
        "I am looking up the information you requested. Thanks for your patience!",
        "This might take a moment.",
        "I'm working hard to get you the answer you need. Please wait a few more seconds.",
        "Searching for the best solution to your query. Hang in there.",
        "Preparing your response. It will be with you in no time!",
        "I'm working overtime to give you the best response. Thank you for your patience.",
        "Good things come to those who wait, like unicorns and rainbows.",
        "Grab a snack while you wait, we'll be here for a while.",
        "Loading... Don't worry, I'm just taking a coffee break.",
        "If patience is a virtue, you're about to become a saint. I'm loading...",
        "I'm running on caffeine and code. I'll be with you shortly!",
        "Slack GPT plugin was made by https://prographers.com/ come check us out! :)",
    };

    /// <summary>
    ///     Messages to display while waiting for a long response
    /// </summary>
    private static readonly string[] LongWaitMessages = new[]
    {
        "Just a moment, I'm still gathering my thoughts...",
        "Hold on, I'm crunching the numbers for you, and there is a lot of numbers...",
        "Let me put on my thinking cap, for longer...",
        "One moment, I'm digging even deeper for your answer...",
        "I'm spinning my gears to get you the best response...",
        "Just a little longer, I promise it's worth the wait!",
        "Bear with me, I'm doing some heavy lifting...",
        "I'm working my magic to get you the perfect answer...",
        "I'm sorting through the data, be with you in a jiffy!",
        "Please hold on, I'm searching the cosmos for your answer...",
        "I'm weaving together the perfect response...",
        "I'm running at full speed to get you the information you need.",
        "Hang tight, I'm scanning the universe for your answer!",
        "I'm putting the pieces together for your response.",
        "Give me a moment, I'm calculating the best solution.",
        "Hold on, I'm gathering the stars to light up your answer.",
        "Just a little longer, I'm polishing up your response.",
        "I'm bending time and space to find your answer.",
        "Hold on, I'm just taking a quick trip to the knowledge dimension!",
        "Just a moment, I'm consulting my crystal ball...",
        "Fun fact, GPT is generating response word by word, so it takes a while to generate a long response."
    };
    
    /// <summary>
    ///     Gets a random loading message
    /// </summary>
    /// <returns></returns>
    public static string GetRandomLoadingMessage()
    {
        return LoadingMessages[new Random().Next(0, LoadingMessages.Length)];
    }
    
    /// <summary>
    ///     Gets a random long wait message
    /// </summary>
    /// <returns></returns>
    public static string GetRandomLongWaitMessage()
    {
        return LongWaitMessages[new Random().Next(0, LongWaitMessages.Length)];
    }
}