using System;
using System.Collections.Generic;

namespace DotnetPrompt.Abstractions.Prompts;

/// <summary>
/// Schema to represent a prompt for an LLM.
/// </summary>
public interface IPromptTemplate
{
    /// <summary>
    /// A list of the names of the variables the prompt template expects.
    /// </summary>
    IList<string> InputVariables { get; set; }

    /// <summary>
    /// Build a prompt from current template and a list of values.
    /// </summary>
    /// <param name="values">Key-Value list of values to use to build prompt.</param>
    /// <returns>String prompt</returns>
    /// <exception cref="ArgumentException">Throws when provided list of keys does not match <see cref="InputVariables"/>.</exception>
    string Format(IDictionary<string, string> values);
}