using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DotnetPrompt.Abstractions.Schema;

/// <summary>
/// Class that contains all relevant information for an LLM Result.
/// </summary>
[DataContract]
public class LLMResult
{
    /// <summary>
    /// List of the things generated.
    /// This is List[List[]] because each input could have multiple generations/generated completions.
    /// </summary>
    [DataMember]
    public IList<IList<Generation>> Generations { get; set; }

    /// <summary>
    /// For arbitrary LLM provider specific output.
    /// </summary>
    [DataMember]
    public IDictionary<string, object> Output { get; set; }
}