using System.Text;
using Slack_GPT_Socket.Settings;

namespace Slack_GPT_Socket.GptApi.ParameterResolvers;

/// <summary>
///     Represents a parameter resolver for the context parameter.
///     Context parameter is passed to the next request, unless it is cleared.
/// </summary>
public class ContextResolver : IParameterResolver
{
    /// <inheritdoc />
    public static string[] Names { get; } =
    {
        "-context"
    };
    
    /// <summary>
    ///     Represents a list of synonymous commands for clearing the context.
    /// </summary>
    public static string[] ClearActionsNames { get; } =
    {
        "clear",
        "null",
        "none",
        "reset",
        "empty",
        ""
    };

    /// <inheritdoc />
    public string Name => "-context";

    /// <inheritdoc />
    public string BuildShortHelpText(GptDefaults gptDefaults, string userId)
    {
        return $"{Name}: similar to system message, but it is persistent for a duration of the conversation. You can clear the context" +
               "by applying -context clear. You can temporarily overwrite context by applying system message. Only latest context message" +
               "is used, they do not stack.";
    }

    /// <inheritdoc />
    public string BuildHelpText(GptDefaults gptDefaults, string commandName, string userId)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{Name}: similar to system message, but it is persistent for a duration of the conversation. You can clear the context" +
                      "by applying -context clear. You can temporarily overwrite context by applying system message. Only latest context message" +
                      "is used, they do not stack.");
        sb.AppendLine("Clear synonymous commands:");
        foreach (var clearActionName in ClearActionsNames)
        {
            sb.AppendLine($"-context {clearActionName}");
        }

        return sb.ToString();
    }

    /// <inheritdoc />
    public bool CanHandle(ParameterEventArgs args)
    {
        return Names.Contains(args.Name.ToLower());
    }

    /// <inheritdoc />
    public void Resolve(GptRequest input, ParameterEventArgs args)
    {
        if (ClearActionsNames.Contains(args.Value.ToLower()))
        {
            input.System.IsContextMessage = ContextMessageStatus.Cleared;
            return;
        }

        input.System.IsContextMessage = ContextMessageStatus.Set;
        input.System.Replace(args.Value);
    }
}