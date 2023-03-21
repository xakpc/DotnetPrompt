using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DotnetPrompt.Abstractions.LLM;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Linq;
using DotnetPrompt.Abstractions.LLM.Schema;

namespace DotnetPrompt.LLM.CohereAI
{
    public record Completions
    {
        [JsonPropertyName("id")]
        public string id { get; init; }
        [JsonPropertyName("generations")]
        public IList<Generation> generations { get; init; }
        [JsonPropertyName("prompt")]
        public string prompt { get; init; }
    }

    public record Generation
    {
        [JsonPropertyName("id")]
        public string id { get; set; }
        [JsonPropertyName("text")]
        public string text { get; set; }
    }


    public class CohereAIModel : BaseModel
    {
        private readonly string _cohereAiKey;
        private readonly CohereAIModelConfiguration _configuration;

        public CohereAIModel(string cohereAIKey, CohereAIModelConfiguration configuration, ILogger logger = null, IDistributedCache cache = null) : base(logger, cache)
        {
            _cohereAiKey = cohereAIKey;
            _configuration = configuration;
        }

        protected override string AsUniqueString()
        {
            var serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            return JsonSerializer.Serialize(this, serializerOptions);
        }

        protected override async Task<ModelResult> GenerateInternalAsync(IList<string> prompts, IList<string> stop = null)
        {

            var configuration = _configuration with {};

            if (stop != null && stop.Any())
            {
                if (_configuration.StopSequences != null && _configuration.StopSequences.Any())
                {
                    throw new InvalidOperationException("`stop` found in both the input and default params.");
                }

                configuration = _configuration with { StopSequences = stop }; // todo do better
            }

            var results = new ModelResult()
            {
                Generations = new List<IList<Abstractions.LLM.Schema.Generation>>()
            };
            foreach (var subPrompt in prompts)
            {
                var subPromptOptions = configuration with { Prompt = subPrompt };
                    Logger.LogTrace("Performing CohereAi request for {CompletionsOptions}", subPromptOptions);
                    var response = await CompletionWithRetry(subPromptOptions);
                    Logger.LogTrace("CohereAi request Result: {response}", response);

                    results.Generations.Add(response.generations.Select(g =>
                        new Abstractions.LLM.Schema.Generation()
                        {
                            Text = g.text,
                            Info = new Dictionary<string, object>()
                            {
                                { "id", g.id },
                                { "completions-id", response.id }
                            }
                        }).ToList());


                    //choices.AddRange(response.Choices);

                    // todo
                    //tokenUsage = new CompletionsUsage
                    //{
                    //    CompletionTokens = tokenUsage.CompletionTokens + response.Usage.CompletionTokens,
                    //    PromptTokens = tokenUsage.PromptTokens + response.Usage.PromptTokens,
                    //    TotalTokens = tokenUsage.TotalTokens + response.Usage.TotalTokens
                    //};
            }

            return results;
        }

        private Task<Completions> CompletionWithRetry(CohereAIModelConfiguration subPromptOptions)
        {
            // todo implement polly
            return CallCohereAi(_cohereAiKey, subPromptOptions);
        }

        public override string LLMType => "cohere";
        
        public override int MaxRequestTokens => 2000;

        async Task<Completions> CallCohereAi(string key, CohereAIModelConfiguration configuration)
        {
            const string cohereAi = "https://api.cohere.ai/generate";

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Add("Cohere-Version", "2022-12-06");

            var serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            var stringContent = JsonSerializer.Serialize(configuration, serializerOptions);

            var content = new StringContent(stringContent, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(cohereAi, content);
            var result = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<Completions>(result);
        }
    }
}
