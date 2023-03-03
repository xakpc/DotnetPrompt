using System.Text.Json;
using DotnetPrompt.Abstractions.LLM;
using DotnetPrompt.Chains;
using DotnetPrompt.LLM.OpenAI;
using DotnetPrompt.Prompts;
using NUnit.Framework;

namespace DotnetPrompt.Tests.Examples.LLMs;

public class OpenAIModelExamples
{
    private static OpenAIModel CreateOpenAiModel()
    {
        #region CreateOpenAiModel
        var llm = new OpenAIModel(Constants.OpenAIKey,
            OpenAIModelConfiguration.Default with
            {
                Model = "text-ada-001",
                SnippetCount = 1,
                GenerationSampleCount = 2,
                Temperature = 0.9f
            });
        #endregion
        return llm;
    }
    
    [Test]
    public async Task Example_PromptLLM()
    {
        #region Example_PromptLLM
        var llm = CreateOpenAiModel();

        var output = await llm.PromptAsync("Tell me a joke");
        Console.WriteLine(output);
        #endregion
    }

    [Test]
    public async Task Example_PromptLLM2()
    {
        #region Example_PromptLLM2
        var llm = new OpenAIModel(Constants.OpenAIKey, OpenAIModelConfiguration.Default with
        {
            Temperature = 0.9f
        });

        var oneInputPrompt =
            new PromptTemplate(template: "What's a funny response to '{message}'", inputVariables: new[] { "message" });

        var valuesOneInput = new Dictionary<string, string>
        {
            { "message", "I have some exciting news to share with you!" }
        };
        var text = oneInputPrompt.Format(valuesOneInput);
        Console.WriteLine(text);

        var response = await llm.PromptAsync(text);
        Console.WriteLine(response);

        var chain = new ModelChain(oneInputPrompt, llm);
        var executor = chain.GetExecutor();

        var result = await executor.PromptAsync("I have some exciting news to share with you!");
        Console.WriteLine(result);
        var result2 = await executor.PromptAsync("Want to grab lunch later?");
        Console.WriteLine(result2);
        #endregion
    }

    [Test]
    public async Task Example_GenerateAsync()
    {
        #region Example_GenerateAsync

        var llm = new OpenAIModel(Constants.OpenAIKey, OpenAIModelConfiguration.Default with
        {
            SnippetCount = 2,
            GenerationSampleCount = 2,
            Temperature = 0.9f
        });

        var prompts = new[] { "Tell me a joke", "Tell me a poem" };

        // we generate two completions on each prompt
        var output = await llm.GenerateAsync(prompts);

        Console.WriteLine(JsonSerializer.Serialize(output));

        #endregion

    }
}