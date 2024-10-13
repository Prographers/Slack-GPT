using System.Text;
using Slack_GPT_Socket.GptApi;
using SlackNet;
using SlackNet.Blocks;
using SlackNet.Events;

namespace Slack_GPT_Socket;

/// <summary>
///     Utility class for parsing Slack responses.
/// </summary>
public static class SlackParserUtils
{
    /// <summary>
    ///     Removes the context block from the response.
    /// </summary>
    /// <param name="reply"></param>
    /// <returns></returns>
    public static string? RemoveContextBlockFromResponses(MessageEvent? reply)
    {
        if(reply == null) return null;
        
        var blocks = reply.Blocks;
        if (blocks == null) return reply.Text;

        var contextBlock = blocks.FirstOrDefault(b => b.Type == "context");
        if (contextBlock == null) return reply.Text;

        blocks.Remove(contextBlock);
    
        return ConvertBlocksToText(blocks);
    }
    
    /// <summary>
    ///     Converts a given text string into a list of blocks for Slack message formatting.
    /// </summary>
    /// <param name="text">The text string to be converted into blocks.</param>
    /// <returns>A list of blocks containing the text converted into a section block.</returns>
    public static List<Block> ConvertTextToBlocks(string text)
    {
        var blocks = new List<Block>();

        blocks.Add(new SectionBlock
        {
            Text = new Markdown(text)
        });

        return blocks;
    }

    /// <summary>
    ///     Converts a list of blocks to text.
    /// </summary>
    /// <param name="blocks">Blocks to convert from</param>
    /// <returns>Returns string built from blocks</returns>
    private static string? ConvertBlocksToText(IList<Block> blocks)
    {
        var sb = new StringBuilder();
        foreach (var block in blocks)
        {
            switch (block)
            {
                case SectionBlock sectionBlock:
                    sb.Append(sectionBlock.Text);
                    break;
            }
        }
        
        return sb.ToString();
    }

    /// <summary>
    ///     Attaches files to the given blocks.
    /// </summary>
    /// <param name="slack"></param>
    /// <param name="blocks"></param>
    /// <param name="response"></param>
    /// <exception cref="NotImplementedException"></exception>
    public static async Task AttachFilesToBlocks(ISlackApiClient slack, List<Block> blocks, GptResponse response)
    {
        if (response.FileAttachments.Count == 0) return;

        foreach (var file in response.FileAttachments)
        {
            var fileUploadResponse = await slack.Files.Upload(
                file.Data, 
                file.MimeType, 
                file.Name, 
                file.Title);

            if (file.IsImage)
            {
                blocks.Add(new ImageBlock
                {
                    Title = file.Title,
                    ImageUrl = fileUploadResponse.File.Permalink,
                    AltText = file.Title,
                });
            }
            else
            {
                blocks.Add(new FileBlock
                {
                    ExternalId = fileUploadResponse.File.Id,
                    Source = fileUploadResponse.File.Permalink,
                    BlockId = "file_block_" + fileUploadResponse.File.Id,
                });
            }
        }
    }
}