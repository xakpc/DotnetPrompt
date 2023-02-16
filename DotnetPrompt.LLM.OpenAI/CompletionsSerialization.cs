using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DotnetPrompt.LLM.OpenAI;

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

public record Choice
{
    /// <summary> Generated text for given completion prompt. </summary>
    [JsonPropertyName("text")]
    public string Text { get; init; }
    /// <summary> Index. </summary>
    [JsonPropertyName("index")]
    public int? Index { get; init; }
    /// <summary> Log Prob Model. </summary>
    [JsonPropertyName("logprobs")]
    public CompletionsLogProbability Logprobs { get; init; }
    /// <summary> Reason for finishing. </summary>
    [JsonPropertyName("finish_reason")]
    public string FinishReason { get; init; }
}

public record CompletionsLogProbability
{
    /// <summary> Log Probability of Tokens. </summary>
    [JsonPropertyName("token_logprobs")]
    public IReadOnlyList<float> TokenLogProbability { get; init; }

    /// <summary> Top Log Probabilities. </summary>
    [JsonPropertyName("top_logprobs")]
    public IReadOnlyList<IDictionary<string, float>> TopLogProbability { get; init; }

    /// <summary> Tokens. </summary>
    [JsonPropertyName("tokens")]
    public IReadOnlyList<string> Tokens { get; init; }

    /// <summary> Text offset. </summary>
    [JsonPropertyName("text_offset")]
    public IReadOnlyList<int> TextOffset { get; init; }
}

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