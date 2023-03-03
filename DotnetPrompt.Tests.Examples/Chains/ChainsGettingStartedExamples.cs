using DotnetPrompt.Chains;
using DotnetPrompt.LLM.OpenAI;
using DotnetPrompt.Prompts;
using NUnit.Framework;

namespace DotnetPrompt.Tests.Examples.Chains;

public class ChainsGettingStartedExamples
{
    [Test]
    public async Task Example_QueryLLM()
    {
        #region Example_QueryLLM_Prompt
        var llm = new OpenAIModel(Constants.OpenAIKey, OpenAIModelConfiguration.Default with { Temperature = 0.9f });
        var prompt = new PromptTemplate("What is a good name for a company that makes {product}?");
        #endregion
        #region Example_QueryLLM_Model
        var chain = new ModelChain(prompt, llm);
        var executor = chain.GetExecutor();

        // Run the chain only specifying the input variable.
        var result = await executor.PromptAsync("colorful socks");
        Console.WriteLine(result);
        #endregion
    }
}