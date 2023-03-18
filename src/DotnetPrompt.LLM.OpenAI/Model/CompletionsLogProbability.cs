using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DotnetPrompt.LLM.OpenAI.Model;

public record CompletionsLogProbability
{
    /// <summary> Log Probability of Tokens. </summary>
    [JsonPropertyName("token_logprobs")]
    public IReadOnlyList<float?> TokenLogProbability { get; init; }

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