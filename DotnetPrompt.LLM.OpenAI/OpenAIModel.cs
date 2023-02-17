using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DotnetPrompt.Abstractions.LLM;
using DotnetPrompt.Abstractions.Schema;
using DotnetPrompt.LLM.OpenAI.Encoder;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;

namespace DotnetPrompt.LLM.OpenAI;

public class OpenAIModel : BaseModel
{
    public OpenAIModelConfiguration DefaultModelConfiguration { get; init; }

    public string OpenAiApiKey = null;

    public int BatchSize = 20;

    public float? RequestTimeout = null;

    public int MaxRetries = 6;

    public bool Streaming { get; init; } = false;

    /// <summary>
    /// Holds any model parameters valid for `create` call not explicitly specified.
    /// </summary>
    public Dictionary<string, object> ModelExtraArguments { get; set; } = new();

    #region Constructors

    public OpenAIModel(string openAIApiKey, OpenAIModelConfiguration defaultModelConfiguration, ILogger logger, IDistributedCache cache = null) : base(logger, cache)
    {
        OpenAiApiKey = openAIApiKey;
        DefaultModelConfiguration = defaultModelConfiguration;

        ValidateModelExtraArguments();
    }

    public OpenAIModel(string openAIApiKey, ILogger logger) : this(openAIApiKey, OpenAIModelConfiguration.Default, logger)
    {
        ValidateModelExtraArguments();
    }

    /// <summary>
    /// Default constructor without logging
    /// </summary>
    /// <param name="openAIApiKey"></param>
    /// <param name="defaultModelConfiguration"></param>
    public OpenAIModel(string openAIApiKey, OpenAIModelConfiguration defaultModelConfiguration)
    {
        OpenAiApiKey = openAIApiKey;
        DefaultModelConfiguration = defaultModelConfiguration;

        ValidateModelExtraArguments();
    }

    /// <summary>
    /// Constructor to use in DI
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="logger"></param>
    /// <param name="cache"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public OpenAIModel(IConfiguration configuration, ILogger logger, IDistributedCache cache = null) : base(logger, cache)
    {
        this.OpenAiApiKey = configuration["OpenAI:Key"];

        if (string.IsNullOrWhiteSpace(this.OpenAiApiKey))
        {
            throw new InvalidOperationException("No OpenAI:Key configuration found");
        }

        // todo load model defaults from configuration
        DefaultModelConfiguration = OpenAIModelConfiguration.Default;

        ValidateModelExtraArguments();
    }

    #endregion

    private void ValidateModelExtraArguments()
    {
        // todo prevent duplication of model arguments
    }

    /// <summary>
    /// Use tenacity to retry the completion call.
    /// </summary>
    /// <returns></returns>
    public Task<Completions> CompletionWithRetry(OpenAIModelConfiguration modelConfiguration)
    {
        // Wait 2^x * 1 second between each retry starting with
        // 4 seconds, then up to 10 seconds, then 10 seconds afterwards

        // todo apply polly

        return CompletionsResponseValue(modelConfiguration);
    }

