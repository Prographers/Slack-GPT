using Humanizer;
using Slack_GPT_Socket.Settings;

namespace Slack_GPT_Socket.GptApi.ParameterResolvers;

public class NoToolsResolver : IParameterResolver
{
    public static string[] Names { get; } =
    [
        "-no-tools",
        "-disable-tools"
    ];

    public string Name => "-noTools";

    public string BuildShortHelpText(GptDefaults gptDefaults, string userId)
    {
        return $"{Name}: Disables tools in output (some models may not support this). Enabled by default.";
    }

    public string BuildHelpText(GptDefaults gptDefaults, string commandName, string userId)
    {
        var namesList = string.Join(", ", Names);
        return $"{namesList}\n\t: Disables tools in output (some models may not support this). Enabled by default.\n";
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
        input.NoTools = true;
    }
}