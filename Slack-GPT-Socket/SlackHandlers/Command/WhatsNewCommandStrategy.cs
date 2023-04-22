using System.Text;
using Octokit;
using SlackNet.Interaction;

namespace Slack_GPT_Socket.Command;

/// <summary>
///     Handles the whatsnew command. Basically just a wrapper for the GitHub API, to get the latest release notes.
/// </summary>
public class WhatsNewCommandStrategy : ICommandStrategy
{
    private readonly GitHubClient _github;
    private readonly string _owner;
    private readonly string _repo;

    public WhatsNewCommandStrategy()
    {
        _github = new GitHubClient(new ProductHeaderValue("Prographers"));
        _owner = "Prographers";
        _repo = "Slack-GPT";
    }

    public string Command => "whatsnew";

    /// <summary>
    ///     Returns the latest release notes.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    public async Task<SlashCommandResponse> Execute(SlashCommand command)
    {
        var versionString = command.Text.Substring(8).Trim();

        var releases = await _github.Repository.Release.GetAll(_owner, _repo);
        var currentVersion = versionString == string.Empty
            ? Application.VersionString
            : versionString;

        var latestRelease = releases.FirstOrDefault();
        var latestVersion = latestRelease?.TagName;
        var currentRelease = releases.FirstOrDefault(r => r.TagName == currentVersion);
        var releaseNotes = new StringBuilder();

        if (currentRelease == null)
        {
            releaseNotes.AppendLine("You are running an *unknown version* of the bot.");
            releaseNotes.AppendLine("Please update to the latest version.");
            releaseNotes.AppendLine();
            releaseNotes.AppendLine($"Current version: {Application.VersionString}");
            releaseNotes.AppendLine($"Latest version: {latestVersion}");
            releaseNotes.AppendLine();
        }
        
        var currentMajorMinor = currentVersion.Substring(0, currentVersion.LastIndexOf('.'));
        var latestMajorMinor = latestVersion.Substring(0, latestVersion.LastIndexOf('.'));

        var relevantReleases = releases
            .Where(r => r.TagName.StartsWith(currentMajorMinor))
            .OrderByDescending(r => r.TagName)
            .ToList();


        if (currentMajorMinor != latestMajorMinor)
        {
            releaseNotes.AppendLine("*New Update!*");
            foreach (var release in releases.Where(r => r.TagName.StartsWith(latestMajorMinor)).OrderByDescending(r => r.TagName))
            {
                releaseNotes.AppendLine($"{release.TagName}");
                releaseNotes.AppendLine($"{release.Body}");
                releaseNotes.AppendLine();
                releaseNotes.AppendLine($"\t{release.HtmlUrl}");
            }

            releaseNotes.AppendLine();
            releaseNotes.AppendLine("*Whats new - current version*");
        }
        else
        {
            releaseNotes.AppendLine("*Whats new*");
        }

        foreach (var release in relevantReleases)
        {
            releaseNotes.AppendLine($"{release.TagName}");
            releaseNotes.AppendLine($"{release.Body}");
            releaseNotes.AppendLine();
            releaseNotes.AppendLine($"\t{release.HtmlUrl}");
        }

        return CommandStrategyUtils.SlashCommandResponse(releaseNotes.ToString());
    }
}