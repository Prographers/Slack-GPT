using System.Runtime.Serialization;
using Humanizer;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OpenAI;
using OpenAI.Images;
using Slack_GPT_Socket.GptApi;

namespace Slack_GPT_Socket.Functions;

[JsonConverter(typeof(StringEnumConverter))]
public enum QualityEnum
{
    [EnumMember(Value = "standard")] Standard,
    [EnumMember(Value = "hd")] HighDefinition
}

[JsonConverter(typeof(StringEnumConverter))]
public enum SizeEnum
{
    Square,
    Portrait,
    Landscape
}

[JsonConverter(typeof(StringEnumConverter))]
public enum StyleEnum
{
    [EnumMember(Value = "vivid")] Vivid,
    [EnumMember(Value = "natural")] Natural
}

/// <summary>
///     Generate an image based on the given prompt, returns the URL of the generated image.
/// </summary>
public partial class GenerateImageTool : BaseGptTool, IExpressionGptTool<string, string, QualityEnum?, SizeEnum?, StyleEnum?>
{
    private readonly OpenAIClient _api;

    public override string[] Aliases { get; } =
    {
        "image",
        "generate-image",
    };

    public GenerateImageTool(OpenAIClient api) : this()
    {
        _api = api;
    }

    /// <summary>
    ///     Generate an image based on the given prompt, returns the byte array of the generated image, that is then
    ///     attached AUTOMATICALLY to the response message. Do not mention the image in the response message.
    /// </summary>
    /// <param name="prompt">
    ///     Prompt field guides the AI in generating images by providing a clear, concise description of the desired
    ///     scene or subject, including specific details like the setting, style, and mood. Use descriptive language to specify
    ///     key elements, context, and artistic styles to ensure the resulting image closely aligns with your vision.
    /// </param>
    /// <param name="name">
    ///     Short image name that will be used as file name. Use hyphen and leave out extension format. Use
    ///     hyphens to mark words
    /// </param>
    /// <param name="quality">
    ///     The quality of the image that will be generated. hd creates images with finer details and greater
    ///     consistency across the image.
    /// </param>
    /// <param name="size">The size of the generated images, default is square</param>
    /// <param name="style">
    ///     The style of the generated images. Vivid causes the model to lean towards generating hyper-real and
    ///     dramatic images. Natural causes the model to produce more natural, less hyper-real looking images. Defaults to
    ///     vivid
    /// </param>
    /// <returns></returns>
    public async Task<CallExpressionResult> CallExpression(string prompt, string name, QualityEnum? quality,
        SizeEnum? size, StyleEnum? style)
    {
        var client = _api.GetImageClient("dall-e-3");

        GeneratedImageQuality? generatedImageQuality = quality switch
        {
            QualityEnum.Standard => GeneratedImageQuality.Standard,
            QualityEnum.HighDefinition => GeneratedImageQuality.High,
            _ => default
        };

        GeneratedImageSize? generatedImageSize = size switch
        {
            SizeEnum.Square => GeneratedImageSize.W1024xH1024,
            SizeEnum.Portrait => GeneratedImageSize.W1024xH1792,
            SizeEnum.Landscape => GeneratedImageSize.W1792xH1024,
            _ => default
        };

        GeneratedImageStyle? generatedImageStyle = style switch
        {
            StyleEnum.Vivid => GeneratedImageStyle.Vivid,
            StyleEnum.Natural => GeneratedImageStyle.Natural,
            _ => default
        };

        var generateImageAsync = await client.GenerateImageAsync(prompt, new ImageGenerationOptions
        {
            Quality = generatedImageQuality,
            Size = generatedImageSize,
            Style = generatedImageStyle,
            ResponseFormat = GeneratedImageFormat.Bytes
        });

        var imageBytes = generateImageAsync.Value.ImageBytes;
        var revisedPrompt = generateImageAsync.Value.RevisedPrompt;
        
        using var memoryStream = new MemoryStream();
        await imageBytes.ToStream().CopyToAsync(memoryStream);
        var bytes = memoryStream.ToArray();

        return new CallExpressionResult
        {
            TextResponse = "Generated using prompt: " + revisedPrompt + ". Image data is part of the message, do not embed it in the message.",
            Files = new List<FileAttachment>
            {
                new()
                {
                    MimeType = "image/png",
                    Data = bytes,
                    Name = $"{name}.png",
                    Title = name.Humanize()
                }
            }
        };
    }
}