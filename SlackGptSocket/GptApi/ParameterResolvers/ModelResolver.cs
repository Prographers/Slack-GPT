using SlackGptSocket.Settings;

namespace SlackGptSocket.GptApi.ParameterResolvers;

/// <summary>
///     Represents a parameter resolver for the model parameter.
/// </summary>
public class ModelResolver : IParameterResolver
{
    private readonly ModelInfo[] _models;

    public ModelResolver(ModelInfo[] models)
    {
        _models = models;
    }

    /// <inheritdoc />
    public static string[] Names { get; } =
    {
        "-model",
        "-m"
    };

    /// <inheritdoc />
    public string Name => Names[0];

    /// <inheritdoc />
    public string BuildShortHelpText(GptDefaults gptDefaults, string userId)
    {
        return
            $"{Name}: specifies model, default {(gptDefaults.Model ?? "gpt-4").ToLower()}, options: gpt-4, gpt-3.5-turbo";
    }

    /// <inheritdoc />
    public string BuildHelpText(GptDefaults gptDefaults, string commandName, string userId)
    {
        var names = string.Join(", ", Names);
        var models = string.Join(", ", _models.Select(x => x.Model));
        return
            $"{names}: specifies model, default {(gptDefaults.Model ?? "gpt-4").ToLower()}, options: gpt-4, gpt-3.5-turbo\n" +
            $"Available models: {models}";
    }

    /// <inheritdoc />
    public bool CanHandle(ParameterEventArgs args)
    {
        return Names.Contains(args.Name.ToLower());
    }

    /// <inheritdoc />
    public void Resolve(GptRequest input, ParameterEventArgs args)
    {
        var testValue = args.Value.ToLower();

        foreach (var modelInfo in _models)
        {
            var model = modelInfo.Model;
            var aliases = modelInfo.Aliases;

            if (model != testValue && !aliases.Contains(testValue))
                continue;

            // remove resolved model name from prompt and set model property of input
            input.Model = model;
            return;
        }

        // if no model was found, set args as has value to false
        args.HasValue = false;
    }
}