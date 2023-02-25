using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DotnetPrompt.Abstractions.LLM.Schema;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace DotnetPrompt.Abstractions.LLM;

/// <summary>
/// LLM class that expect subclasses to implement a simpler call method.
/// </summary>
/// <remarks>The purpose of this class is to expose a simpler interface for working with LLMs,
/// rather than expect the user to implement the full _generate method.</remarks>
public abstract class Model : BaseModel
{
    /// <summary>
    /// Run the LLM on the given prompt and input.
    /// </summary>
    /// <param name="prompt">The prompt to generate text for.</param>
    /// <param name="stop">A list of stop words to stop the generation at.</param>
    /// <returns>The generated text.</returns>
    public abstract string Call(string prompt, IList<string> stop = null);

    /// <summary>
    /// Run the LLM on the given prompt and input.
    /// </summary>
    /// <param name="prompt">The prompt to generate text for.</param>
    /// <param name="stop">A list of stop words to stop the generation at.</param>
    /// <returns>The generated text.</returns>
    public abstract Task<string> CallAsync(string prompt, IList<string> stop = null, CancellationToken ct = default);

    /// <summary>
    /// Run the LLM on the given prompt and input.
    /// </summary>
    /// <param name="prompts">The prompts to generate text for.</param>
    /// <param name="stop">A list of stop words to stop the generation at.</param>
    /// <returns>The LLMResult object containing the generated text.</returns>
    public LLMResult Generate(IList<string> prompts, IList<string> stop = null)
    {
        var generations = new List<IList<Generation>>();
        foreach (string prompt in prompts)
        {
            string text = Call(prompt, stop);
            generations.Add(new List<Generation> { new() { Text = text } });
        }

        return new LLMResult { Generations = generations };
    }

    public async Task<LLMResult> GenerateAsync(IList<string> prompts, IList<string> stop = null, CancellationToken ct = default)
    {
        var generations = new List<IList<Generation>>();
        foreach (var prompt in prompts)
        {
            var text = await CallAsync(prompt, stop, ct);
            generations.Add(new List<Generation> { new() { Text = text } });
        }

        return new LLMResult { Generations = generations };
    }

    protected override string AsUniqueString()
    {
        throw new NotImplementedException();
    }

    protected override Task<LLMResult> GenerateInternalAsync(IList<string> prompts, IList<string> stop)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the identifying parameters.
    /// </summary>
    public abstract IDictionary<string, object> IdentifyingParams { get; }

    /// <summary>
    /// Get a string representation of the object for printing.
    /// </summary>
    public override string ToString()
    {
        string clsName = $"\u001b[1m{GetType().Name}\u001b[0m";
        var identifyingParams = IdentifyingParams;
        string paramsStr = string.Join(", ", identifyingParams.Select(kv => $"{kv.Key}={kv.Value}"));
        return $"{clsName}\nParams: {paramsStr}";
    }

    protected Model(ILogger logger, IDistributedCache cache = null) : base(logger, cache)
    {
    }
}