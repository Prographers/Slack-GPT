using FluentAssertions;
using LiteDB;
using SlackGptSocket.BotInfo;
using SlackGptSocket.GptApi;
using SlackGptSocket.Settings;
using SlackGptSocket.SlackHandlers.Command;
using SlackGptSocket.Utilities.LiteDB;
using SlackNet.Blocks;
using SlackNet.Interaction;
using SlackNet.WebApi;

namespace SlackGptTests.Handlers;

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

        _botInfo = new SlackBotInfo
        {
            BotInfo = new AuthTestResponse
            {
                UserId = "BOTID",
                TeamId = "TEAMID"
            }
        };
        _commandManager = new CommandManager(
            null, null, null,
            _customCommands, _botInfo, _userCommandDb, _gptDefaults, null
        );
    }

    private GptDefaults _gptDefaults;
    private UserCommandDb _userCommandDb;
    private GptCustomCommands _customCommands;
    private GptClientResolver _resolver;
    private CommandManager _commandManager;
    private SlackBotInfo _botInfo;
    
    private static void AssertCommandResult(SlashCommandResponse list, string result)
    {
        list.Message.Blocks[0].Should().BeOfType<SectionBlock>();
        list.Message.Blocks[0].Should().NotBeNull();
        var text = (list.Message.Blocks[0] as SectionBlock)!.Text.Text;
        Console.WriteLine(text);
        text.Should().Contain(result);
    }
    
    private async Task ExecuteAndAssertSlashCommand(string commandText, string userId, string expectedText)
    {
        var response = await _commandManager.Execute(new SlashCommand
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


    [Test]
    [TestCase("help", "commands you can use")]
    [TestCase("help -notFound", "not found")]
    [TestCase("help -model", "specifies model")]
    [TestCase("status", "I'm Online")]
    public async Task ExecuteCommand_Ok(string command, string contains)
    {
        // Arrange
        var slashCommand = new SlashCommand
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
        var slashCommand = new SlashCommand
        {
            Text = "help -customCommand"
        };
        _userCommandDb.AddCommand(new GptUserCommand
        {
            Command = "-customCommand",
            UserId = userId,
            Description = "This is a custom command",
            Prompt = "This is a custom command"
        });

        // Act
        await ExecuteAndAssertSlashCommand("help", "U0123ID", "This is a custom command");
        await ExecuteAndAssertSlashCommand("help -customCommand", "U0123ID", "This is a custom command");
        await ExecuteAndAssertSlashCommand("help -customcommand", "U0123ID", "This is a custom command");
    }

    [Test]
    public async Task CommandsCommand_List_Empty_Ok()
    {
        // Arrange
        var slashCommand = new SlashCommand
        {
            Text = "commands"
        };

        // Act
        var response = await _commandManager.Execute(slashCommand);

        // Assert
        AssertCommandResult(response, "No commands found.");
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public async Task CommandsCommand_Add_And_List_Ok(bool isGlobal)
    {
        // Arrange
        var addCommand = new SlashCommand
        {
            Text = "commands add -command prompt" + (isGlobal
                ? " -global"
                : ""),
            UserId = "U0123ID"
        };
        var listCommand = new SlashCommand
        {
            Text = "commands",
            UserId = "U0123ID"
        };

        // Act
        var add = await _commandManager.Execute(addCommand);
        var list = await _commandManager.Execute(listCommand);

        // Assert
        AssertCommandResult(add, "Added command");
        AssertCommandResult(list, "-command");
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public async Task CommandsCommand_AddRemove_And_List_Ok(bool isGlobal)
    {
        // Arrange
        var addCommand = new SlashCommand
        {
            Text = "commands add -command prompt" + (isGlobal
                ? " -global"
                : ""),
            UserId = "U0123ID"
        };
        var removeCommand = new SlashCommand
        {
            Text = "commands remove -command",
            UserId = "U0123ID"
        };
        var listCommand = new SlashCommand
        {
            Text = "commands",
            UserId = "U0123ID"
        };

        // Act
        var add = await _commandManager.Execute(addCommand);
        var remove = await _commandManager.Execute(removeCommand);
        var list = await _commandManager.Execute(listCommand);

        // Assert
        AssertCommandResult(add, "Added command");
        AssertCommandResult(remove, "Removed command");
        AssertCommandResult(list, "No commands found.");
    }

    [Test]
    public async Task CommandsCommand_AddRemove_And_List_CannotRemove()
    {
        // Arrange
        var addCommand = new SlashCommand
        {
            Text = "commands add -command prompt",
            UserId = "U9999ID"
        };
        var removeCommand = new SlashCommand
        {
            Text = "commands remove -command",
            UserId = "U0123ID"
        };
        var listCommand = new SlashCommand
        {
            Text = "commands",
            UserId = "U0123ID"
        };

        // Act
        var add = await _commandManager.Execute(addCommand);
        var postAdd = await _commandManager.Execute(listCommand);
        var remove = await _commandManager.Execute(removeCommand);
        var list = await _commandManager.Execute(listCommand);

        // Assert
        AssertCommandResult(add, "Added command");
        AssertCommandResult(postAdd, "No commands found.");
        AssertCommandResult(remove, "not found.");
        AssertCommandResult(list, "No commands found.");
    }

    [Test]
    public async Task CommandsCommand_AddSameCommand_MultipleUsers_CannotRemove()
    {
        // Arrange
        var addCommand1 = new SlashCommand
        {
            Text = "commands add -command prompt1",
            UserId = "U9999ID"
        };
        var addCommand2 = new SlashCommand
        {
            Text = "commands add -command prompt2",
            UserId = "U0123ID"
        };
        var listCommand1 = new SlashCommand
        {
            Text = "commands",
            UserId = "U9999ID"
        };
        var listCommand2 = new SlashCommand
        {
            Text = "commands",
            UserId = "U0123ID"
        };
        var listCommandDetails1 = new SlashCommand
        {
            Text = "commands help -command",
            UserId = "U9999ID"
        };
        var listCommandDetails2 = new SlashCommand
        {
            Text = "commands help -command",
            UserId = "U0123ID"
        };

        // Act
        var add1 = await _commandManager.Execute(addCommand1);
        var add2 = await _commandManager.Execute(addCommand2);
        var list1 = await _commandManager.Execute(listCommand1);
        var list2 = await _commandManager.Execute(listCommand2);
        var listDetails1 = await _commandManager.Execute(listCommandDetails1);
        var listDetails2 = await _commandManager.Execute(listCommandDetails2);

        // Assert
        AssertCommandResult(add1, "Added command");
        AssertCommandResult(add2, "Added command");
        AssertCommandResult(list1, "-command");
        AssertCommandResult(list2, "-command");
        AssertCommandResult(listDetails1, "prompt1");
        AssertCommandResult(listDetails2, "prompt2");
    }

    [Test]
    public async Task WhatsNewCommand_Ok()
    {
        // Arrange
        var slashCommand = new SlashCommand
        {
            Text = "whatsNew v1.2.3"
        };

        // Act
        var response = await _commandManager.Execute(slashCommand);

        // Assert
        AssertCommandResult(response, "Added support for custom commands, and help at /gpt command");
    }
    
    
    [Test]
    public async Task WhatsNewCommand_Error()
    {
        // Arrange
        var slashCommand = new SlashCommand
        {
            Text = "whatsNew"
        };

        // Act
        var response = await _commandManager.Execute(slashCommand);

        // Assert
        AssertCommandResult(response, "unknown version");
    }
    
    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public async Task CommandsCommand_AddOverwrite_Ok(bool isGlobal)
    {
        // Arrange
        var addCommand1 = new SlashCommand
        {
            Text = $"commands add -command prompt1 {(isGlobal ? "-global" : "")}",
            UserId = "U0123ID"
        };
        var addCommand2 = new SlashCommand
        {
            Text = $"commands add -command prompt2 {(isGlobal ? "-global" : "")}",
            UserId = "U0123ID"
        };
        var listCommand = new SlashCommand
        {
            Text = "commands",
            UserId = "U0123ID"
        };
        var listCommandDetails = new SlashCommand
        {
            Text = "commands help -command",
            UserId = "U0123ID"
        };


        // Act
        var add1 = await _commandManager.Execute(addCommand1);
        var add2 = await _commandManager.Execute(addCommand2);
        var list = await _commandManager.Execute(listCommand);
        var listDetails = await _commandManager.Execute(listCommandDetails);

        // Assert
        AssertCommandResult(add1, "Added command");
        AssertCommandResult(add2, "already exists");
        AssertCommandResult(list, "-command");
        AssertCommandResult(listDetails, "prompt1");
    }
    
    [Test]
    public async Task CommandsCommand_RemoveSingle_Ok()
    {
        // Arrange
        var addCommand1 = new SlashCommand
        {
            Text = $"commands add -command prompt1 -global",
            UserId = "U0123ID"
        };
        var addCommand2 = new SlashCommand
        {
            Text = $"commands add -command prompt2",
            UserId = "U0123ID"
        };
        var listCommand = new SlashCommand
        {
            Text = "commands",
            UserId = "U0123ID"
        };
        var removeCommand = new SlashCommand
        {
            Text = "commands remove -command",
            UserId = "U0123ID"
        };

        // Act
        var add1 = await _commandManager.Execute(addCommand1);
        var add2 = await _commandManager.Execute(addCommand2);
        var list1 = await _commandManager.Execute(listCommand);
        var remove1 = await _commandManager.Execute(removeCommand);
        var list2 = await _commandManager.Execute(listCommand);

        // Assert
        AssertCommandResult(add1, "Added command");
        AssertCommandResult(add2, "already exists");
        AssertCommandResult(list1, "-command [Global]");
        AssertCommandResult(remove1, "Removed command -command.");
        AssertCommandResult(list2, "No commands found.");
    }
}