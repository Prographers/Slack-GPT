using System.Reflection;

namespace Slack_GPT_Socket;

public class Application
{
    static Application()
    {
        // Read the version from version.txt
        var versionRaw = File.ReadAllText("version.txt");
        versionRaw = versionRaw.Trim('"', ' ', 'v');
        var version = Version.TryParse(versionRaw, out var versionParsed) ? versionParsed : null;
        Version = version ?? new Version(1, 0, 0, 0);
    }

    public static Version Version { get; }
    
    public static string VersionString => $"v{Version.Major}.{Version.Minor}.{Version.Build}";
}