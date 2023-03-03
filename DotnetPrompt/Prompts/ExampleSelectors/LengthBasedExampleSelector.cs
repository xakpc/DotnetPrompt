using System;
using System.Collections.Generic;
using System.Linq;
using DotnetPrompt.Abstractions.Prompts;

namespace DotnetPrompt.Prompts.ExampleSelectors;

/// <summary>
/// Select examples for <see cref="FewShotPromptTemplate"/> based on length.
/// </summary>
public class LengthBasedExampleSelector : IExampleSelector
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="examples">A list of the examples that the prompt template expects.</param>
    /// <param name="examplePrompt">Prompt template used to format the examples.</param>
    public LengthBasedExampleSelector(IList<IDictionary<string, string>> examples, IPromptTemplate examplePrompt)
    {
        Examples = examples;
        ExamplePrompt = examplePrompt;

        _exampleTextLengths = examples.Select(examplePrompt.Format).Select(GetTextLength).ToList();
    }

    /// <summary>
    /// A list of the examples that the prompt template expects.
    /// </summary>
    private IList<IDictionary<string, string>> Examples { get; }

    /// <summary>
    /// Prompt template used to format the examples.
    /// </summary>
    private IPromptTemplate ExamplePrompt { get; }

    /// <summary>
    /// Function to measure prompt length. Defaults to word count.
    /// </summary>
    public Func<string, int> GetTextLength { get; init; } = s => s.Split(new [] {'\n',' '}, StringSplitOptions.RemoveEmptyEntries).Length;

    /// <summary>
    /// Max length for the prompt, beyond which examples are cut.
    /// </summary>
    public int MaxLength { get; init; } = 2048;

    private readonly List<int> _exampleTextLengths;

    /// <inheritdoc />
    public void AddExample(IDictionary<string, string> example)
    {
        Examples.Add(example);
        var stringExample = ExamplePrompt.Format(example);
        _exampleTextLengths.Add(GetTextLength(stringExample));
    }

    /// <inheritdoc />
    public IList<IDictionary<string, string>> SelectExamples(IDictionary<string, string> inputVariables)
    {
        // Select how many examples to use based on the input lengths.
        var inputs = string.Join(" ", inputVariables.Values.ToArray());
        var remainingLength = MaxLength - GetTextLength(inputs);
        var i = 0;
        var examples = new List<IDictionary<string, string>>();
        while (remainingLength > 0 && i < Examples.Count)
        {
            var newLength = remainingLength - _exampleTextLengths[i];
            if (newLength < 0)
            {
                break;
            }

            examples.Add(Examples[i]);
            remainingLength = newLength;
            i++;
        }
        return examples;
    }
}