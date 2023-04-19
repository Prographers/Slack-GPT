using FluentAssertions;
using LiteDB;
using Slack_GPT_Socket.GptApi;
using Slack_GPT_Socket.Settings;
using Slack_GPT_Socket.Utilities.LiteDB;

namespace Slack_GPT_Tests.GptApi;

[TestFixture]
[Parallelizable(ParallelScope.All)]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class GptClientUserCommandsTests
{
    private GptDefaults _gptDefaults;
    private GptCustomCommands _customCommands;
    private GptClientResolver _resolver;
    private UserCommandDb _userCommandDb;
    
    [SetUp]
    public void Setup()
    {
        _gptDefaults = new GptDefaults();
        _customCommands = new GptCustomCommands(MoqUtils.CreateOptionsMonitorMock(
            new GptCommands()
        ));
        _userCommandDb = new UserCommandDb(new LiteDatabase("Filename=:memory:;Mode=Memory;Cache=Shared"));
        _resolver = new GptClientResolver(_customCommands, _gptDefaults, _userCommandDb);
    }
    
    [Test]
    public void ResolveParameters_UserCommand_Ok()
    {
        // Arrange
        var command = new GptUserCommand()
        {
            Command = "-test",
            Prompt = "This is a test command.",
            UserId = null,
            Description = "This is a test command."
        };
        _userCommandDb.AddCommand(command);
        
        var prompts = new[] {
            ("user",  $"-test How's the weather?")
        };

        // Act
        var chatRequest = _resolver.TestParseRequest(prompts);

        // Assert
        chatRequest.Messages[1].Content.Should().Be($"{command.Prompt}\nHow's the weather?");
    }
    
    [Test]
    public void ResolveParameters_UserCommand_AddMultiple_Test_Ok()
    {
        // Arrange
        for (int i = 0; i < 3; i++)
        {
            var command = new GptUserCommand()
            {
                Command = $"-test_{i}",
                Prompt = $"This is a test command number {i}.",
                UserId = null,
                Description = $"This is a test command number {i}."
            };
            _userCommandDb.AddCommand(command);
            
            var prompts = new[] {
                ("user",  $"{command.Command} How's the weather?")
            };

            // Act
            var chatRequest = _resolver.TestParseRequest(prompts);

            // Assert
            chatRequest.Messages[1].Content.Should().Be($"{command.Prompt}\nHow's the weather?");
        }
    }

    
    [Test]
    public void ResolveParameters_UserCommand_Remove_NotFound_Ok()
    {
        // Arrange
        var command = new GptUserCommand()
        {
            Command = "-test",
            Prompt = "This is a test command.",
            UserId = null,
            Description = "This is a test command."
        };
        _userCommandDb.AddCommand(command);
        _userCommandDb.RemoveCommand(command);
        
        var prompts = new[] {
            ("user",  $"-test How's the weather?")
        };

        // Act
        var chatRequest = _resolver.TestParseRequest(prompts);

        // Assert
        chatRequest.Messages[1].Content.Should().Be($"-test How's the weather?");
    }
}