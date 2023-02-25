using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotnetPrompt.Abstractions.Chains;

public interface IChainExecutor
{
    public Task<string> PromptAsync(string input);

    public Task<IDictionary<string, string>> PromptAsync(IDictionary<string, string> input, List<string> stops = null);
}