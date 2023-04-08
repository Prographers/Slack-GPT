using FluentAssertions;
using Slack_GPT_Socket.GptApi;
using Slack_GPT_Socket.Settings;

namespace Slack_GPT_Tests.GptApi;

public class GptClientUserCommandsTests
{
    private GptDefaults _gptDefaults;
    private GptCustomCommands _customCommands;
    private GptClientResolver _resolver;
    private MemoryUserCommandDb _userCommandDb;
    [SetUp]
    public void Setup()
    {
        _gptDefaults = new GptDefaults();
        _customCommands = new GptCustomCommands(MoqUtils.CreateOptionsMonitorMock(
            new GptCommands()
        ));
        _userCommandDb = new MemoryUserCommandDb();
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
        _userCommandDb.Commands.Add(command);
        
        var prompts = new[] {
            ("user",  $"-test How's the weather?")
        };

        // Act
        var chatRequest = _resolver.TestParseRequest(prompts);

        // Assert
        chatRequest.Messages[1].Content.Should().Be($"{command.Prompt}\nHow's the weather?");
    }

}