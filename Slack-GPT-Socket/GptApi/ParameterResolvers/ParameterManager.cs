using System.Collections;
using Slack_GPT_Socket.Settings;
using Slack_GPT_Socket.Utilities.LiteDB;

namespace Slack_GPT_Socket.GptApi.ParameterResolvers;

public class ParameterManager : IEnumerable<IParameterResolver>
{
    private readonly List<IParameterResolver> _resolvers;

    /// <summary>
    ///     Model aliases for quick access
    /// </summary>
    private ModelInfo[] _models = new ModelInfo[]
    {
        new("o1-preview", "o1"),
        new("o1-mini", "o1-mini"),
        new("gpt-4o", "gpt4o"),
        new("gpt-4", "gpt4"),
        new("gpt-4o-mini", "gpt4o-mini"),
        new("gpt-4-turbo", "gpt4-turbo"),
        new("gpt-3.5-turbo", "chatgpt", "gpt-3", "gpt3", "turbo")
    };

    
    public ParameterManager(GptCustomCommands customCommands,
        GptDefaults gptDefaults, IUserCommandDb userCommandDb)
    {
        _resolvers = new List<IParameterResolver>
        {
            new MaxTokenResolver(),
            new TemperatureResolver(),
            new TopPResolver(),
            new PresencePenaltyResolver(),
            new FrequencyPenaltyResolver(),
            new ModelResolver(Models),
            new SystemResolver(),
            new ContextResolver(),
            new PredefinedCommandResolver(customCommands),
            new UsersCommandResolver(userCommandDb)
        };
    }

    /// <summary>
    ///     Gets the models that are available.
    /// </summary>
    public ModelInfo[] Models => _models;
    

    /// <summary>
    ///     Tries to resolve the parameter.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public bool TryResolve(GptRequest input, ParameterEventArgs args)
    {
        var resolved = false;
        foreach (var resolver in _resolvers)
        {
            if (!resolver.CanHandle(args)) continue;

            resolver.Resolve(input, args);
            resolved = true;
            if (!args.PassThrough) return resolved;
        }

        return resolved;
    }
    
    public int Count => _resolvers.Count;
    
    /// <summary>
    ///     Gets the resolver at the given index.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public IParameterResolver GetResolver(int index)
    {
        return _resolvers[index];
    }

    public IEnumerator<IParameterResolver> GetEnumerator()
    {
        return _resolvers.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}