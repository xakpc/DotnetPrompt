using System.Text.Json.Serialization;

namespace DotnetPrompt.LLM.OpenAI.Model;

public record Choice
{
    /// <summary> Index. </summary>
    [JsonPropertyName("index")]
    public int? Index { get; init; }
    /// <summary> Log Prob Model. </summary>
    [JsonPropertyName("logprobs")]
    public CompletionsLogProbability Logprobs { get; init; }
    /// <summary> Reason for finishing. </summary>
    [JsonPropertyName("finish_reason")]
    public string FinishReason { get; init; }


    /// <summary> Generated text for given completion prompt. </summary>
    [JsonPropertyName("text")]
    public string Text { get; init; }

    /// <summary>
    /// The generated completions in the chat format.
    /// </summary>
    [JsonPropertyName("message")]
    public ChatMessage Message { get; init; }
}