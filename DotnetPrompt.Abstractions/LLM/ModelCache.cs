using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DotnetPrompt.Abstractions.LLM.Schema;
using Microsoft.Extensions.Caching.Distributed;

namespace DotnetPrompt.Abstractions.LLM;

/// <summary>
/// Cache of requests to model based on prompt and model configuration
/// </summary>
public class ModelCache
{
    private readonly IDistributedCache _cache;

    public ModelCache(IDistributedCache cache)
    {
        _cache = cache;
    }

    private async Task<List<Generation>> LookupCacheAsync(string prompt, string llmString, CancellationToken token = default)
    {
        var cachedString = await _cache.GetStringAsync((prompt, llmString).ToString(), token);
        return string.IsNullOrEmpty(cachedString) ? default : JsonSerializer.Deserialize<List<Generation>>(cachedString);
    }

    private Task SetCacheAsync(string prompt, string llmString, IEnumerable<Generation> newResultsGeneration, CancellationToken token = default)
    {
        var cacheString = JsonSerializer.Serialize(newResultsGeneration);
        return _cache?.SetStringAsync((prompt, llmString).ToString(), cacheString, token);
    }

    /// <summary>
    /// Get prompts that are already cached.
    /// </summary>
    /// <param name="llmString"></param>
    /// <param name="prompts"></param>
    /// <returns></returns>
    public async Task<(Dictionary<int, IList<Generation>> ExistingPrompts, string LLMString, IList<int> MissingPromptIdxs, IList<string> MissingPrompts)> 
        GetPromptsAsync(string llmString, IList<string> prompts)
    {
        var existingPrompts = new Dictionary<int, IList<Generation>>();
        var missingPromptIdxs = new List<int>();
        var missingPrompts = new List<string>();

        for (var i = 0; i < prompts.Count; i++)
        {
            var prompt = prompts[i];
            if (_cache == null)
            {
                continue;
            }

            var cacheVal = await LookupCacheAsync(prompt, llmString);
            if (cacheVal != default)
            {
                existingPrompts[i] = cacheVal;
            }
            else
            {
                missingPrompts.Add(prompt);
                missingPromptIdxs.Add(i);
            }
        }

        return (existingPrompts, 
            llmString, 
            missingPromptIdxs, 
            missingPrompts);
    }

    /// <summary>
    /// Update the cache and get the LLM output.
    /// </summary>
    /// <param name="existingPrompts"></param>
    /// <param name="llmString"></param>
    /// <param name="missingPromptIndexes"></param>
    /// <param name="newResults"></param>
    /// <param name="prompts"></param>
    /// <returns></returns>
    public async Task<IDictionary<string, object>> UpdateCache(
        Dictionary<int, IList<Generation>> existingPrompts,
        string llmString,
        IList<int> missingPromptIndexes,
        ModelResult newResults,
        IList<string> prompts)
    {
        for (var i = 0; i < newResults.Generations.Count; i++)
        {
            existingPrompts[missingPromptIndexes[i]] = newResults.Generations[i];
            var prompt = prompts[missingPromptIndexes[i]];
            await SetCacheAsync(prompt, llmString, newResults.Generations[i]);
        }

        return newResults.Output;
    }
}