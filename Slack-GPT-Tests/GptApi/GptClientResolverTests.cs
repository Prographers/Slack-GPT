using FluentAssertions;
using OpenAI.Chat;
using Slack_GPT_Socket;
using Slack_GPT_Socket.GptApi;
using Slack_GPT_Socket.Settings;

namespace Slack_GPT_Tests.GptApi;

[TestFixture]
[Parallelizable(ParallelScope.All)]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class GptClientResolverTests
{
    private GptDefaults _gptDefaults;
    private GptCustomCommands _customCommands;
    private GptClientResolver _resolver;

    [SetUp]
    public void Setup()
    {
        _gptDefaults = new GptDefaults();
        _customCommands = new GptCustomCommands(MoqUtils.CreateOptionsMonitorMock(
            new GptCommands()
        ));
        _resolver = new GptClientResolver(_customCommands, _gptDefaults);
    }

    [Test]
    public void ParseRequest_WithValidInputs_ReturnsChatRequest()
    {
        // Arrange
        var prompts = new[] {
            ("user", "How's the weather?")
        };

        // Act
        var chatRequest = _resolver.TestParseRequest(prompts);

        // Assert
        chatRequest.Should().NotBeNull();
        var messages = chatRequest.Messages.ToList();
        messages.Should().HaveCount(2);
        messages[0].Role.Should().Be("system");
        messages[0].Content.Should().StartWith("You are a helpful assistant.");
        messages[1].Should().BeEquivalentTo(new ChatPrompt("user", "How's the weather?"));
    }

    [Test]
    public void ResolveModel_WithValidInputs_SetsModelCorrectly()
    {
        // Arrange
        var prompts = new[] {
            ("user", "gpt-3.5-turbo How's the weather?")
        };

        // Act
        var chatRequest = _resolver.TestParseRequest(prompts);

        // Assert
        chatRequest.Model.Should().Be("gpt-3.5-turbo");
    }

    [Test]
    public void ResolveModel_WithInvalidInputs_SetsDefaultModel()
    {
        // Arrange
        var prompts = new[] {
            ("user", "invalidModel How's the weather?")
        };
        
        // Act
        var chatRequest = _resolver.TestParseRequest(prompts);

        // Assert
        chatRequest.Model.Should().Be("gpt-4");
        chatRequest.Messages[1].Content.Should().Be("invalidModel How's the weather?");
    }

    [Test]
    public void ResolveParameters_WithValidInputs_ResolvesParametersCorrectly()
    {
        // Arrange
        var prompts = new[] {
            ("user",  "-maxTokens 20 -temperature 0.7 How's the weather?")
        };

        // Act
        var chatRequest = _resolver.TestParseRequest(prompts);

        // Assert
        chatRequest.MaxTokens.Should().Be(20);
        chatRequest.Temperature.Should().BeApproximately(0.7f, 0.001f);
        chatRequest.Messages[1].Content.Should().Be("How's the weather?");
    }
    
    [Test]
    public void ResolveParameters_WithInvalidInputs_ResolvesParametersCorrectly()
    {
        // Arrange
        var prompts = new[] {
            ("user",  "-maxTokens 20 -temperature 0.7 -invalidParameter How's the weather?")
        };

        // Act
        var chatRequest = _resolver.TestParseRequest(prompts);

        // Assert
        chatRequest.MaxTokens.Should().Be(20);
        chatRequest.Temperature.Should().BeApproximately(0.7f, 0.001f);
        chatRequest.Messages[1].Content.Should().Be("-invalidParameter How's the weather?");
    }
    
    [Test]
    [TestCase("-max_tokens 20", 20)]
    [TestCase("-maxTokens 20", 20)]
    [TestCase("-max-tokens 200", 200)]
    [TestCase("-max_token 2000", 2000)]
    [TestCase("-maxtoken 1000", 1000)]
    [TestCase("-max-token 44a", 44)]
    public void ResolveParameters_MaxTokens_WithAliases_Ok(string parameter, int value)
    {
        // Arrange
        var prompts = new[] {
            ("user",  $"{parameter} How's the weather?")
        };

        // Act
        var chatRequest = _resolver.TestParseRequest(prompts);

        // Assert
        chatRequest.MaxTokens.Should().Be(value);
        chatRequest.Messages[1].Content.Should().Be("How's the weather?");
    }
    
    [Test]
    [TestCase("-temperature 0.7", 0.7f)]
    [TestCase("-temp .03", 0.03f)]
    [TestCase("-t 1.5", 1.5f)]
    [TestCase("-t 1.5f", 1.5f)]
    [TestCase("-t 1,2", 1.2f)]
    public void ResolveParameters_Temperature_WithAliases_Ok(string parameter, float value)
    {
        // Arrange
        var prompts = new[] {
            ("user",  $"{parameter} How's the weather?")
        };

        // Act
        var chatRequest = _resolver.TestParseRequest(prompts);

        // Assert
        chatRequest.Temperature.Should().BeApproximately(value, 0.001f);
        chatRequest.Messages[1].Content.Should().Be("How's the weather?");
    }
    
    [Test]
    [TestCase("-frequency_penalty 0.3", 0.3f)]
    [TestCase("-frequencypenalty .03", 0.03f)]
    public void ResolveParameters_FrequencyPenaltyResolver_WithAliases_Ok(string parameter, float value)
    {
        // Arrange
        var prompts = new[] {
            ("user",  $"{parameter} How's the weather?")
        };

        // Act
        var chatRequest = _resolver.TestParseRequest(prompts);

        // Assert
        chatRequest.FrequencyPenalty.Should().BeApproximately(value, 0.001f);
        chatRequest.Messages[1].Content.Should().Be("How's the weather?");
    }
    
    [Test]
    [TestCase("-presence_penalty 0.3", 0.3f)]
    [TestCase("-PresencePenalty .03", 0.03f)]
    [TestCase("-Presence-Penalty 1", 1f)]
    public void ResolveParameters_PresencePenaltyResolver_WithAliases_Ok(string parameter, float value)
    {
        // Arrange
        var prompts = new[] {
            ("user",  $"{parameter} How's the weather?")
        };

        // Act
        var chatRequest = _resolver.TestParseRequest(prompts);

        // Assert
        chatRequest.PresencePenalty.Should().BeApproximately(value, 0.001f);
        chatRequest.Messages[1].Content.Should().Be("How's the weather?");
    }
    
    [Test]
    [TestCase("-Top-P 0.3", 0.3f)]
    [TestCase("-top_p .03", 0.03f)]
    [TestCase("-topp 1", 1f)]
    public void ResolveParameters_TopPResolver_WithAliases_Ok(string parameter, float value)
    {
        // Arrange
        var prompts = new[] {
            ("user",  $"{parameter} How's the weather?")
        };

        // Act
        var chatRequest = _resolver.TestParseRequest(prompts);

        // Assert
        chatRequest.TopP.Should().BeApproximately(value, 0.001f);
        chatRequest.Messages[1].Content.Should().Be("How's the weather?");
    }
    
    [Test]
    [TestCase("-model", "gpt-4")]
    [TestCase("-m gpt-3", "gpt-3.5-turbo")]
    [TestCase("-m gpt3", "gpt-3.5-turbo")]
    [TestCase("-m chatGPT", "gpt-3.5-turbo")]
    public void ResolveParameters_ModelResolver_WithAliases_Ok(string parameter, string value)
    {
        // Arrange
        var prompts = new[] {
            ("user",  $"{parameter} How's the weather?")
        };

        // Act
        var chatRequest = _resolver.TestParseRequest(prompts);

        // Assert
        chatRequest.Model.Should().Be(value);
        chatRequest.Messages[1].Content.Should().Be("How's the weather?");
    }
    
    [Test]
    [TestCase("-system \"You are a helpful assistant\"", "You are a helpful assistant")]
    [TestCase("-s \"Hello.\"", "Hello.")]
    [TestCase("-s Hello", "Hello")]
    [TestCase("-s !@#$%^&*():\"\"\\", "!@#$%^&*():\"\"\\")]
    public void ResolveParameters_SystemResolver_WithAliases_Ok(string parameter, string value)
    {
        // Arrange
        var prompts = new[] {
            ("user",  $"{parameter} How's the weather?")
        };

        // Act
        var chatRequest = _resolver.TestParseRequest(prompts);

        // Assert
        chatRequest.Messages[0].Content.Should().StartWith(value);
        chatRequest.Messages[1].Content.Should().Be("How's the weather?");
    }
    
    [Test]
    [TestCase("-command", "This is a command body", false)]
    [TestCase("-command-sys", "This is a system command", true)]
    public void ResolveParameters_PredefinedCommandResolver_WithAliases_Ok(string parameter, 
        string commandBody, bool asSystem)
    {
        _customCommands.Commands.Commands.Add(new GptCommand()
        {
            Command = parameter,
            Prompt = commandBody,
            AsSystem = asSystem,
            Description = "This command is for tests only"
        });
        
        // Arrange
        var prompts = new[] {
            ("user",  $"{parameter} How's the weather?")
        };

        // Act
        var chatRequest = _resolver.TestParseRequest(prompts);

        // Assert
        if(asSystem)
            chatRequest.Messages[0].Content.Should().Contain(commandBody);
        else
            chatRequest.Messages[1].Content.Should().Be(commandBody + "\nHow's the weather?");
    }
}