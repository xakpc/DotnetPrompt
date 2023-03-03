using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DotnetPrompt.LLM.OpenAI.Model;

/// <summary> Expected response schema to completion request. </summary>
public record Completions
{
    /// <summary> Id for completion response. </summary>
    [JsonPropertyName("id")]
    public string Id { get; init; }
    /// <summary> Object for completion response. </summary>
    [JsonPropertyName("object")]
    public string Object { get; init; }
    /// <summary> Created time for completion response. </summary>
    [JsonPropertyName("created")]
    public int? Created { get; init; }
    /// <summary> Model used for completion response. </summary>
    [JsonPropertyName("model")]
    public string Model { get; init; }
    /// <summary> Array of choices returned containing text completions to prompts sent. </summary>
    [JsonPropertyName("choices")]
    public IReadOnlyList<Choice> Choices { get; init; }
    /// <summary> Usage counts for tokens input using the completions API. </summary>
    [JsonPropertyName("usage")]
    public CompletionsUsage Usage { get; init; }
}