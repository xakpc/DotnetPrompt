using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using DotnetPrompt.Abstractions.Chains;

namespace DotnetPrompt.Chains;

/// <summary>
/// Chain executor for a single run
/// </summary>
public class OneShotChainExecutor : IChainExecutor
{
    private readonly IChain _chainToExecute;

    /// <summary>
    /// ctor
    /// </summary>
    public OneShotChainExecutor(IChain chainToExecute)
    {
        _chainToExecute = chainToExecute;
    }

    /// <inheritdoc />
    public async Task<string> PromptAsync(string input)
    {
        if (_chainToExecute.InputVariables.Count > 1 || !_chainToExecute.InputVariables.Any())
        {
            throw new InvalidOperationException(
                "Simplified PromptAsync could not be used for templates with more than one input variables or zero variables");
        }

        var context = new ChainMessage(new Dictionary<string, string> { { _chainToExecute.InputVariables[0], input } });

        var writeOnceBlock = PrepareWriteOnceBlock(_chainToExecute.OutputBlock, context);
        _chainToExecute.InputBlock.Post(context);
        var result = await CompleteAndReceiveAsync(writeOnceBlock);

        return result.Values[_chainToExecute.DefaultOutputKey];
    }

    /// <inheritdoc />
    public async Task<IDictionary<string, string>> PromptAsync(IDictionary<string, string> input, List<string>? stops = null)
    {
        var context = new ChainMessage(input, stops);

        var writeOnceBlock = PrepareWriteOnceBlock(_chainToExecute.OutputBlock, context);
        _chainToExecute.InputBlock.Post(context);
        var result = await CompleteAndReceiveAsync(writeOnceBlock);

        return result.Values;
    }

    #region Private Methods
    private static async Task<ChainMessage> CompleteAndReceiveAsync(ISourceBlock<ChainMessage> writeOnceBlock)
    {
        try
        {
            // the WriteOnceBlock would be completed on message receive, successfully or not, on fail it would produce AggregateException
            await writeOnceBlock.Completion.ConfigureAwait(false);
            return await writeOnceBlock.ReceiveAsync().ConfigureAwait(false);
        }
        catch (AggregateException e)
        {
            throw e.GetBaseException();
        }
    }
    private static WriteOnceBlock<ChainMessage> PrepareWriteOnceBlock(ISourceBlock<ChainMessage> outputBlock,
        ChainMessage message)
    {
        var writeOnceBlock = new WriteOnceBlock<ChainMessage>(null);

        // Setting MaxMessages to one instructs the source component to unlink from the WriteOnceBlock<T> object after offering the WriteOnceBlock<T> object one message.
        outputBlock.LinkTo(writeOnceBlock, new DataflowLinkOptions { MaxMessages = 1, PropagateCompletion = true },
            resultContext => resultContext.Id == message.Id);

        return writeOnceBlock;
    }
    #endregion
}

/// <summary>
/// Extension class for <see cref="IChain"/>
/// </summary>
public static class ChainExecutorExtensions
{
    /// <summary>
    /// Create chain executor for <see cref="IChain"/>
    /// </summary>
    /// <param name="chain"></param>
    /// <param name="oneShot"></param>
    /// <returns><see cref="IChainExecutor"/> instance to execute chain</returns>
    /// <exception cref="NotImplementedException"></exception>
    public static IChainExecutor GetExecutor(this IChain chain, bool oneShot = true)
    {
        if (oneShot)
        {
            return new OneShotChainExecutor(chain);
        }

        throw new NotImplementedException("Only one shot is currently supported");
    }
}