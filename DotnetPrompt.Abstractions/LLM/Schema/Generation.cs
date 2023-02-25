using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DotnetPrompt.Abstractions.LLM.Schema;

/// <summary>
/// Output of a single generation.
/// </summary>
public record Generation
{
    /// <summary>
    /// Generated text output.
    /// </summary>
    public string Text { get; init; }

    /// <summary>
    /// Raw generation info response from the provider
    /// May include things like reason for finishing (e.g. in OpenAI)
    /// </summary>
    public Dictionary<string, object> Info { get; init; }
}