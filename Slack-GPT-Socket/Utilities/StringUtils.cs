using System.Text;
using Humanizer;

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

    /// <summary>
    ///     Sanitizes a string to only contain numbers and symbols.
    /// </summary>
    /// <param name="input">String input that should be parsed as number eventualy</param>
    /// <returns></returns>
    public static string SanitizeNumber(this string input)
    {
        return string.Join("", input.Where(x => char.IsDigit(x) || char.IsSymbol(x) 
                        || char.IsPunctuation(x) || char.IsSeparator(x))).Replace(",", ".");
    }
    
    /// <summary>
    ///     Checks if a string contains any of the given values.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    public static bool Contains(this string input, IEnumerable<string> values)
    {
        return values.Any(input.Contains);
    }
    
    /// <summary>
    ///     Normalizes a parameter name for better comparison.
    /// </summary>
    /// <param name="parameterName"></param>
    /// <returns></returns>
    public static string GetNormalizedParameter(this string parameterName)
    {
        // Remove leading '-'
        if (parameterName.StartsWith("-")) parameterName = parameterName.Substring(1);

        // Pascalize the name (converts 'no-tools', 'no_tools', 'notools' to 'NoTools')
        parameterName = parameterName.Pascalize();

        // Depluralize (converts 'NoTools' to 'NoTool')
        var singularName = parameterName.Singularize(false);

        // Convert to lower case for case-insensitive comparison
        return singularName.ToLowerInvariant();
    }
}