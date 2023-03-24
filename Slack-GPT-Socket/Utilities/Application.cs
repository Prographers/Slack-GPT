using System.Reflection;

namespace Slack_GPT_Socket;

public class Application
{
    static Application()
    {
        // Get the current assembly
        var assembly = Assembly.GetExecutingAssembly();

        // Get the assembly version
        var version = assembly.GetName().Version;

        Version = version ?? new Version(1, 0, 0, 0);
    }

    public static Version Version { get; }
    
    public static string VersionString => $"v{Version.Major}.{Version.Minor}.{Version.Build}";
}