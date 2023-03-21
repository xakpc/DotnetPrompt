using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using DotnetPrompt.Abstractions.Chains;
using DotnetPrompt.Abstractions.LLM;
using DotnetPrompt.Abstractions.Prompts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DotnetPrompt.Chains;

/// <summary>
/// LLM chain
/// </summary>
public class ModelChain : IChain
{
    private readonly TransformBlock<ChainMessage, ChainMessage> _modelBlock;
    private readonly IPromptTemplate _prompt;
    private readonly ILargeLanguageModel _llm;
    private readonly ILogger _logger;

    private readonly CancellationTokenSource _cancellationTokenSource = new(TimeSpan.FromMinutes(1));

    /// <inheritdoc />
    public IList<string> InputVariables => _prompt.InputVariables;

    /// <inheritdoc />
    public string DefaultOutputKey { get; set; }

    /// <inheritdoc />
    public ITargetBlock<ChainMessage> InputBlock => _modelBlock;

    /// <inheritdoc />
    public ISourceBlock<ChainMessage> OutputBlock => _modelBlock;

    /// <inheritdoc />
    public void Cancel()
    {
        _cancellationTokenSource.Cancel();
    }

    /// <summary>
    /// Maximum number of tokens
    /// </summary>
    public int ModelMaxRequestTokens => _llm.MaxRequestTokens;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="prompt">Prompt template for chain to use in LLM</param>
    /// <param name="llm">LLM to use</param>
    /// <param name="defaultOutputKey">Default key for chain result</param>
    /// <param name="logger">Default logger</param>
    public ModelChain(IPromptTemplate prompt, ILargeLanguageModel llm, string defaultOutputKey = "text", ILogger? logger = null)
    {
        // chain 
        _prompt = prompt;
        _llm = llm;
        _logger = logger ?? new NullLogger<ModelChain>();

        DefaultOutputKey = defaultOutputKey;

        if (InputVariables.Any(s => DefaultOutputKey.Equals(s)))
        {
            throw new InvalidOperationException("DefaultOutputKey should not be the same as InputValues");
        }

        _modelBlock = new TransformBlock<ChainMessage, ChainMessage>(Transform, new ExecutionDataflowBlockOptions()
        {
            CancellationToken = _cancellationTokenSource.Token
        });
    }

    private async Task<ChainMessage> Transform(ChainMessage message)
    {
        try
        {
            _logger.LogTrace("ModelChain.Transformation input message: {Context}", message);
            var input = message.Values;
            var stops = message.Stops;

            // format prompt on inputs
            var prompt = _prompt.Format(input);

            _logger.LogDebug("Input prompt after formatting\n{Prompt}", prompt);

            _logger.LogTrace("Sending LLM request");
            // perform llm generation
            var result = await _llm.GenerateAsync(new List<string>() { prompt }, stops);
            _logger.LogTrace("LLM response: {result}", result);

            // expand input and pass to the next
            var text = result.Generations.FirstOrDefault()?.FirstOrDefault()?.Text?.Trim();
            _logger.LogInformation("Result of ModelChain: {text}", text);
            input.Add(DefaultOutputKey, text);
            return message;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, $"ModelChain.Transform failure with {ex.Message}. Chain might be in Fault state.");
            throw; // todo: handle ex and fault states
        }
    }
}