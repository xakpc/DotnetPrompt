using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetPrompt.Abstractions.LLM;

public static class ModelExtensions
{
    /// <summary>
    /// Run the LLM on the given prompt and input.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="prompt"></param>
    /// <param name="stop"></param>
    /// <returns></returns>
    public static async Task<string> PromptAsync(this ILargeLanguageModel model, string prompt, List<string> stop = default)
    {
        return (await model.GenerateAsync(new List<string> { prompt }, stop)).Generations.FirstOrDefault()?.FirstOrDefault()?.Text.Trim();
    }
}