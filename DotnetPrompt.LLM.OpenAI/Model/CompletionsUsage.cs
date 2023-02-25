using System.Text.Json.Serialization;

namespace DotnetPrompt.LLM.OpenAI.Model;

/// <summary>
/// Representation of the token counts processed for a completions request.
/// Counts consider all tokens across prompts, choices, choice alternates, best_of generations, and other consumers.
/// </summary>
public record CompletionsUsage
{
    /// <summary> Number of tokens received in the completion. </summary>
    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; init; }
    /// <summary> Number of tokens sent in the original request. </summary>
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; init; }
    /// <summary> Total number of tokens transacted in this request/response. </summary>
    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; init; }
}