    protected virtual async Task<Completions> CompletionsResponseValue(OpenAIModelConfiguration options)
    {
        const string openAiUrl = "https://api.openai.com/v1/completions";

        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", OpenAiApiKey);
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        var stringContent = JsonSerializer.Serialize(options, serializerOptions);

        if (ModelExtraArguments.Any()) // expand original model with extra custom data
        {
            var originalDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(stringContent);
            ModelExtraArguments.ToList().ForEach(x => originalDictionary.Add(x.Key, x.Value));
            stringContent = JsonSerializer.Serialize(originalDictionary, serializerOptions);
        }

        var content = new StringContent(stringContent, Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(openAiUrl, content);
        var result = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<Completions>(result);
    }

    /// <summary>
    /// Calculate the maximum number of tokens possible to generate for a prompt.
    /// </summary>
    /// <param name="prompt">The prompt to pass into the model.</param>
    /// <returns>The maximum number of tokens to generate for a prompt.</returns>
    /// <example>var maxTokens = openai.MaxTokensForPrompt("Tell me a joke.")</example>
    public int MaxTokensForPrompt(string prompt)
    {
        // Calculate the maximum number of tokens possible to generate for a prompt.
        int numTokens = GetNumTokens(prompt);

        // Get max context size for model by name
        int maxSize = ModelNameToContextSize(DefaultModelConfiguration.Model);
        return maxSize - numTokens;
    }

    public override int GetNumTokens(string text)
    {
        List<int> tokens = Gpt3Tokenizer.Encode(text);
        return tokens.Count;
    }

    public override string LLMType => "openai";

    /// <summary>
    /// Calculate the maximum number of tokens possible to generate for a model.
    ///  text-davinci-003: 4,097 tokens
    ///  text-curie-001: 2,048 tokens
    ///  text-babbage-001: 2,048 tokens
    ///  text-ada-001: 2,048 tokens
    ///  code-davinci-002: 8,000 tokens
    ///  code-cushman-001: 2,048 tokens
    /// </summary>
    /// <param name="modelName">The modelname we want to know the context size for.</param>
    /// <returns>The maximum context size</returns>
    public int ModelNameToContextSize(string modelName)
    {
        switch (modelName)
        {
            case "text-davinci-003":
                return 4097;
            case "text-curie-001":
            case "text-babbage-001":
            case "text-ada-001":
                return 2048;
            case "code-davinci-002":
                return 8000;
            case "code-cushman-001":
                return 2048;
            default:
                return 4097;
        }
    }

    protected override string AsUniqueString()
    {
        var serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        return JsonSerializer.Serialize(this, serializerOptions);
    }

    protected override async Task<LLMResult> GenerateInternalAsync(IList<string> prompts, IList<string> stop = null)
    {
        var completionsOptions = DefaultModelConfiguration with { };

        // `base.DefaultStop`, then `DefaultModelConfiguration.Stop`, then `stop`
        if (stop != null && stop.Any())
        {
            if (completionsOptions.Stop != null && completionsOptions.Stop.Any())
            {
                throw new InvalidOperationException("`stop` found in both the input and default params.");
            }

            completionsOptions.Stop = stop;
        }

        var subPrompts = GetSubPrompts(completionsOptions, prompts);
        var choices = new List<Choice>();
        var tokenUsage = new CompletionsUsage();

        foreach (var subPrompt in subPrompts)
        {
            if (Streaming)
            {
                // todo
            }
            else
            {
                var subPromptOptions = completionsOptions with { Prompt = subPrompt };
                _logger.LogTrace("Performing OpenAI request for {CompletionsOptions}", subPromptOptions);
                var response = await CompletionWithRetry(subPromptOptions);
                _logger.LogTrace("OpenAI request Result: {response}", response);
                
                choices.AddRange(response.Choices);

                //Can't update token usage if streaming
                // todo
                tokenUsage = new CompletionsUsage
                {
                    CompletionTokens = tokenUsage.CompletionTokens + response.Usage.CompletionTokens, 
                    PromptTokens = tokenUsage.PromptTokens + response.Usage.PromptTokens, 
                    TotalTokens = tokenUsage.TotalTokens + response.Usage.TotalTokens
                };
            }
        }
        return CreateLLMResult(completionsOptions, choices, prompts, tokenUsage);
    }

    public List<List<string>> GetSubPrompts(OpenAIModelConfiguration completionsOptions, IList<string> prompts)
    {
        if (completionsOptions.MaxTokens == -1)
        {
            if (prompts.Count != 1)
            {
                throw new ArgumentException("max_tokens set to -1 not supported for multiple inputs.");
            }
            completionsOptions.MaxTokens = MaxTokensForPrompt(prompts[0]);
        }

        var subPrompts = new List<List<string>>();
        for (var i = 0; i < prompts.Count; i += BatchSize)
        {
            subPrompts.Add(prompts.ToList().GetRange(i, Math.Min(BatchSize, prompts.Count - i)));
        }
        return subPrompts;
    }

    private static LLMResult CreateLLMResult(OpenAIModelConfiguration configuration, 
        IList<Choice> choices, ICollection<string> prompts,
        CompletionsUsage tokenUsage)
    {
        var generations = new List<IList<Generation>>();
        for (int i = 0; i < prompts.Count; i++)
        {
            var subChoices = choices.Skip(i * configuration.SnippetCount).Take(configuration.SnippetCount).ToArray();
            generations.Add(subChoices.Select(choice => new Generation
            {
                Text = choice.Text,
                Info = new Dictionary<string, object>
                {
                    { "finish_reason", choice.FinishReason },
                    { "logprobs", choice.Logprobs }
                }
            }).ToList());
        }
        return new LLMResult
        {
            Generations = generations,
            Output = new Dictionary<string, object> { { "token_usage", tokenUsage } }
        };
    }
}