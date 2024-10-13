using Newtonsoft.Json;
using OpenAI.Chat;

namespace Slack_GPT_Socket.GptApi;

/// <summary>
///     Base class for GPT tools. You can use dependency injection to create a new tool and add it to the GPT client.
///     Constructor must inherit from "this" parameterless constructor.
/// </summary>
public abstract class BaseGptTool: IExpressionGptTool
{
    /// <summary>
    ///     The name of the tool.
    /// </summary>
    public abstract string Name { get; }
    
    /// <summary>
    ///     The aliases of the tool, which can be used to call the tool.
    /// </summary>
    public virtual string[] Aliases { get; } = Array.Empty<string>();

    /// <summary>
    ///     Call the expression with the given parameters.
    /// </summary>
    /// <param name="jsonParameters"></param>
    /// <returns></returns>
    public abstract Task<CallExpressionResult> CallExpressionInternal(string jsonParameters, Func<string, Type, object>  deserialize);

    /// <summary>
    ///     The description of the tool, for use by the model to choose when and how to call the tool.
    /// </summary>
    public abstract string Description { get; }

    /// <summary>
    ///     If you turn on Structured Outputs by supplying strict: true and call the API with an unsupported JSON Schema, you
    ///     will receive an error.
    /// </summary>
    public virtual bool IsStrict { get; set; }

    /// <summary>
    ///     The parameters that the tool accepts, which are described as a JSON schema.
    /// </summary>
    public GptToolParameter Parameters { get; set; } = new();
    
    public static ChatTool ToChatTool(BaseGptTool tool)
    {
        var parameters = BinaryData.FromString(JsonConvert.SerializeObject(tool.Parameters));
        return ChatTool.CreateFunctionTool(tool.Name, tool.Description, parameters, tool.IsStrict);
    }
}