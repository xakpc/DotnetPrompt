using DotnetPrompt.Chains;
using DotnetPrompt.Chains.Specialized;
using DotnetPrompt.LLM.OpenAI;
using NUnit.Framework;

namespace DotnetPrompt.Tests.Examples.Chains;

public class SpecializedChainsExamples
{
    [Test]
    public async Task Example_SummarizeChain()
    {
        var llmChain = new SummarizeChain(new OpenAIModel(Constants.OpenAIKey, OpenAIModelConfiguration.Default));
        var executor = llmChain.GetExecutor();

        var text =
            "Two independent experiments reported their results this morning at CERN, Europe's high-energy physics laboratory near Geneva in Switzerland. Both show convincing evidence of a new boson particle weighing around 125 gigaelectronvolts, which so far fits predictions of the Higgs previously made by theoretical physicists.\r\n\r\n\"As a layman I would say: 'I think we have it'. Would you agree?\" Rolf-Dieter Heuer, CERN's director-general, asked the packed auditorium. The physicists assembled there burst into applause.";

        var answer = await executor.PromptAsync(text);

        Console.WriteLine(answer);
    }
}