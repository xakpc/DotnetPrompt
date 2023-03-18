using DotnetPrompt.Chains;
using DotnetPrompt.Chains.Specialized;
using DotnetPrompt.LLM.OpenAI;
using DotnetPrompt.Prompts;
using DotnetPrompt.Tests.Integration;
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

    [Test]
    public async Task LongExample_SummarizeChain()
    {
        var mapChain = new SummarizeChain(new OpenAIModel(Constants.OpenAIKey, OpenAIModelConfiguration.Default), TestLogger.Create<SummarizeChain>());

        var reducePrompt =
            "Given a set of summaries for a long text, use a summarization algorithm to combine them into a final summary." +
            "The output should be a single summary that captures the main ideas of the entire text.\n\n" +
            "{input}";

        var reduceChain = new ModelChain(new PromptTemplate(reducePrompt), new OpenAIModel(Constants.OpenAIKey, OpenAIModelConfiguration.Default), TestLogger.Create<SummarizeChain>());

        var text =
            @"The field of artificial intelligence (AI) has grown tremendously in recent years, with new breakthroughs and innovations emerging on a regular basis. From autonomous vehicles and smart homes to personal assistants and advanced medical diagnoses, AI is rapidly transforming the way we live and work.

One of the key areas of focus in AI research is natural language processing (NLP), which aims to enable machines to understand and generate human language. This has given rise to a range of applications, from chatbots and virtual assistants to sentiment analysis and automated translation.

However, despite these advances, NLP still faces many challenges. For example, language is inherently ambiguous and context-dependent, making it difficult for machines to accurately interpret meaning. Additionally, there are issues of bias and ethics to consider, particularly when it comes to sensitive topics like race, gender, and religion.

Despite these challenges, researchers are making significant progress in the field of NLP. For example, recent advances in deep learning and neural networks have enabled machines to perform increasingly complex language tasks with greater accuracy and efficiency. In addition, new techniques for data preprocessing and model training are helping to improve the performance of NLP systems.

Looking to the future, the potential applications of NLP are vast and varied. From improving customer service to enhancing medical diagnoses, NLP has the potential to revolutionize many aspects of our lives. However, as with any technology, there are also risks and challenges to consider, particularly when it comes to issues of privacy, security, and bias.

In conclusion, while NLP is still a developing field, it holds enormous promise for the future. With continued research and innovation, we can expect to see even more exciting breakthroughs in the years to come.";

        var mapReduceChain = new MapReduceChain(mapChain, reduceChain)
        {
            MaxTokens = 100
        };
        var executor = mapReduceChain.GetExecutor();

        var answer = await executor.PromptAsync(text);

        var llmChain = new SummarizeChain(new OpenAIModel(Constants.OpenAIKey, OpenAIModelConfiguration.Default));
        var llmExecutor = llmChain.GetExecutor();
        Console.WriteLine("---");
        Console.WriteLine(answer);

        var answer2 = await llmExecutor.PromptAsync(text);
        Console.WriteLine(answer2);
    }
}