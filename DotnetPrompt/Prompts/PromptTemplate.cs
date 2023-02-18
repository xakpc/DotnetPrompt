using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DotnetPrompt.Abstractions.Prompts;

namespace DotnetPrompt.Prompts;

/// <summary>
/// Schema to represent a prompt for an LLM based on formatted string
/// </summary>
public class PromptTemplate : IPromptTemplate
{
    /// <summary>
    /// Build PromptTemplate Schema from template string
    /// </summary>
    /// <param name="template"></param>
    public PromptTemplate(string template)
    {
        template = template.Replace("{{", "\a").Replace("}}", "\a\a"); // todo: find a better way
        InputVariables = ExtractWordsInBrackets(template).Distinct().ToList();
        template = template.Replace("\a\a", "}").Replace("\a", "{");

        Template = template;
    }

    // todo: what the point of this ctor?
    /// <summary>
    /// Build PromptTemplate Schema from template string and inputVariables
    /// </summary>
    /// <param name="template">Template string</param>
    /// <param name="inputVariables">List of input variables</param>
    /// <exception cref="ArgumentException">Throws when list of input variables is invalid</exception>
    public PromptTemplate(string template, IList<string> inputVariables)
    {
        template = template.Replace("{{", "\a").Replace("}}", "\a\a"); // todo: find a better way
        var extractedInputVariables = ExtractWordsInBrackets(template).Distinct().ToList();
        template = template.Replace("\a\a", "}").Replace("\a", "{");

        if (!extractedInputVariables.SequenceEqual(inputVariables))
        {
            throw new ArgumentException("Template and input arguments are different");
        }

        InputVariables = inputVariables;
        Template = template;
    }

    /// <summary>
    /// The prompt template.
    /// </summary>
    public string Template { get; set; }

    /// <summary>
    /// A list of the names of the variables the prompt template expects.
    /// </summary>
    public IList<string> InputVariables { get; set; }

    /// <summary>
    /// Take examples in list format with prefix and suffix to create a prompt.
    /// Intended be used as a way to dynamically create a prompt from examples
    /// </summary>
    /// <param name="prefix">String that should go before any examples. Generally includes examples. Default to an empty string</param>
    /// <param name="examples">List of examples to use in the prompt.</param>
    /// <param name="suffix">String to go after the list of examples. Should generally set up the user's input.</param>
    /// <param name="inputVariables">A list of variable names the final prompt template will expect.</param>
    /// <param name="exampleSeparator">The separator to use in between examples. Defaults to two new line characters.</param>
    /// <returns>The final prompt generated.</returns>
    /// <remarks>Verbatum string on windows will produce `\r\n` and that need to be handled somehow (todo).</remarks>
    public static PromptTemplate FromExamples(List<string> examples, string suffix,
        List<string> inputVariables, string prefix = "", string exampleSeparator = "\n\n")
    {
        var sb = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(prefix))
        {
            sb.Append(prefix);
            sb.Append(exampleSeparator);
        }

        sb.AppendJoin(exampleSeparator, examples);

        if (!string.IsNullOrWhiteSpace(suffix))
        {
            sb.Append(exampleSeparator);
            sb.Append(suffix);
        }

        return new PromptTemplate(sb.ToString(), inputVariables);
    }

    /// <summary>
    /// Load a prompt from a file.
    /// </summary>
    /// <param name="templateFile">The path to the file containing the prompt template.</param>
    /// <param name="inputVariables">A list of variable names the final prompt template will expect.</param>
    /// <returns>The prompt loaded from the file.</returns>
    public static PromptTemplate FromFile(string templateFile, string[] inputVariables)
    {
        var file = File.ReadAllText(templateFile);
        return new PromptTemplate(file, inputVariables);
    }

    /// <summary>
    /// Build a prompt from current template and a list of values.
    /// </summary>
    /// <param name="values">Key-Value list of values to use to build prompt.</param>
    /// <returns>String prompt</returns>
    /// <exception cref="ArgumentException">Throws when provided list of keys does not match <see cref="InputVariables"/>.</exception>
    public string Format(IDictionary<string, string> values)
    {
        if (!InputVariables.All(x => values.Keys.Contains(x))) // check if all elements of inputVariables are present in valuesKeys
        {
            throw new ArgumentException("parameters and input arguments are different ");
        }

        return values.Aggregate(Template, (s, kv) => s.Replace($"{{{kv.Key}}}", kv.Value));
    }

    /// <summary>
    /// Build a prompt from current template and a list of value tuples.
    /// </summary>
    /// <param name="values">Tuple of key - value</param>
    /// <returns></returns>
    public string Format((string, string)[] values)
    {
        return Format(values.ToDictionary(k => k.Item1, v => v.Item2));
    }

    /// <summary>
    /// Extract words in curly brackets as a list
    /// </summary>
    private static IEnumerable<string> ExtractWordsInBrackets(string inputString)
    {
        var startIndex = inputString.IndexOf("{", StringComparison.OrdinalIgnoreCase);
        while (startIndex != -1)
        {
            var endIndex = inputString.IndexOf("}", startIndex, StringComparison.OrdinalIgnoreCase);
            if (endIndex != -1)
            {
                var wordLength = endIndex - startIndex - 1;
                if (wordLength > 0)
                {
                    var word = inputString.Substring(startIndex + 1, wordLength);
                    yield return word;
                }
                startIndex = inputString.IndexOf("{", endIndex, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                yield break;
            }
        }
    }
}