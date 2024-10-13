using FluentAssertions;
using Newtonsoft.Json;
using OpenAI.Chat;
using Slack_GPT_Socket.GptApi;

namespace Slack_GPT_Tests.GptApi;

[TestFixture]
public class GptToolJsonTests
{
    [Test]
    public void TestGptTool_Json()
    {
        var tool = new TestGptTool();
        
        var functionName = "weather";
        var description = "Get current weather at location";
        var isStrict = true;
        var jsonParameter =
            """"
            {
                "type": "object",
                "properties": {
                    "location": {
                        "type": "string",
                        "description": "The city and state, e.g. San Francisco, CA",
                    },
                    "unit": {
                        "type": "string", 
                        "description": "The unit to return the temperature in",
                        "enum": ["F", "C"]
                    },
                },
                "required": ["location"],
                "additionalProperties": false
            }
            """";
        
        tool.Name.Should().Be(functionName);
        tool.Description.Should().Be(description);
        tool.IsStrict.Should().Be(isStrict);
        JsonConvert.DeserializeObject<GptToolParameter>(jsonParameter).Should().BeEquivalentTo(tool.Parameters);

        var toolParameters = JsonConvert.SerializeObject(tool.Parameters);
        Beautify(toolParameters).Should().Be(Beautify(jsonParameter));
        
        var chatTool = BaseGptTool.ToChatTool(tool);
        chatTool.Kind.Should().Be(ChatToolKind.Function);
        chatTool.FunctionName.Should().Be(functionName);
        chatTool.FunctionDescription.Should().Be(description);
        chatTool.FunctionParameters.ToString().Should().Be(toolParameters);
    }
    
    public static string Minify(string json)
    {
        return ReformatJson(json, Formatting.None);
    }

    public static string Beautify(string json)
    {
        return ReformatJson(json, Formatting.Indented);
    }

    public static string ReformatJson(string json, Formatting formatting)
    {
        using (StringReader stringReader = new StringReader(json))
        using (StringWriter stringWriter = new StringWriter())
        {
            ReformatJson(stringReader, stringWriter, formatting);
            return stringWriter.ToString();
        }
    }

    public static void ReformatJson(TextReader textReader, TextWriter textWriter, Formatting formatting)
    {
        using (JsonReader jsonReader = new JsonTextReader(textReader))
        using (JsonWriter jsonWriter = new JsonTextWriter(textWriter))
        {
            jsonWriter.Formatting = formatting;
            jsonWriter.WriteToken(jsonReader);
        }
    }
}

file class TestGptTool : BaseGptTool
{
    public TestGptTool()
    {
        Parameters.Properties = new();
        Parameters.Properties.Add("location", new GptToolParameter
        {
            Type = PropertyType.String,
            Description = "The city and state, e.g. San Francisco, CA",
            Required = true
        });

        Parameters.Properties.Add("unit", new GptToolParameter
        {
            Type = PropertyType.String,
            Description = "The unit to return the temperature in",
            Enum = ["F", "C"]
        });
    }

    public override string Name => "weather";

    public override async Task<CallExpressionResult> CallExpressionInternal(string jsonParameters, Func<string, Type, object> deserialize)
    {
        throw new NotImplementedException();
    }

    public override string Description => "Get current weather at location";
    
    public override bool IsStrict => true;
}