using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DotnetPrompt.Abstractions.Schema;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DotnetPrompt.Abstractions.LLM;

public interface ILargeLanguageModel
{
    /// <summary>
    /// Run the LLM on the given prompt and input.
    /// </summary>
    /// <param name="prompts"></param>
    /// <param name="stop"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">When cache asked without </exception>
    Task<LLMResult> GenerateAsync(IList<string> prompts, IList<string> stop = null);

    string LLMType { get; }
}

public abstract class BaseModel : ILargeLanguageModel
{
    private readonly IDistributedCache _cache;
    protected readonly ILogger _logger;

    public IList<string> DefaultStop { get; set; } = null;

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
    /// <param name="parameters"></param>
    /// <param name="prompts"></param>
    /// <returns></returns>
    public async Task<(Dictionary<int, IList<Generation>> ExistingPrompts, 
        string LLMString, IList<int> MissingPromptIdxs, 
        IList<string> MissingPrompts)> 
        GetPromptsAsync(IList<string> prompts)
    {
        string llmString = AsUniqueString();

        var existingPrompts = new Dictionary<int, IList<Generation>>();
        var missingPromptIdxs = new List<int>();
        var missingPrompts = new List<string>();

        for (int i = 0; i < prompts.Count; i++)
        {
            var prompt = prompts[i];
            if (_cache != null)
            {
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
        }

        return (existingPrompts, 
            llmString, 
            missingPromptIdxs, 
            missingPrompts);
    }

    /// <summary>
    /// Return current model as unique string for caching purposes
    /// </summary>
    /// <returns></returns>
    protected abstract string AsUniqueString();

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
        LLMResult newResults,
        IList<string> prompts)
    {
        for (var i = 0; i < newResults.Generations.Count; i++)
        {
            existingPrompts[missingPromptIndexes[i]] = newResults.Generations[i];
            var prompt = prompts[missingPromptIndexes[i]];
            await SetCacheAsync(prompt, llmString, newResults.Generations[i]);
        }

        return newResults.Output;
        //return llmOutput != null ? new Dictionary<int, List<string>>(existingPrompts) : null;
    }

    #region Constructors

    protected BaseModel(ILogger logger = null, IDistributedCache cache = null)
    {
        _logger = logger ?? NullLogger.Instance;
        _cache = cache;
    }

    #endregion

    public bool UseCache { get; set; } = false;

    //public BaseCallbackManager CallbackManager { get; set; } = GetCallbackManager();

    /// <summary>
    /// Check Cache and run the LLM on the given prompt and input.
    /// </summary>
    /// <param name="prompt"></param>
    /// <returns></returns>
    public async Task<string> PromptAsync(string prompt, List<string> stop = default)
    {
       return (await GenerateAsync(new List<string> {prompt}, stop)).Generations.FirstOrDefault()?.FirstOrDefault()?.Text;
    }

    /// <summary>
    /// Run the LLM on the given prompt and input.
    /// </summary>
    /// <param name="prompts"></param>
    /// <param name="stop"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">When cache asked without </exception>
    public async Task<LLMResult> GenerateAsync(IList<string> prompts, IList<string> stop = null)
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

        if (_cache == null || !UseCache)
        {
            // This happens when _cache is Null, but Cache is True
            if (UseCache)
            {
                throw new InvalidOperationException("Asked to cache, but no cache found at `LLMCache`.");
            }

            //CallbackManager.OnLLMStart(new Dictionary<string, object> { { "name", this.GetType().Name } }, prompts,
            //    verbose: Verbose);

            try
            {
                var output = await GenerateInternalAsync(prompts, stop);
                //CallbackManager.OnLLMEnd(output, verbose: Verbose);
                return output;
            }
            catch (Exception e)
            {
                //CallbackManager.OnLLMError(e, verbose: Verbose);
                _logger.LogError(e, "LLM Error '{Message}' in {Model}", e.Message, LLMType);
                throw;
            }
        }

        var pars = this.ToDict();

        var (existingPrompts, llmString, missingPromptIdxs, missingPrompts) = await GetPromptsAsync(prompts);

        IDictionary<string, object> llmOutput = default;
        if (missingPrompts.Count > 0)
        {
            //CallbackManager.OnLLMStart(new Dictionary<string, object> { { "name", this.GetType().Name } },
            //    missingPrompts, verbose: Verbose);
            try
            {
                var newResults = await GenerateInternalAsync(missingPrompts, stop);
                //CallbackManager.OnLLMEnd(newResults, verbose: Verbose);
                llmOutput = await UpdateCache(existingPrompts, llmString, missingPromptIdxs, newResults, prompts);
            }
            catch (Exception e)
            {
                //CallbackManager.OnLLMError(e, verbose: Verbose);
                _logger.LogError(e, "LLM Error '{Message}' in {Model}", e.Message, LLMType);
                throw;
            }
        }

        var generations = existingPrompts
            .Take(prompts.Count)
            .Select(i => i.Value)
            .ToList();

        return new LLMResult
        {
            Generations = generations,
            Output = llmOutput
        };
    }

    protected abstract Task<LLMResult> GenerateInternalAsync(IList<string> prompts, IList<string> stop = null);

    /// <summary>
    /// Get the number of tokens present in the text.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public virtual int GetNumTokens(string text)
    {
        // 
        // TODO: this method may not be exact.
        // TODO: this method may differ based on model (eg codex).
        // TODO: do actual tokenization?

        return text.Length;
        //try
        //{
        //    // create a GPT-3 tokenizer instance
        //    var tokenizer = GPT2TokenizerFast.FromPretrained("gpt2");

        //    // tokenize the text using the GPT-3 tokenizer
        //    var tokenizedText = tokenizer.Tokenize(text);

        //    // calculate the number of tokens in the tokenized text
        //    return tokenizedText.Count;
        //}
        //catch (ImportException)
        //{
        //    throw new ValueError(
        //        "Could not import transformers python package. " +
        //        "This is needed in order to calculate get_num_tokens. " +
        //        "Please it install it with `pip install transformers`."
        //    );
        //}

    }

    public Dictionary<string, dynamic> IdentifyingParams
    {
        // Get the identifying parameters.
        get { return new Dictionary<string, dynamic> { }; }
    }

    public override string ToString()
    {
        // Get a string representation of the object for printing.

        var clsName = $"{this.GetType().Name}";
        return $"{clsName}\nParams: {this.IdentifyingParams}";
    }

    public abstract string LLMType { get; }

    public Dictionary<string, dynamic> ToDict()
    {
        // Return a dictionary of the LLM.

        var starterDict = this.IdentifyingParams;
        starterDict["_type"] = this.LLMType;
        return starterDict;
    }
}