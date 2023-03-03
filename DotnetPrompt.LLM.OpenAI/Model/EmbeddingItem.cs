using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DotnetPrompt.LLM.OpenAI.Model;

/// <summary> Expected response schema to embeddings object list item request. </summary>
public record EmbeddingItem
{
    /// <summary> List of embeddings value for the input prompt. These represents a measurement of releated of text strings. </summary>
    [JsonPropertyName("embedding")]
    public IReadOnlyList<float> Embedding { get; init; }
    /// <summary> Index of the prompt to which the EmbeddingItem corresponds. </summary>
    [JsonPropertyName("index")]
    public int Index { get; init; }

    /// <summary>
    /// Text of embeddings
    /// </summary>
    [JsonIgnore]
    public string Text { get; set; }
}