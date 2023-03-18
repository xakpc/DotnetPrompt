using System;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DotnetPrompt.LLM.OpenAI;

/// <summary>
/// OpenAI implementation to communicate with Azure version of OpenAI
/// </summary>
public class AzureOpenAIModel : OpenAIModel
{
    public string DeploymentId { get; }
    public string Account { get; }

    protected override async Task<Model.Completions> CompletionsResponseValue(OpenAIModelConfiguration options)
    {
        var endpoint = $"https://{Account}.openai.azure.com/";
        var client = new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(OpenAiApiKey));

        var azureOptions = new CompletionsOptions()
        {
            Temperature = options.Temperature,
            CacheLevel = options.CacheLevel,
            CompletionConfig = options.CompletionConfig,
            Echo = options.Echo,
            FrequencyPenalty = options.FrequencyPenalty,
            GenerationSampleCount = options.GenerationSampleCount,
            LogProbability = options.LogProbability,
            MaxTokens = options.MaxTokens,
            Model = options.Model,
            NucleusSamplingFactor = options.NucleusSamplingFactor,
            PresencePenalty = options.PresencePenalty,
            SnippetCount = options.SnippetCount,
            User = options.User
        };
        azureOptions.Stop.AddRange(options.Stop);
        azureOptions.Prompt.AddRange(options.Prompt);
        azureOptions.LogitBias.AddRange(options.LogitBias);

        var completionsResponse = await client.GetCompletionsAsync(DeploymentId, azureOptions);

        var azureCompletions = completionsResponse.Value;
        return new Model.Completions()
        {
            Choices = azureCompletions.Choices.Select(c => new Model.Choice()
                {
                    FinishReason = c.FinishReason,
                    Index = c.Index,
                    Logprobs = c.Logprobs != null ? new Model.CompletionsLogProbability()
                    {
                        TextOffset = c.Logprobs.TextOffset,
                        TokenLogProbability = c.Logprobs.TokenLogProbability,
                        Tokens = c.Logprobs.Tokens,
                        TopLogProbability = c.Logprobs.TopLogProbability
                    } : null,
                    Text = c.Text
                }).ToList(),
            Created = azureCompletions.Created,
            Id = azureCompletions.Id,
            Model = azureCompletions.Model,
            Usage = azureCompletions.Usage != null ? new Model.CompletionsUsage()
                {
                    CompletionTokens = azureCompletions.Usage.CompletionTokens,
                    PromptTokens = azureCompletions.Usage.PromptTokens,
                    TotalTokens = azureCompletions.Usage.TotalTokens
                }
                : null
        };
    }

    public AzureOpenAIModel(string openAIApiKey, string deploymentId, string account, OpenAIModelConfiguration defaultModelConfiguration, ILogger logger = null) : base(openAIApiKey, defaultModelConfiguration, logger)
    {
        DeploymentId = deploymentId;
        Account = account;
    }

    public AzureOpenAIModel(IConfiguration configuration, ILogger<AzureOpenAIModel> logger, IDistributedCache cache) : base(configuration, logger, cache)
    {
        OpenAiApiKey = configuration.GetRequiredSection("AzureOpenAI:Key").Value;
        Account = configuration.GetRequiredSection("AzureOpenAI:Account").Value;
        DeploymentId = configuration.GetRequiredSection("AzureOpenAI:DeploymentId").Value;
        UseCache = configuration.GetSection("AzureOpenAI:UseCache").Get<bool?>() ?? false;

        var partialConfig = configuration.GetRequiredSection("AzureOpenAI:ModelConfiguration").Get<OpenAIModelConfiguration>();
        var defaultConfiguration = OpenAIModelConfiguration.Default;
        DefaultModelConfiguration = new OpenAIModelConfiguration()
        {
            CacheLevel = partialConfig.CacheLevel ?? defaultConfiguration.CacheLevel,
            CompletionConfig = partialConfig.CompletionConfig ?? defaultConfiguration.CompletionConfig,
            Echo = partialConfig.Echo ?? defaultConfiguration.Echo,
            FrequencyPenalty = partialConfig.FrequencyPenalty ?? defaultConfiguration.FrequencyPenalty,
            GenerationSampleCount = partialConfig.GenerationSampleCount ?? defaultConfiguration.GenerationSampleCount,
            LogProbability = partialConfig.LogProbability ?? defaultConfiguration.LogProbability,
            LogitBias = partialConfig.LogitBias ?? defaultConfiguration.LogitBias,
            MaxTokens = partialConfig.MaxTokens ?? defaultConfiguration.MaxTokens,
            Model = partialConfig.Model ?? defaultConfiguration.Model,
            NucleusSamplingFactor = partialConfig.NucleusSamplingFactor ?? defaultConfiguration.NucleusSamplingFactor,
            PresencePenalty = partialConfig.PresencePenalty ?? defaultConfiguration.PresencePenalty,
            SnippetCount = partialConfig.SnippetCount ?? defaultConfiguration.SnippetCount,
            Stop = partialConfig.Stop ?? defaultConfiguration.Stop,
            User = partialConfig.User ?? defaultConfiguration.User,
        };
    }

    protected override string AsUniqueString()
    {
        var serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        return JsonSerializer.Serialize(this, serializerOptions);
    }
}