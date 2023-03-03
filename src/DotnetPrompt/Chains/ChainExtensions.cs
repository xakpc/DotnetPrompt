using System;
using System.Threading.Tasks.Dataflow;
using DotnetPrompt.Abstractions.Chains;

namespace DotnetPrompt.Chains;

/// <summary>
/// Extension class for <see cref="IChain"/>
/// </summary>
public static class ChainExtensions
{
    /// <summary>
    /// Link several chain together
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
    /// <returns>
    /// </returns>
    /// <exception cref="InvalidOperationException">Source output does not match target input</exception>
    public static IDisposable LinkTo(this IChain source, IChain target)
    {
        if (!target.InputVariables.Contains(source.DefaultOutputKey))
        {
            throw new InvalidOperationException("Source output does not match target input");
        }

        return source.OutputBlock.LinkTo(target.InputBlock, new DataflowLinkOptions() { PropagateCompletion = true });
    }
}