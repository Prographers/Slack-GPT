using System.Text.RegularExpressions;
using GptCore.Database;
using GptCore.Settings;
using SlackGptSocket.Settings;
using SlackGptSocket.Utilities.LiteDB;
using SlackNet.Interaction;

namespace SlackGptSocket.SlackHandlers.Command;

/// <summary>
///     Custom command to list all commands, allows also for adding and removing commands.
/// </summary>
public class CommandsCommandStrategy : ICommandStrategy
{
    private readonly IUserCommandDb _userCommandDb;

    public CommandsCommandStrategy(IUserCommandDb userCommandDb)
    {
        _userCommandDb = userCommandDb;
    }

    public string Command => "commands";

    /// <summary>
    ///     Execute commands command
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    public async Task<SlashCommandResponse> Execute(SlashCommand command)
    {
        var restOfCommand = command.Text.Substring(Command.Length).Trim();
        var commandName = restOfCommand.Split(" ")[0];
        restOfCommand = restOfCommand.Substring(commandName.Length).Trim();

        switch (commandName)
        {
            case "": // List all commands
                return ListCommands(command);
            case "add": // Add a command
                return AddCommand(command, restOfCommand);
            case "remove": // Remove a command
                return RemoveCommand(command, restOfCommand);
            case "help": // Help for this command
                return HelpCommand(command, restOfCommand);
        }

        return CommandStrategyUtils.SlashCommandResponse("Command not found. Type /help or /generate to get started.");
    }

    /// <summary>
    ///     List help for commands prompt or details for a specific command
    /// </summary>
    /// <param name="command"></param>
    /// <param name="commandToHelp"></param>
    /// <returns></returns>
    private SlashCommandResponse HelpCommand(SlashCommand command, string? commandToHelp)
    {

        if (string.IsNullOrWhiteSpace(commandToHelp))
        {
            return CommandStrategyUtils.SlashCommandResponse(
            "Usage: /commands [add|remove|help] [command] [prompt] [description] [options: -global]\n" +
            "Examples:\n" +
            "/commands add -test \"This is a test\" \"This is a test command\"\n" +
            "/commands remove -test\n" +
            "/commands help\n" +
            "/commands help -test\n" +
            "/commands");
        }
        else
        {
            var commandHelp = _userCommandDb.FindCommand(commandToHelp, command.UserId);
            return CommandStrategyUtils.SlashCommandResponse(
                $"{commandHelp.Command}\n" +
                $"\tPrompt: {commandHelp.Prompt}\n" +
                $"\tDescription: {commandHelp.Description}\n" +
                $"\tIs Global: {(commandHelp.UserId == null)}\n" +
                $"\tAs System: {commandHelp.AsSystem}");
        }
    }

    /// <summary>
    ///     Removes a command
    /// </summary>
    /// <param name="command"></param>
    /// <param name="commandToRemove"></param>
    /// <returns></returns>
    private SlashCommandResponse RemoveCommand(SlashCommand command, string commandToRemove)
    {
        var toRemove = _userCommandDb.FindCommand(commandToRemove, command.UserId);
        if (toRemove == null) return CommandStrategyUtils.SlashCommandResponse($"Command {commandToRemove} not found.");
        _userCommandDb.RemoveCommand(toRemove);
        return CommandStrategyUtils.SlashCommandResponse($"Removed command {commandToRemove}.");
    }

    /// <summary>
    ///     New command should be in the format "add command prompt description" or "add command prompt"
    ///     Prompt and Description can have " " in them, so we need to split on " " and then recombine the last two
    ///     You can escape " " with \ within the prompt and description. Eg: add test "this is a \"test\""
    /// </summary>
    /// <param name="command"></param>
    /// <param name="restOfCommand"></param>
    /// <returns></returns>
    private SlashCommandResponse AddCommand(SlashCommand command, string restOfCommand)
    {
        var commandToAdd = restOfCommand;
        var isGlobal = false;
        var matches = Regex.Matches(commandToAdd,
            @"[^\s""']+|""([^""\\]*(?:\\.[^""\\]*)*)""|'([^'\\]*(?:\\.[^'\\]*)*)'");

        var splitCommand = matches.Select(m => m.Value).ToArray();

        // If there is "-global" parameter after prompt or description, then this is a global command.
        // There can be more then one option
        var options = splitCommand[1..].Where(c => c.StartsWith("-")).ToArray();
        if (options.Contains("-global")) isGlobal = true;


        GptUserCommand newCommand = new()
        {
            Command = splitCommand[0],
            Description = splitCommand.Length > 2
                ? splitCommand[2]
                : "",
            Prompt = splitCommand.Length > 1
                ? splitCommand[1]
                : "",
            UserId = isGlobal
                ? null
                : command.UserId
        };
        if (!_userCommandDb.AddCommand(newCommand))
        {
            return CommandStrategyUtils.SlashCommandResponse(
                $"Command {newCommand.Command} already exists. " +
                $"Use /commands remove {newCommand.Command} to remove it first.");
        }
        return CommandStrategyUtils.SlashCommandResponse(
            $"Added command {newCommand.Command} described as {newCommand.Description}\n" +
            $"\tPrompt:{newCommand.Prompt}.\n" +
            $"\tGlobal: {isGlobal}");
    }

    /// <summary>
    ///     List all commands
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    private SlashCommandResponse ListCommands(SlashCommand command)
    {
        var commands = _userCommandDb.GetAllCommands(command.UserId);
        var globalCommands = _userCommandDb.GetAllCommands();
        var allCommands = commands.Concat(globalCommands).ToList();

        if (!allCommands.Any()) return CommandStrategyUtils.SlashCommandResponse("No commands found.");

        var commandList = string.Join("\n",
            allCommands.Select(c => $"{c.Command} [{(c.UserId == null ? "Global" : "User")}]"));
        return CommandStrategyUtils.SlashCommandResponse(commandList);
    }
}