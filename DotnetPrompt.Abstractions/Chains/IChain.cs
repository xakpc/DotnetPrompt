using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;

namespace DotnetPrompt.Abstractions.Chains;

/// <summary>
/// Generic Chain
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
/// <remarks>Experimental, not yet used</remarks>
public interface IChain <TInput, TOutput>
{
    ITargetBlock<TInput> InputBlock { get; }
    ISourceBlock<TOutput> OutputBlock { get; }

    bool Run(TInput message)
    {
        return InputBlock.Post(message);
    }
}


/// <summary>
/// Basic interface for a chain
/// </summary>
public interface IChain : IChain<ChainMessage, ChainMessage>
{
    /// <summary>
    /// List of inputs chain require to run
    /// </summary>
    IList<string> InputVariables { get; }

    /// <summary>
    /// Output chain produces
    /// </summary>
    string DefaultOutputKey { get; set; }
}