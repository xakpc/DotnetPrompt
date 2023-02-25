using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotnetPrompt.Abstractions.LLM.Schema;

namespace DotnetPrompt.Abstractions.LLM;

public interface ILargeLanguageModel
{
    /// <summary>
    /// Run the LLM on the given prompts and stops.
    /// </summary>
    /// <param name="prompts"></param>
    /// <param name="stop"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">When cache asked without </exception>
    Task<LLMResult> GenerateAsync(IList<string> prompts, IList<string> stop = null);

    /// <summary>
    /// Keyword for model type, used for serialization
    /// </summary>
    string LLMType { get; }
}