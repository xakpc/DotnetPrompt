using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DotnetPrompt.LLM.OpenAI.Model
{
    internal record BaseEmbeddingOptions
    {
        /// <summary> The ID of the end-user, for use in tracking and rate-limiting. </summary>
        [JsonPropertyName("user")]
        public string User { get; init; }
        /// <summary> ID of the model to use. </summary>
        [JsonPropertyName("model")]
        public string Model { get; init; }
    }

    /// <summary> Schema to create a prompt completion from a deployment. </summary>
    internal record EmbeddingsOptions : BaseEmbeddingOptions
    {
        /// <summary>
        /// Input text to get embeddings for, encoded as a string.
        /// To get embeddings for multiple inputs in a single request, pass an array of strings.
        /// Each input must not exceed 2048 tokens in length for V1 models (obsolete)
        /// Each input must not exceed 8192 tokens in length for V2 models (default)
        /// 
        /// Unless you are embedding code, we suggest replacing newlines (\n) in your input with a single space,
        /// as we have observed inferior results when newlines are present.
        /// </summary>
        [JsonPropertyName("input")]
        public IList<string> Input { get; init; }
    }
}
