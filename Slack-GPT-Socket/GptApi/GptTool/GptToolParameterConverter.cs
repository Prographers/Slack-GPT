using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Slack_GPT_Socket.GptApi;

public class GptToolParameterConverter : JsonConverter<GptToolParameter>
{
    public override void WriteJson(JsonWriter writer, GptToolParameter? value, JsonSerializer serializer)
    {
        writer.WriteStartObject();

        // Write "type"
        writer.WritePropertyName("type");
        writer.WriteValue(value.Type.ToString().ToLower());

        // Write "description" if available
        if (!string.IsNullOrEmpty(value.Description))
        {
            writer.WritePropertyName("description");
            writer.WriteValue(value.Description);
        }

        // Write "enum" if available
        if (value.Enum != null && value.Enum.Count > 0)
        {
            writer.WritePropertyName("enum");
            serializer.Serialize(writer, value.Enum);
        }

        // Write "anyOf" if available
        if (value.AnyOf != null && value.AnyOf.Count > 0)
        {
            writer.WritePropertyName("anyOf");
            serializer.Serialize(writer, value.AnyOf);
        }

        // Write "properties" if available
        if (value.Properties != null && value.Properties.Count > 0)
        {
            writer.WritePropertyName("properties");
            writer.WriteStartObject();
            foreach (var prop in value.Properties)
            {
                writer.WritePropertyName(prop.Key);
                serializer.Serialize(writer, prop.Value);
            }

            writer.WriteEndObject();
        }

        // Write "required" if there are required properties
        if (value.Properties != null && value.Properties.Count > 0)
        {
            var requiredProps = value.Properties
                .Where(p => p.Value.Required)
                .Select(p => p.Key)
                .ToList();
            if (requiredProps.Count > 0)
            {
                writer.WritePropertyName("required");
                serializer.Serialize(writer, requiredProps);
            }
        }

        // Write "additionalProperties" if set to false and type object
        if (!value.AdditionalProperties && value.Type == PropertyType.Object)
        {
            writer.WritePropertyName("additionalProperties");
            writer.WriteValue(false);
        }

        writer.WriteEndObject();
    }

    public override GptToolParameter ReadJson(JsonReader reader, Type objectType, GptToolParameter existingValue,
        bool hasExistingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        var parameter = new GptToolParameter();

        if (jsonObject.TryGetValue("type", out var typeToken))
            if (Enum.TryParse(typeToken.Value<string>(), true, out PropertyType type))
                parameter.Type = type;

        if (jsonObject.TryGetValue("description", out var descriptionToken))
            parameter.Description = descriptionToken.Value<string>();

        if (jsonObject.TryGetValue("enum", out var enumToken) && enumToken.Type == JTokenType.Array)
            parameter.Enum = enumToken.ToObject<List<string>>();

        if (jsonObject.TryGetValue("anyOf", out var anyOfToken) && anyOfToken.Type == JTokenType.Array)
            parameter.AnyOf = anyOfToken.ToObject<List<GptToolParameter>>(serializer);

        if (jsonObject.TryGetValue("properties", out var propertiesToken) &&
            propertiesToken.Type == JTokenType.Object)
            parameter.Properties = propertiesToken.ToObject<Dictionary<string, GptToolParameter>>(serializer);

        if (jsonObject.TryGetValue("required", out var requiredToken) && requiredToken.Type == JTokenType.Array)
        {
            var requiredFields = requiredToken.ToObject<List<string>>();
            foreach (var requiredField in requiredFields)
            {
                if (parameter.Properties.ContainsKey(requiredField))
                    parameter.Properties[requiredField].Required = true;
            }
        }

        if (jsonObject.TryGetValue("additionalProperties", out var additionalPropertiesToken))
            parameter.AdditionalProperties = additionalPropertiesToken.Value<bool>();

        return parameter;
    }
}