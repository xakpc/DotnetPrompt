using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DotnetPrompt.Abstractions.Schema;

/// <summary>
/// Output of a single generation.
/// </summary>
[DataContract]
public class Generation
{
    /// <summary>
    /// Generated text output.
    /// </summary>
    [DataMember]
    public string Text { get; set; }

    /// <summary>
    /// Raw generation info response from the provider
    /// May include things like reason for finishing (e.g. in OpenAI)
    /// </summary>
    [DataMember]
    public Dictionary<string, object> Info { get; set; }
}