using System.Collections.Generic;

namespace DotnetPrompt.Prompts.ExampleSelectors;

/// <summary>
/// Interface for selecting examples to include in prompts.
/// </summary>
public interface IExampleSelector
{
    /// <summary>
    /// Add new example to store for a key.
    /// </summary>
    /// <param name="example">List of examples to select.</param>
    /// <remarks>
    /// Example should be a dictionary with the keys being the input variables and the values being the values for those input variables.
    /// </remarks>
    void AddExample(IDictionary<string, string> example);

    /// <summary>
    /// Select which examples to use based on the inputs.
    /// </summary>
    /// <param name="inputVariables">List of input variables that should be user for calculation of possible examples</param>
    /// <returns></returns>
    IList<IDictionary<string, string>> SelectExamples(IDictionary<string, string> inputVariables);
}