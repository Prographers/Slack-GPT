using FluentAssertions;
using LiteDB;
using SlackGptSocket.GptApi;
using SlackGptSocket.Settings;
using SlackGptSocket.Utilities.LiteDB;

namespace SlackGptTests.GptApi;

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
        _customCommands = new GptCustomCommands(new GptCommands());
        _userCommandDb = new UserCommandDb(new LiteDatabase("Filename=:memory:;Mode=Memory;Cache=Shared"));
        _resolver = new GptClientResolver(_customCommands, _gptDefaults, _userCommandDb);
    }
    
    [Test]
    [TestCase(null)]
    [TestCase("U0123ID")]
    public void ResolveParameters_UserCommand_Ok(string? userId)
    {
        // Arrange
        var command = new GptUserCommand()
        {
            Command = "-test",
            Prompt = "This is a test command.",
            UserId = userId,
            Description = "This is a test command."
        };
        _userCommandDb.AddCommand(command);
        
        var prompts = new[] {
            ("user",  $"-test How's the weather?")
        };

        // Act
        var chatRequest = _resolver.TestParseRequest("U0123ID", prompts);

        // Assert
        chatRequest.Messages[1].Content.Should().Be($"{command.Prompt}\nHow's the weather?");
    }
    
    [Test]
    [TestCase(null)]
    [TestCase("U0123ID")]
    public void ResolveParameters_UserCommand_AddMultiple_Test_Ok(string? userId)
    {
        // Arrange
        for (int i = 0; i < 3; i++)
        {
            var command = new GptUserCommand()
            {
                Command = $"-test_{i}",
                Prompt = $"This is a test command number {i}.",
                UserId = userId,
                Description = $"This is a test command number {i}."
            };
            _userCommandDb.AddCommand(command);
            
            var prompts = new[] {
                ("user",  $"{command.Command} How's the weather?")
            };

            // Act
            var chatRequest = _resolver.TestParseRequest("U0123ID", prompts);

            // Assert
            chatRequest.Messages[1].Content.Should().Be($"{command.Prompt}\nHow's the weather?");
        }
    }

    
    [Test]
    [TestCase(null)]
    [TestCase("U0123ID")]
    public void ResolveParameters_UserCommand_Remove_NotFound_Ok(string? userId)
    {
        // Arrange
        var command = new GptUserCommand()
        {
            Command = "-test",
            Prompt = "This is a test command.",
            UserId = userId,
            Description = "This is a test command."
        };
        _userCommandDb.AddCommand(command);
        _userCommandDb.RemoveCommand(command);
        
        var prompts = new[] {
            ("user",  $"-test How's the weather?")
        };

        // Act
        var chatRequest = _resolver.TestParseRequest("U0123ID", prompts);

        // Assert
        chatRequest.Messages[1].Content.Should().Be($"-test How's the weather?");
    }
    
    [Test]
    [TestCase("U0123ID", "U9999ID")]
    [TestCase("U9999ID", "U0123ID")]
    public void ResolveParameters_UserCommand_Exists_NotFound_Ok(string? ownerId, string? userId)
    {
        // Arrange
        var command = new GptUserCommand()
        {
            Command = "-test",
            Prompt = "This is a test command.",
            UserId = ownerId,
            Description = "This is a test command."
        };
        _userCommandDb.AddCommand(command);
        _userCommandDb.RemoveCommand(command);
        
        var prompts = new[] {
            ("user",  $"-test How's the weather?")
        };

        // Act
        var chatRequest = _resolver.TestParseRequest(userId, prompts);

        // Assert
        chatRequest.Messages[1].Content.Should().Be($"-test How's the weather?");
    }
}