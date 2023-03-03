using DotnetPrompt.Abstractions.LLM;
using DotnetPrompt.Chains;
using DotnetPrompt.LLM.OpenAI;
using DotnetPrompt.Prompts;
using NUnit.Framework;

namespace DotnetPrompt.Tests.Examples.LLMs;

public class ChatGptModelExamples
{
    private static ChatGptModel CreateChatGptModel()
    {
        #region CreateOpenAiModel
        var llm = new ChatGptModel(Constants.OpenAIKey, ChatGptModelConfiguration.Default with { 
                SnippetCount = 1,
                Temperature = 0.9f });
        #endregion
        return llm;
    }

    [Test]
    public async Task Example_PromptLLM()
    {
        #region Example_PromptLLM
        var llm = CreateChatGptModel();

        var output = await llm.PromptAsync("Tell me a joke");
        Console.WriteLine(output);
        #endregion
    }

    [Test]
    public async Task Example_PromptLLM2()
    {
        #region Example_PromptLLM2

        // create llm
        var llm = CreateChatGptModel();

        // create prompt template
        var oneInputPrompt = new PromptTemplate(template: "What's a funny response to '{message}'", inputVariables: new[] { "message" });
        var chatTemplate = new ChatMLPromptTemplate(oneInputPrompt, system: "You are my buddy");

        // create value
        var valuesOneInput = new Dictionary<string, string>
        {
            { "message", "I have some exciting news to share with you!" }
        };

        // create text for llm
        var text = chatTemplate.Format(valuesOneInput);
        Console.WriteLine(text);

        // get answer from llm
        var response = await llm.PromptAsync(text);
        Console.WriteLine(response);

        // create chain
        var chain = new ModelChain(chatTemplate, llm);
        var executor = chain.GetExecutor();

        // get answer from the chain providing different values
        var result = await executor.PromptAsync("I have some exciting news to share with you!");
        Console.WriteLine(result);
        var result2 = await executor.PromptAsync("Want to grab lunch later?");
        Console.WriteLine(result2);
        #endregion
    }
}