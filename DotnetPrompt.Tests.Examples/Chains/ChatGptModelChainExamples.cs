using DotnetPrompt.Chains;
using DotnetPrompt.Chains.Specialized;
using DotnetPrompt.LLM.OpenAI;
using DotnetPrompt.Prompts;
using NUnit.Framework;

namespace DotnetPrompt.Tests.Examples.Chains;

public class ChatGptModelChainExamples
{
    [Test]
    public async Task Example_ChatMLPromptTemplate_Chatbot()
    {
        #region Example_ChatMLPromptTemplate_Chatbot

        var suffix = new PromptTemplate("{human_phrase}");

        var examples = new List<(string, string)>()
        {
            new("Hello nice to meet you.", "Nice to meet you too."),
            new("How is it going today?", "Not so bad, thank you! How about you?"),
            new("I am ok, but I am a bit sad...", "Oh? Why that?")
        };

        var prompt = new ChatMLPromptTemplate(suffix, "This is a discussion between a human and a robot. The robot is very nice and empathetic.", examples);

        var llm = new ChatGptModel(Constants.OpenAIKey, ChatGptModelConfiguration.Default with { Temperature = 0.9f, MaxTokens = 100 });
        var llmChain = new ModelChain(prompt, llm);
        var executor = llmChain.GetExecutor();

        var answer1 = await executor.PromptAsync("I broke up with my girlfriend...");
        Console.WriteLine(answer1);
        var answer2 = await executor.PromptAsync("I won a lot of money today");
        Console.WriteLine(answer2);

        #endregion
    }

    [Test]
    public async Task Example_ChatMLPromptTemplate_QuestionAnswering()
    {
        #region Example_ModelChainFewShotPromptTemplate_QuestionAnswering

        var llm = new ChatGptModel(Constants.OpenAIKey, ChatGptModelConfiguration.Default with { Temperature = 0, MaxTokens = 100 });
        var llmChain = new QuestionAnsweringChain(llm);
        var executor = llmChain.GetExecutor();

        var answer1 = await executor.PromptAsync(new
        {
            context = "OpenAI offers several plans for GPT-3, ranging from a free tier to paid plans with larger amounts of access and dedicated support. The recommended plan for GPT-3 would depend on the specific needs and use case of the individual or organization. However, the most commonly used paid plan for GPT-3 is the \"Pro\" plan, which provides a significant amount of access to the API and is suitable for most applications.",
            question = "Which plan is recommended for GPT-3?"
        }.ToDictionary());

        Console.WriteLine(answer1["answer"]);

        var answer12 = await executor.PromptAsync(new
        {
            context = "GPT-3 supports many different natural languages, including English, Spanish, French, German, Italian, Dutch, Portuguese, Japanese, Korean, Chinese, and more. However, English is the language that GPT-3 has been most extensively trained on, and for which it has produced the most impressive results. Therefore, English is generally considered the most preferable language for GPT-3.",
            question = "Which language is preferable for GPT-3?"
        }.ToDictionary());
        Console.WriteLine(answer12["answer"]);

        Console.WriteLine("---");
        
        var llm2 = new OpenAIModel(Constants.OpenAIKey, OpenAIModelConfiguration.Default with { Temperature = 0, MaxTokens = 100 });
        var llmChain2 = new QuestionAnsweringChain(llm2);
        var executor2 = llmChain2.GetExecutor();

        var answer2 = await executor2.PromptAsync(new
        {
            context = "OpenAI offers several plans for GPT-3, ranging from a free tier to paid plans with larger amounts of access and dedicated support. The recommended plan for GPT-3 would depend on the specific needs and use case of the individual or organization. However, the most commonly used paid plan for GPT-3 is the \"Pro\" plan, which provides a significant amount of access to the API and is suitable for most applications.",
            question = "Which plan is recommended for GPT-3?"
        }.ToDictionary());
        Console.WriteLine(answer2["answer"]);
        
        var answer22 = await executor.PromptAsync(new
        {
            context = "GPT-3 supports many different natural languages, including English, Spanish, French, German, Italian, Dutch, Portuguese, Japanese, Korean, Chinese, and more. However, English is the language that GPT-3 has been most extensively trained on, and for which it has produced the most impressive results. Therefore, English is generally considered the most preferable language for GPT-3.",
            question = "Which language is preferable for GPT-3?"
        }.ToDictionary());
        Console.WriteLine(answer22["answer"]);
        #endregion
    }
}