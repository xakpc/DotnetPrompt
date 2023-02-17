using DotnetPrompt.LLM.OpenAI;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using System.Text.Json;

namespace DotnetPrompt.Test.Examples.LLMs;

public class OpenAIModelExamples
{
    private static OpenAIModel CreateOpenAiModel()
    {
        #region CreateOpenAiModel
        var llm = new OpenAIModel(OpenAIKey,
            OpenAIModelConfiguration.Default with
            {
                Model = "text-ada-001",
                SnippetCount = 2,
                GenerationSampleCount = 2,
                Temperature = 0.9f
            },
            NullLogger.Instance);
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
    public async Task Example_GenerateText()
    {
        var llm = CreateOpenAiModel();

        var prompts = Enumerable.Repeat(new[] { "Tell me a joke", "Tell me a poem" }, 15).SelectMany(arr => arr).ToArray();
        
        // we generate two completions on each prompt
        var output = await llm.GenerateAsync(prompts);

        Console.WriteLine(output.Generations.Count);
        //> 30
        Console.WriteLine(output.Generations.SelectMany(i => i).Count());
        //> 60

        Console.WriteLine(JsonSerializer.Serialize(output.Generations[0]));
        //> [{"Text":"\n\nWhy did the chicken cross the road?\n\nTo get to the other side!","Info":{"finish_reason":"stop","logprobs":null}},{"Text":"\n\nWhy did the chicken cross the road?\n\nTo get to the other side.","Info":{"finish_reason":"stop","logprobs":null}}]
        Console.WriteLine(JsonSerializer.Serialize(output.Generations[^1]));
        //> [{"Text":"\n\nHow can love last so long\n\nWhen loved has been before our eyes\n\nWe have been through everything\n\nAnd still something feels right\n\nNo matter how many times we are told\n\nThat love isn't real\n\nAnd we keep trying no matter how hard\n\nWe are trying to find our way\n\nIn every step we make\n\nWe are keeping our heartstrings in place\n\nWhile all around us people are falling\n\nAnd we are not really that different\n\nThen why on earth would I want to be with you\n\nWhen all I have to look at\n\nIs the chance that we will get together\n\nAnd how many times do I have to explain\n\nThat I want this one person only\n\nAnd I don't want any other person\n\nWho can change my life for the better\n\nSo how can love last so long\n\nWhen we have been through so much together\n\nAnd we know what love is not real\n\nBut we continue to try and find out\n\nThat love is real and we are still alive","Info":{"finish_reason":"stop","logprobs":null}},{"Text":"\n\nHow did the world get to be the way it is\n\nThere are many ways and ways can be\n\nAnd depending on what you believe\n\nThe world can be in the Applicant or theangel\n\nThe Applicant or the angel can be in the form of a person\n\nBut in all likelihood, the world will be filled with things\n\nThat you and I will never know about\n\nWe will never know about the weather\n\nWe will never know about the stars\n\nWe will never know about love\n\nWe will never know about life\n\nWe will never know about the moon\n\nWe will never know about the weather\n\nWe will never know about the stars\n\nWe will never know about love\n\nWe will never know about life\n\nWe will never know about the moon\n\nThe world has been changed by people\n\nThe world has been changed by people\n\nAnd the world will be changed by people\n\nA great many people want this world\n\nThey want the world to be the way it is\n\nAnd they are right\n\nThe world has been changed by people\n\nThe world will be changed by people\n\nSo be it.","Info":{"finish_reason":"stop","logprobs":null}}]

        Console.WriteLine(llm.GetNumTokens("what a joke"));
        //> 3
    }
}