namespace Slack_GPT_Socket.GptApi.ParameterResolvers;

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
    public string Name => "-model";

    /// <inheritdoc />
    public bool CanHandle(ParameterEventArgs args)
    {
        var names = new[]
        {
            "-model",
            "-m"
        };
        return names.Contains(args.Name.ToLower());
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