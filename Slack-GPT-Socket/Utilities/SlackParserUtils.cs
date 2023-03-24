using System.Text;
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
}