using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotnetPrompt.Abstractions.Chains;

/// <summary>
/// Chain executor to run chains
/// </summary>
public interface IChainExecutor
{
    /// <summary>
    /// Execute chain with single string input and single string output
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public Task<string> PromptAsync(string input);

    /// <summary>
    /// Execute chain with default inputs and outputs
    /// </summary>
    /// <param name="input"></param>
    /// <param name="stops"></param>
    /// <returns></returns>
    public Task<IDictionary<string, string>> PromptAsync(IDictionary<string, string> input, List<string> stops = null);
}