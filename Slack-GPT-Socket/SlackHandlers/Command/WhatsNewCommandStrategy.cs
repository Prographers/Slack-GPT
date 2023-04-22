using Octokit;
using SlackNet.Interaction;

namespace Slack_GPT_Socket.Command;

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

    public async Task<SlashCommandResponse> Execute(SlashCommand command)
    {
        var versionString = command.Text.Substring(8).Trim();

        var releases = await _github.Repository.Release.GetAll(_owner, _repo);
        var currentVersion = versionString == string.Empty
            ? Application.VersionString
            : versionString;

        var latestRelease = releases.FirstOrDefault(r => r.TagName == currentVersion);

        if (latestRelease == null)
            return CommandStrategyUtils.SlashCommandResponse($"No release found for current version. {currentVersion}");

        return CommandStrategyUtils.SlashCommandResponse(latestRelease.Body);
    }
}