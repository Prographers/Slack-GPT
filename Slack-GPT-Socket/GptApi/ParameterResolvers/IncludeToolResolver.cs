using Humanizer;
using Slack_GPT_Socket.Settings;

namespace Slack_GPT_Socket.GptApi.ParameterResolvers;

public class IncludeToolResolver : IParameterResolver
{
    public static string[] Names { get; } =
    [
        "-tool",
        "-add-tool",
        "-enable-tool",
        "-attach-tool",
        "-use-tool",
        "-tool-enable",
        "-tool-add",
        "-tool-attach",
        "-tool-use",
        "-use"
    ];

    public string Name => "-tool";
    
    public string BuildShortHelpText(GptDefaults gptDefaults, string userId)
    {
        return $"{Name}: Adds a tool to the output. Some tools are enabled by default some need this flag to be explicitly enabled.";
    }
    
    public string BuildHelpText(GptDefaults gptDefaults, string commandName, string userId)
    {
        var namesList = string.Join(", ", Names);
        return $"{namesList}\n\t: Adds a tool to the output. Some tools are enabled by default some need this flag to be explicitly enabled.\n";
    }
    
    public bool CanHandle(ParameterEventArgs args)
    {
        var parameterName = args.Name;
        if (!parameterName.StartsWith("-")) return false;

        parameterName = args.Name.GetNormalizedParameter();
        return Names.Any(name => parameterName == name.GetNormalizedParameter());
    }

    public void Resolve(GptRequest input, ParameterEventArgs args)
    {
        input.Tools.Add(args.Value);
    }
}