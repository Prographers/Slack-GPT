using Newtonsoft.Json;

namespace Slack_GPT_Socket.GptApi;

[JsonConverter(typeof(GptToolParameterConverter))]
public class GptToolParameter
{
    /// <summary>
    ///     The type of the object.
    /// </summary>
    [JsonProperty("type")]
    public PropertyType Type { get; set; } = PropertyType.Object;
    
    /// <summary>
    ///     Index of the object in the calling method.
    /// </summary>
    [JsonIgnore]
    public int Index { get; set; }

    /// <summary>
    ///     The properties of the object.
    /// </summary>
    [JsonProperty("properties")]
    public Dictionary<string, GptToolParameter>? Properties { get; set; }

    /// <summary>
    ///     Possible values for the object.
    /// </summary>
    [JsonProperty("enum")]
    public List<string>? Enum { get; set; }

    /// <summary>
    ///     The items to select from structured output possiblities.
    /// </summary>
    [JsonProperty("anyOf")]
    public List<GptToolParameter>? AnyOf { get; set; }

    /// <summary>
    ///     The description of the object, for use by the model to choose when and how to call the tool.
    /// </summary>
    [JsonProperty("description")]
    public string Description { get; set; }

    /// <summary>
    ///     The required properties of the object.
    /// </summary>
    [JsonProperty("required")]
    public bool Required { get; set; }

    /// <summary>
    ///     Whether to allow additional properties.
    /// </summary>
    [JsonProperty("additionalProperties")]
    public bool AdditionalProperties { get; set; }
}