using System.Collections.Generic;

namespace DotnetPrompt.Abstractions.LLM.Schema;

/// <summary>
/// Class that contains all relevant information for an LLM Result.
/// </summary>
public record ModelResult
{
    /// <summary>
    /// List of the things generated.
    /// This is List[List[]] because each input could have multiple generations/generated completions.
    /// </summary>
    public IList<IList<Generation>> Generations { get; set; }

    /// <summary>
    /// For arbitrary LLM provider specific output.
    /// </summary>
    public IDictionary<string, object> Output { get; set; }
}