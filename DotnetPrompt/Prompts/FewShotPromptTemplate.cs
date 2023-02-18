using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotnetPrompt.Abstractions.Prompts;
using DotnetPrompt.Prompts.ExampleSelectors;

namespace DotnetPrompt.Prompts;

public class FewShotPromptTemplate : IPromptTemplate
{
    
    private readonly IPromptTemplate? _prefix;
    private readonly IPromptTemplate _suffixPromptTemplate;
    private readonly IPromptTemplate _examplePromptTemplate;

    private readonly IExampleSelector? _exampleSelector;
    public IList<IDictionary<string, string>>? Examples { get; init; } = null;

    /// <inheritdoc />
    public IList<string> InputVariables { get; init; }

    public string ExampleSeparator { get; set; } = "\n\n";

    #region Constructors

    public FewShotPromptTemplate(IPromptTemplate examplePromptTemplate,
        IPromptTemplate suffixPromptTemplate,
        IList<IDictionary<string, string>> examples)
    {
        Examples = examples;
        InputVariables = suffixPromptTemplate.InputVariables;

        _suffixPromptTemplate = suffixPromptTemplate;
        _examplePromptTemplate = examplePromptTemplate;
    }

    public FewShotPromptTemplate(IPromptTemplate examplePromptTemplate,
        IPromptTemplate suffixPromptTemplate,
        IExampleSelector exampleSelector)
    {
        InputVariables = suffixPromptTemplate.InputVariables;
        _suffixPromptTemplate = suffixPromptTemplate;
        _examplePromptTemplate = examplePromptTemplate;
        _exampleSelector = exampleSelector;
    }

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

        throw new ArgumentException("One of 'examples' and 'example_selector' should be provided");
    }

    public string Format(IDictionary<string, string> values)
    {
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

        // Format the template with the input variables.
        //return TemplateFormatterMapping[TemplateFormat](template, values);
        return template.ToString();
    }

    //public static ValidationException CheckExamplesAndSelector(Dictionary<string, object> values)
    //{
    //    // Check that one and only one of examples/example_selector are provided.
    //    List<Dictionary<string, object>> examples = values.GetValueOrDefault("examples") as List<Dictionary<string, object>>;
    //    BaseExampleSelector exampleSelector = values.GetValueOrDefault("example_selector") as BaseExampleSelector;
    //    if (examples != null && exampleSelector != null)
    //    {
    //        return new ValidationException("Only one of 'examples' and 'example_selector' should be provided");
    //    }

    //    if (examples == null && exampleSelector == null)
    //    {
    //        return new ValidationException("One of 'examples' and 'example_selector' should be provided");
    //    }

    //    return null;
    //}

    //public static ValidationException TemplateIsValid(Dictionary<string, object> values)
    //{
    //    // Check that prefix, suffix and input variables are consistent.
    //    if ((bool)values["validate_template"])
    //    {
    //        CheckValidTemplate(
    //            (string)values["prefix"] + (string)values["suffix"],
    //            (string)values["template_format"],
    //            (List<string>)values["input_variables"]
    //        );
    //    }
    //    return null;
    //}
}