using FluentAssertions;
using LiteDB;
using Slack_GPT_Socket;
using Slack_GPT_Socket.Command;
using Slack_GPT_Socket.GptApi;
using Slack_GPT_Socket.Settings;
using Slack_GPT_Socket.Utilities.LiteDB;
using SlackNet.Blocks;
using SlackNet.Interaction;
using SlackNet.WebApi;

namespace Slack_GPT_Tests.Handlers;

[TestFixture]
[Parallelizable(ParallelScope.All)]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class CommandHandlerTests
{
    [SetUp]
    public void Setup()
    {
        _gptDefaults = new GptDefaults();
        _customCommands = new GptCustomCommands(MoqUtils.CreateOptionsMonitorMock(
            new GptCommands()
        ));
        _userCommandDb = new UserCommandDb(new LiteDatabase("Filename=:memory:;Mode=Memory;Cache=Shared"));

        _botInfo = new SlackBotInfo()
        {
            BotInfo = new AuthTestResponse()
            {
                UserId = "BOTID",
                TeamId = "TEAMID"
            }
        };
        _commandManager = new CommandManager(
            _customCommands, _botInfo, _userCommandDb, _gptDefaults, null
        );
    }

    private GptDefaults _gptDefaults;
    private UserCommandDb _userCommandDb;
    private GptCustomCommands _customCommands;
    private GptClientResolver _resolver;
    private CommandManager _commandManager;
    private SlackBotInfo _botInfo;


    [Test]
    [TestCase("help", "commands you can use")]
    [TestCase("help -notFound", "not found")]
    [TestCase("help -model", "specifies model")]
    [TestCase("status", "I'm Online")]
    public async Task ExecuteCommand_Ok(string command, string contains)
    {
        // Arrange
        var slashCommand = new SlashCommand()
        {
            Text = command
        };

        // Act
        var response = await _commandManager.Execute(slashCommand);

        // Assert
        response.Message.Blocks[0].Should().BeOfType<SectionBlock>();
        response.Message.Blocks[0].Should().NotBeNull();
        var text = (response.Message.Blocks[0] as SectionBlock)!.Text.Text;
        Console.WriteLine(text);
        text.Should().NotBeNullOrEmpty();
        text.Should().Contain(contains);
    }

    [Test]
    [TestCase(null)]
    [TestCase("U0123ID")]
    public async Task HelpCommand_CustomGlobal_Ok(string? userId)
    {
        // Arrange
        var slashCommand = new SlashCommand()
        {
            Text = "help -customCommand"
        };
        _userCommandDb.AddCommand(new GptUserCommand()
        {
            Command = "-customCommand",
            UserId = userId,
            Description = "This is a custom command",
            Prompt = "This is a custom command",
        });

        // Act
        await ExecuteAndAssertSlashCommand("help", "U0123ID", "This is a custom command");
        await ExecuteAndAssertSlashCommand("help -customCommand", "U0123ID", "This is a custom command");
        await ExecuteAndAssertSlashCommand("help -customcommand", "U0123ID", "This is a custom command");
    }

    private async Task ExecuteAndAssertSlashCommand(string commandText, string userId, string expectedText)
    {
        var response = await _commandManager.Execute(new SlashCommand()
        {
            Text = commandText,
            UserId = userId
        });

        // Assert
        response.Message.Blocks[0].Should().BeOfType<SectionBlock>();
        response.Message.Blocks[0].Should().NotBeNull();
        var text = (response.Message.Blocks[0] as SectionBlock)!.Text.Text;
        Console.WriteLine(text);
        text.Should().NotBeNullOrEmpty();
        text.Should().Contain(expectedText);
    }
}