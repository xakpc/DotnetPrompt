using System.Text;
using System.Text.RegularExpressions;

namespace DotnetPrompt.Test.Examples;

public class PythonHelpers
{
    public static IEnumerable<(string Name, string Def, string Body)> GetPythonMethods(string filePath)
    {
        var regex = new Regex(@"(def\s+(\w+)\([\w,:*\s]+\)\s+->\s+([\w\[\]]+):\n)(\s{8,}.+\n)+", RegexOptions.Compiled|RegexOptions.Multiline);

        var fileContent = File.ReadAllText(filePath);
        var matches = regex.Matches(fileContent);

        foreach (Match match in matches)
        {
            var methodName = match.Groups[1].Value;
            var methodArgs = match.Groups[2].Value;

            var sb = new StringBuilder();
            foreach (Capture capture in match.Groups[4].Captures)
            {
                sb.Append(capture.Value);
            }
            var methodBody = sb.ToString();
            yield return (methodArgs, methodName, methodBody);
        }
    }
}