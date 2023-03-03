using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DotnetPrompt.LLM.CohereAI
{
    /// <summary>
    /// Cohere Configuration
    /// </summary>
    public record CohereAIModelConfiguration
    {
        public static CohereAIModelConfiguration Default => new()
        {
            MaxTokens = 256,
            Temperature = 0.75f,
            K = 0,
            P = 1,
            FrequencyPenalty = 0,
            PresencePenalty = 0
        };

        [JsonPropertyName("prompt")] 
        public string? Prompt { get; init; }

        [field: JsonPropertyName("prompt_vars")]
        internal Dictionary<string, string> PromptVars { get; init; }

        [JsonPropertyName("model")] public string Model { get; init; }

        [JsonPropertyName("preset")]
        internal string Preset { get; init; }

        [JsonPropertyName("num_generations")]
        internal int? NumGenerations { get; init; }

        [JsonPropertyName("max_tokens")]
        public int? MaxTokens { get; init; }

        [JsonPropertyName("temperature")] public float? Temperature { get; init; }

        [JsonPropertyName("k")] public int? K { get; init; }

        [JsonPropertyName("p")] public float? P { get; init; }

        [JsonPropertyName("frequency_penalty")]
        public float? FrequencyPenalty { get; init; }

        [JsonPropertyName("presence_penalty")]
        public float? PresencePenalty { get; init; }

        /// <summary>
        /// The generated text will be cut at the beginning of the earliest occurence of an end sequence. The sequence will be excluded from the text.
        /// </summary>
        [JsonPropertyName("end_sequences")]
        public IList<string> EndSequences { get; init; }

        [JsonPropertyName("stop_sequences")]
        public IList<string> StopSequences { get; init; }

        [JsonPropertyName("return_likelihoods")]
        internal string? ReturnLikelihoods { get; init; }

        [JsonPropertyName("truncate")]
        internal string? Truncate { get; init; }

        [JsonPropertyName("logit_bias")]
        public IDictionary<int, float> LogitBias { get; init; }
    }
}