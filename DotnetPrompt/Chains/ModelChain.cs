using System;
using System.Collections.Generic;
using System.Linq;
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

    /// <inheritdoc />
    public IList<string> InputVariables => _prompt.InputVariables;

    /// <inheritdoc />
    public virtual string DefaultOutputKey { get; set; } = "text";

    /// <inheritdoc />
    public ITargetBlock<ChainMessage> InputBlock => _modelBlock;

    /// <inheritdoc />
    public ISourceBlock<ChainMessage> OutputBlock => _modelBlock;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="prompt"></param>
    /// <param name="llm"></param>
    /// <param name="logger"></param>
    public ModelChain(IPromptTemplate prompt, ILargeLanguageModel llm, ILogger? logger = null)
    {
        // chain 
        _prompt = prompt;
        _llm = llm;
        _logger = logger ?? new NullLogger<ModelChain>();

        _modelBlock = new TransformBlock<ChainMessage, ChainMessage>(Transform);
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
            var text = result.Generations.FirstOrDefault()?.FirstOrDefault()?.Text;
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