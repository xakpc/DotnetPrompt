using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using DotnetPrompt.Abstractions.LLM;
using DotnetPrompt.Abstractions.LLM.Schema;
using DotnetPrompt.LLM.OpenAI.Model;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using Choice = DotnetPrompt.LLM.OpenAI.Model.Choice;
using CompletionsUsage = DotnetPrompt.LLM.OpenAI.Model.CompletionsUsage;
using System.Text.Json.Nodes;

namespace DotnetPrompt.LLM.OpenAI;

public static class Tokens
{
    public const string ImStart = "<|im_start|>";
    public const string ImEnd = "<|im_end|>";
}

public record ChatGptModelConfiguration : OpenAIModelConfiguration
{
    public new static ChatGptModelConfiguration Default = new()
    {
        Model = "gpt-3.5-turbo",
        Temperature = 0.7f,
        MaxTokens = 256,
        NucleusSamplingFactor = 1,
        FrequencyPenalty = 0,
        PresencePenalty = 0,
        SnippetCount = 1,
        GenerationSampleCount = null,
        Prompt = null
    };

    [JsonPropertyName("messages")] public IList<ChatMessage> Messages { get; set; }
}

/// <summary>
/// OpenAI model to use with gpt-3.5-turbo model
/// </summary>
public class ChatGptModel : BaseModel
{
    public const string Model = "gpt-3.5-turbo";

    public ChatGptModelConfiguration DefaultModelConfiguration { get; set; }
    public string OpenAiApiKey { get; set; }

    #region Constructors

    /// <summary>
    /// Default constructor without logging
    /// </summary>
    public ChatGptModel(string openAIApiKey, ChatGptModelConfiguration defaultModelConfiguration, ILogger logger = null, IDistributedCache cache = null)
        : base(logger, cache)
    {
        OpenAiApiKey = openAIApiKey;
        DefaultModelConfiguration = defaultModelConfiguration with { Model = Model };
    }

    #endregion

    protected override string AsUniqueString()
    {
        var serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        return JsonSerializer.Serialize(this, serializerOptions);
    }

    public override string LLMType => "chatgpt";
    public override int MaxRequestTokens => 4000;

    protected override async Task<ModelResult> GenerateInternalAsync(IList<string> prompts, IList<string> stop = null)
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

        var choices = new List<Choice>();
        var tokenUsage = new CompletionsUsage();

        foreach (var subPrompt in prompts)
        {
            // if we pass string instead of valid json wrap it
            var messages = ValidateJSON(subPrompt)
                ? JsonSerializer.Deserialize<IList<ChatMessage>>(subPrompt, new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                })
                : new[] { new ChatMessage("user", subPrompt) };

            var subPromptOptions = completionsOptions with
            {
                Messages = messages,
                MaxTokens = completionsOptions.MaxTokens == -1 ? MaxRequestTokens - GetNumTokens(subPrompt) : completionsOptions.MaxTokens
            };

            if (subPromptOptions.MaxTokens < 0)
            {
                throw new InvalidOperationException($"Prompt {subPrompt} has too much tokens");
            }
            
            Logger.LogTrace("Performing ChatGPT request for {CompletionsOptions}", subPromptOptions);
            var response = await CompletionsResponseValue(subPromptOptions);
            Logger.LogTrace("ChatGPT request Result: {response}", response);

            choices.AddRange(response.Choices);

            tokenUsage = new CompletionsUsage
            {
                CompletionTokens = tokenUsage.CompletionTokens + response.Usage.CompletionTokens,
                PromptTokens = tokenUsage.PromptTokens + response.Usage.PromptTokens,
                TotalTokens = tokenUsage.TotalTokens + response.Usage.TotalTokens
            };
        }

        return CreateResult(completionsOptions, choices, prompts, tokenUsage);
    }


    public static bool ValidateJSON(string jsonString)
    {
        try
        {
            JsonNode.Parse(jsonString);
            return true;
        }
        catch (Exception)
        {
            //Invalid json format
            return false;
        }
    }

    private static ModelResult CreateResult(ChatGptModelConfiguration completionsOptions, IReadOnlyCollection<Choice> choices, ICollection<string> prompts, CompletionsUsage tokenUsage)
    {
        var generations = new List<IList<Generation>>();
        for (var i = 0; i < prompts.Count; i++)
        {
            var count = completionsOptions.SnippetCount.Value;
            var subChoices = choices.Skip(i * count).Take(count).ToArray();
            generations.Add(subChoices.Select(choice => new Generation
            {
                Text = choice.Message.Content,
                Info = new Dictionary<string, object>
                {
                    { "finish_reason", choice.FinishReason },
                    { "logprobs", choice.Logprobs }
                }
            }).ToList());
        }
        return new ModelResult
        {
            Generations = generations,
            Output = new Dictionary<string, object> { { "token_usage", tokenUsage } }
        };
    }

    protected async Task<Completions> CompletionsResponseValue(ChatGptModelConfiguration options)
    {
        const string openAiUrl = "https://api.openai.com/v1/chat/completions";

        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", OpenAiApiKey);
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        var stringContent = JsonSerializer.Serialize(options, serializerOptions);

        var content = new StringContent(stringContent, Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(openAiUrl, content);
        var result = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<Completions>(result);
    }
}