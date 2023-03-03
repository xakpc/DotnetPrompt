using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DotnetPrompt.LLM.OpenAI.Model;

public record Embeddings
{
    /// <summary> Embedding values for the prompts submitted in the request. </summary>
    [JsonPropertyName("data")]
    public IReadOnlyList<EmbeddingItem> Data { get; init; }
    /// <summary> ID of the model to use. </summary>
    [JsonPropertyName("model")]
    public string Model { get; init; }
    /// <summary> Usage counts for tokens input using the embeddings API. </summary>
    [JsonPropertyName("usage")]
    public EmbeddingsUsage Usage { get; init; }
}

/// <summary> Measurment of the amount of tokens used in this request and response. </summary>
public record EmbeddingsUsage
{
    /// <summary> Number of tokens sent in the original request. </summary>
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; init; }
    /// <summary> Total number of tokens transacted in this request/response. </summary>
    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; init; }
}