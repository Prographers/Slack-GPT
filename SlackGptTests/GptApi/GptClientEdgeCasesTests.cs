using FluentAssertions;
using GptCore;
using GptCore.Settings;
using LiteDB;
using SlackGptSocket.GptApi;
using SlackGptSocket.Settings;
using SlackGptSocket.Utilities.LiteDB;

namespace SlackGptTests.GptApi;

[TestFixture]
[Parallelizable(ParallelScope.All)]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class GptClientEdgeCasesTests
{
    [SetUp]
    public void Setup()
    {
        _gptDefaults = new GptDefaults();
        _customCommands = new GptCustomCommands(new GptCommands());
        var userCommandDb = new UserCommandDb(new LiteDatabase("Filename=:memory:;Mode=Memory;Cache=Shared"));
        _resolver = new GptClientResolver(_customCommands, _gptDefaults, userCommandDb);
    }

    private GptDefaults _gptDefaults;
    private GptCustomCommands _customCommands;
    private GptClientResolver _resolver;
    
    private string GetTestFile(string fileName)
    {
        var solutionPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\"));
        var filePath = Path.Combine(solutionPath, $"GptApi/FileCases/{fileName}");
        Assert.True(File.Exists(filePath));
        var file = File.ReadAllText(filePath);
        file.Should().NotBeNull();
        return file;
    }
    
    [Test]
    [Timeout(1000)]
    [TestCase("stack-overflow-30042023.txt")]
    public void GptClientEdgeCases_Ok(string fileName)
    {
        var content = GetTestFile(fileName);
        
        var prompts = new[] {
            ("user",  content)
        };

        // Act
        var chatRequest = _resolver.TestParseRequest("U0123ID", prompts);
        
        // Assert not null
        chatRequest.Should().NotBeNull();
        chatRequest.Messages.Should().NotBeNull();
    }
}