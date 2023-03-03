using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using DotnetPrompt.LLM.OpenAI.Model;
using DotnetPrompt.Abstractions.Tools;
using Microsoft.Extensions.Configuration;

namespace DotnetPrompt.LLM.OpenAI;

/// <summary>
/// Embeddings client for OpenAI
/// </summary>
public class OpenAIEmbeddings : IEmbeddings
{
    private string DocumentModelName { get; } = "text-embedding-ada-002";
    private string QueryModelName { get; } = "text-embedding-ada-002";
    public int MaxTokens { get; init; } = 8000;

    /// <summary>
    /// Size of the batch
    /// </summary>
    /// <remarks>Default is 20</remarks>
    public int BatchSize { get; init; } = 20;

    private string OpenAiKey { get; }

    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="openAiKey"></param>
    public OpenAIEmbeddings(string openAiKey)
    {
        OpenAiKey = openAiKey;
    }

    public OpenAIEmbeddings(IConfiguration configuration)
    {
        OpenAiKey = configuration.GetRequiredSection("OpenAI:Key").Value;
    }
    #endregion

    /// <summary>
    /// Call out to OpenAI's embedding endpoint for embedding search docs.
    /// </summary>
    /// <param name="texts"></param>
    /// <returns></returns>
    public async Task<IList<TextEmbedding>> EmbedAsync(IList<string> texts)
    {
        if (texts.Any(t => t.Length / 4 > MaxTokens)) // on average 4 character per token
        {
            throw new ArgumentException("Text length too big");
        }

        var batches = new List<Task<Embeddings>>();
        for (var i = 0; i < texts.Count; i += BatchSize)
        {
            batches.Add(GetEmbeddingItem(texts, i));
        }

        var result = await Task.WhenAll(batches);
        var allData = result.SelectMany(i => i.Data).ToList();

        if (allData.Count > texts.Count)
        {
            throw new InvalidOperationException("Single text should give single embeddings vector");
        }

        return allData.Select(e => new TextEmbedding(e.Embedding, e.Text)).ToList();

        async Task<Embeddings> GetEmbeddingItem(IList<string> texts, int i)
        {
            var result = await CreateEmbedding(new EmbeddingsOptions() { Input = texts.Skip(i).Take(BatchSize).ToList(), Model = DocumentModelName })
                .ConfigureAwait(false);

            for (var index = 0; index < result.Data.Count; index++)
            {
                var item = result.Data[index];
                item.Text = texts[i];
            }

            return result;
        }
    }

    public async Task<TextEmbedding> EmbedAsync(string text)
    {
        if (text.Length / 4 > MaxTokens) // on average 4 character per token
        {
            throw new ArgumentException("Text length too big");
        }

        var result = await CreateEmbedding(new EmbeddingsOptions() { Input = new[] {text}, Model = QueryModelName });

        for (var index = 0; index < result.Data.Count; index++)
        {
            var item = result.Data[index];
            item.Text = text;
        }

        if (result.Data.Count > 1)
        {
            throw new InvalidOperationException("Single text should give single embeddings vector");
        }

        var embeddings = result.Data.First();
        return new TextEmbedding(embeddings.Embedding, embeddings.Text);
    }

    private Task<Embeddings> CreateEmbedding(EmbeddingsOptions options)
    {
        var serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        var stringContent = JsonSerializer.Serialize(options, serializerOptions);

        return PostEmbeddingsAsync(stringContent);
    }

    private async Task<Embeddings> PostEmbeddingsAsync(string stringContent)
    {
        const string openAiUrl = "https://api.openai.com/v1/embeddings";

        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", OpenAiKey);
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var content = new StringContent(stringContent, Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(openAiUrl, content);
        var result = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<Embeddings>(result);
    }
}
