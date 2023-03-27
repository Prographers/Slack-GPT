using System.Reflection;

namespace Slack_GPT_Socket;

public class Application
{
    static Application()
    {
        // Read the version from version.txt
        var versionRaw = File.ReadAllText("version.txt");
        if (!Version.TryParse(versionRaw.Trim('"', 'v').Trim(), out var versionParsed))
        {
            Console.WriteLine($"Failed to parse version.txt\n\"{versionRaw}\"");
        }
        Version = versionParsed ?? new Version(1, 0, 0, 0);
    }

    public static Version Version { get; }
    
    public static string VersionString => $"v{Version.Major}.{Version.Minor}.{Version.Build}";
}