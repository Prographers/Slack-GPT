using System.Text;

namespace Slack_GPT_Socket;

public static class StringUtils
{
    /// <summary>
    ///     Split a string into parts of a given length, to fit into a Slack message.
    /// </summary>
    /// <param name="s">String to split</param>
    /// <param name="partLength">Length of a single part</param>
    /// <returns>Returns split messages while keeping broken code blocks consistent</returns>
    public static List<string> SplitInParts(this string s, int partLength)
    {
        var result = new List<string>();
        var sb = new StringBuilder();
        var insideCodeBlock = false;
        var isCodeBlockBroken = false;
        var partCurrentLength = 0;

        for (var i = 0; i < s.Length; i++)
        {
            var c = s[i];

            if (c == '`' && (i == 0 || s[i - 1] != '\\'))
            {
                if (isCodeBlockBroken && !insideCodeBlock)
                {
                    sb.Append(c);
                    partCurrentLength++;
                    insideCodeBlock = !insideCodeBlock;
                    continue;
                }

                insideCodeBlock = !insideCodeBlock;
            }

            if (partCurrentLength >= partLength)
            {
                if (insideCodeBlock)
                {
                    sb.Append("```\n");
                    isCodeBlockBroken = true;
                }

                result.Add(sb.ToString());
                sb.Clear();
                partCurrentLength = 0;

                if (insideCodeBlock)
                {
                    sb.Append("```\n");
                    partCurrentLength += 4;
                }
            }

            sb.Append(c);
            partCurrentLength++;
        }

        if (sb.Length > 0) result.Add(sb.ToString());

        return result;
    }
}