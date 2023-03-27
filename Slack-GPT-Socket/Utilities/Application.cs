using System.Reflection;

namespace Slack_GPT_Socket;

/// <summary>
///     Information about the application.
/// </summary>
public class Application
{
    static Application()
    {
        // Read the version from version.txt
        var versionRaw = File.ReadAllText("version.txt");
        if (!Version.TryParse(versionRaw.Trim('"', 'v', '\n', '\r').Trim(), out var versionParsed))
        {
            Console.WriteLine($"Failed to parse version.txt\n\"{versionRaw}\"");
        }
        Version = versionParsed ?? new Version(1, 0, 0, 0);
    }

    /// <summary>
    ///     The version of the application.
    /// </summary>
    public static Version Version { get; }
    
    /// <summary>
    ///     The version of the application as a string.
    /// </summary>
    public static string VersionString => $"v{Version.Major}.{Version.Minor}.{Version.Build}";
}