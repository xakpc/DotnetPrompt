using DotnetPrompt.Chains;
using DotnetPrompt.LLM.OpenAI;
using DotnetPrompt.Prompts;
using DotnetPrompt.Tests.Integration;
using NUnit.Framework;

namespace DotnetPrompt.Tests.Examples.Chains;

[TestFixture]
public class ModelChainExamples
{
    [Test]
    public async Task Example_ModelChain_SingleInput()
    {
        #region ModelChain_Example_SingleInput
        var template = "Question: {question}\n\n" +
                       "Answer: Let's think step by step.";

        var prompt = new PromptTemplate(template, new[] { "question" });
        var llm = new OpenAIModel(Constants.OpenAIKey, OpenAIModelConfiguration.Default with { Temperature = 0 });
        var llmChain = new ModelChain(prompt, llm);
        var executor = new OneShotChainExecutor(llmChain);

        var question = "What new discoveries from the James Webb Space Telescope can I tell my 9 year old about?";

        var answer = await executor.PromptAsync(question);

        Console.WriteLine(answer);
        #endregion
    }

    [Test]
    public async Task Example_ModelChain_SingleInputWithLogger()
    {
        #region ModelChain_Example_SingleInputWithLogger
        var template = "Question: {question}\n\n" +
                       "Answer: Let's think step by step.";

        var prompt = new PromptTemplate(template, new[] { "question" });
        var llm = new OpenAIModel(Constants.OpenAIKey, OpenAIModelConfiguration.Default with { Temperature = 0 }, TestLogger.Create<OpenAIModel>());
        var llmChain = new ModelChain(prompt, llm, TestLogger.Create<ModelChain>());
        var executor = llmChain.GetExecutor();

        var question = "What new discoveries from the James Webb Space Telescope can I tell my 9 year old about?";

        var answer = await executor.PromptAsync(question);

        Console.WriteLine(answer);
        #endregion
    }

    [Test]
    public async Task Example_ModelChain_MultipleInput()
    {
        #region ModelChain_Example_MultipleInput
        var template = "Write a {adjective} poem about {subject}.";

        var prompt = new PromptTemplate(template, new[] { "adjective", "subject" });
        var llm = new OpenAIModel(Constants.OpenAIKey, OpenAIModelConfiguration.Default with { Temperature = 0.2f });
        var llmChain = new ModelChain(prompt, llm);
        var executor = llmChain.GetExecutor();

        var question = new Dictionary<string, string>()
        {
            { "adjective", "tragic" },
            { "subject", "OpenAI" },
        };

        var answer = await executor.PromptAsync(question);

        Console.WriteLine(answer["text"]);
        #endregion
    }
}