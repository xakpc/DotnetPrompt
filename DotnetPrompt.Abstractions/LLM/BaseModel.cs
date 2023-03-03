using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotnetPrompt.Abstractions.LLM.Schema;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DotnetPrompt.Abstractions.LLM;

public abstract class BaseModel : ILargeLanguageModel
{
    protected readonly ILogger Logger;
    private readonly ModelCache _modelCache;

    public IList<string> DefaultStop { get; set; } = null;

    /// <summary>
    /// Return current model as unique string for caching purposes
    /// </summary>
    /// <returns></returns>
    protected abstract string AsUniqueString();

    #region Constructors

    protected BaseModel(ILogger logger, IDistributedCache cache)
    {
        Logger = logger ?? NullLogger.Instance;
        _modelCache = cache == null ? null : new ModelCache(cache);
    }

    #endregion

    public bool UseCache { get; set; } = false;

    /// <summary>
    /// Run the LLM on the given prompt and input.
    /// </summary>
    /// <param name="prompts"></param>
    /// <param name="stop"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">When cache asked without </exception>
    public async Task<ModelResult> GenerateAsync(IList<string> prompts, IList<string> stop = null)
    {
        // merge stops
        if (DefaultStop != null && DefaultStop.Any())
        {
            if (stop != null && stop.Any())
            {
                throw new InvalidOperationException("`stop` found in both the input and default params.");
            }

            stop = DefaultStop.ToList();
        }

        if (_modelCache == null || !UseCache)
        {
            // This happens when _cache is Null, but Cache is True
            if (UseCache)
            {
                throw new InvalidOperationException("Asked to cache, but no cache found at `LLMCache`.");
            }

            try
            {
                var output = await GenerateInternalAsync(prompts, stop);
                return output;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "LLM Error '{Message}' in {Model}", e.Message, LLMType);
                throw;
            }
        }

        var (existingPrompts, llmString, missingPromptIdxs, missingPrompts) = await _modelCache.GetPromptsAsync(AsUniqueString(), prompts);

        IDictionary<string, object> llmOutput = default;
        if (missingPrompts.Count > 0)
        {
            try
            {
                var newResults = await GenerateInternalAsync(missingPrompts, stop);
                llmOutput = await _modelCache.UpdateCache(existingPrompts, llmString, missingPromptIdxs, newResults, prompts);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "LLM Error '{Message}' in {Model}", e.Message, LLMType);
                throw;
            }
        }

        var generations = existingPrompts
            .Take(prompts.Count)
            .Select(i => i.Value)
            .ToList();

        return new ModelResult
        {
            Generations = generations,
            Output = llmOutput
        };
    }

    protected abstract Task<ModelResult> GenerateInternalAsync(IList<string> prompts, IList<string> stop = null);

    /// <summary>
    /// Get the number of tokens present in the text.
    /// </summary>
    /// <param name="text"></param>
    /// <returns>Number of characters divided by 4 </returns>
    public virtual int GetNumTokens(string text)
    {
        // TODO: this method may not be exact.
        // TODO: this method may differ based on model (eg codex).
        // TODO: do actual base tokenization?
        return text.Length / 4;
    }

    public abstract string LLMType { get; }
}