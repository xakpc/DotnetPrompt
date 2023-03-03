using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotnetPrompt.Abstractions.Prompts;
using DotnetPrompt.Prompts.ExampleSelectors;

namespace DotnetPrompt.Prompts;

/// <summary>
/// The FewShotPromptTemplate class allows developers to create prompts for natural language processing (NLP) models using few-shot learning techniques.
/// This class takes in a prefix, examples, and suffix, and combines them with a separator to generate a prompt for the language model.
/// </summary>
public class FewShotPromptTemplate : IPromptTemplate
{
    private readonly IPromptTemplate? _prefix;
    private readonly IPromptTemplate _suffixPromptTemplate;
    private readonly IPromptTemplate _examplePromptTemplate;

    private readonly IExampleSelector? _exampleSelector;

    /// <inheritdoc />
    public IList<string> InputVariables { get; init; }

    /// <summary>
    /// List of Examples, where each example is a Dictionary where key is Input Variable and value is Input Value
    /// </summary>
    public IList<IDictionary<string, string>>? Examples { get; init; }

    /// <summary>
    /// Separator for examples, default value is "\n\n"
    /// </summary>
    public string ExampleSeparator { get; set; } = "\n\n";

    #region Constructors
    /// <summary>
    /// Constructor with examples 
    /// </summary>
    /// <param name="examplePromptTemplate">PromptTemplate to format Example</param>
    /// <param name="suffixPromptTemplate">PromptTemplate to format suffix</param>
    /// <param name="examples">List of Examples</param>
    public FewShotPromptTemplate(IPromptTemplate examplePromptTemplate,
        IPromptTemplate suffixPromptTemplate,
        IList<IDictionary<string, string>> examples)
    {
        Examples = examples;
        InputVariables = suffixPromptTemplate.InputVariables;

        _suffixPromptTemplate = suffixPromptTemplate;
        _examplePromptTemplate = examplePromptTemplate;
    }

    /// <summary>
    /// Constructor with example selector
    /// </summary>
    /// <param name="examplePromptTemplate">PromptTemplate to format Example</param>
    /// <param name="suffixPromptTemplate">PromptTemplate to format suffix</param>
    /// <param name="exampleSelector">Example selector that will select examples for prompt</param>
    public FewShotPromptTemplate(IPromptTemplate examplePromptTemplate,
        IPromptTemplate suffixPromptTemplate,
        IExampleSelector exampleSelector)
    {
        InputVariables = suffixPromptTemplate.InputVariables;
        _suffixPromptTemplate = suffixPromptTemplate;
        _examplePromptTemplate = examplePromptTemplate;
        _exampleSelector = exampleSelector;
    }

    /// <summary>
    /// Constructor with examples and prefix
    /// </summary>
    /// <param name="prefixPromptTemplate">PromptTemplate to format prefix</param>
    /// <param name="examplePromptTemplate">PromptTemplate to format Example</param>
    /// <param name="suffixPromptTemplate">PromptTemplate to format suffix</param>
    /// <param name="examples">List of Examples</param>
    public FewShotPromptTemplate(IPromptTemplate prefixPromptTemplate,
        IPromptTemplate examplePromptTemplate,
        IPromptTemplate suffixPromptTemplate,
        IList<IDictionary<string, string>> examples)
    {
        Examples = examples;
        InputVariables = prefixPromptTemplate.InputVariables.Union(suffixPromptTemplate.InputVariables).ToList();

        _prefix = prefixPromptTemplate;
        _suffixPromptTemplate = suffixPromptTemplate;
        _examplePromptTemplate = examplePromptTemplate;
    }

    /// <summary>
    /// Constructor with example selector and prefix
    /// </summary>
    /// <param name="prefixPromptTemplate">PromptTemplate to format prefix</param>
    /// <param name="examplePromptTemplate">PromptTemplate to format Example</param>
    /// <param name="suffixPromptTemplate">PromptTemplate to format suffix</param>
    /// <param name="exampleSelector">Example selector that will select examples for prompt</param>
    public FewShotPromptTemplate(IPromptTemplate prefixPromptTemplate,
        IPromptTemplate examplePromptTemplate,
        IPromptTemplate suffixPromptTemplate,
        IExampleSelector exampleSelector)
    {
        InputVariables = prefixPromptTemplate.InputVariables.Union(suffixPromptTemplate.InputVariables).ToList();

        _prefix = prefixPromptTemplate;
        _suffixPromptTemplate = suffixPromptTemplate;
        _examplePromptTemplate = examplePromptTemplate;
        _exampleSelector = exampleSelector;
    }
    #endregion

    /// <inheritdoc />
    public string Format(IDictionary<string, string>? values = null)
    {
        values ??= new Dictionary<string, string>();

        // Get the examples to use.
        var examples = GetExamples(values);

        // Format the examples.
        var exampleStrings = examples.Select(example => _examplePromptTemplate.Format(example)).ToList();

        // Create the overall template.
        var sb = new StringBuilder();
        var pieces = new List<string> { _prefix?.Format(values) ?? string.Empty }; 
        pieces.AddRange(exampleStrings);
        pieces.Add(_suffixPromptTemplate.Format(values));
        var template = sb.AppendJoin(ExampleSeparator, pieces.Where(piece => !string.IsNullOrEmpty(piece)));
        
        return template.ToString();
    }

    private IEnumerable<IDictionary<string, string>> GetExamples(IDictionary<string, string> inputValues)
    {
        if (Examples != null)
        {
            return Examples;
        }

        if (_exampleSelector != null)
        {
            return _exampleSelector.SelectExamples(inputValues);
        }

        throw new ArgumentException("One of 'Examples' or 'ExampleSelector' should be provided");
    }
}