using DotnetPrompt.Chains;
using DotnetPrompt.LLM.CohereAI;
using DotnetPrompt.LLM.OpenAI;
using DotnetPrompt.Prompts;
using NUnit.Framework;

namespace DotnetPrompt.Tests.Examples.Chains;

public class FewShotModelExamples
{
    [Test]
    public async Task Example_ModelChainFewShotPromptTemplate_SentimentAnalysis()
    {
        #region Example_ModelChainFewShotPromptTemplate_SentimentAnalysis
        var example = new PromptTemplate("Message: {message}\nSentiment: {sentiment}");
        var suffix = new PromptTemplate("Message: {message}\nSentiment: ");

        var examples = new List<IDictionary<string, string>>()
        {
            new Dictionary<string, string>() { { "message", "Support has been terrible for 2 weeks..." }, {"sentiment", "Negative" } },
            new Dictionary<string, string>() { { "message", "I love your framework, it is simple and so fast!" }, {"sentiment", "Positive" } },
            new Dictionary<string, string>() { { "message", "ChatGPT has been released 10 months ago." }, {"sentiment", "Neutral" } },
        };

        var prompt = new FewShotPromptTemplate(example, suffix, examples);

        var llm = new OpenAIModel(Constants.OpenAIKey, OpenAIModelConfiguration.Default with { Temperature = 0.2f });
        var llmChain = new ModelChain(prompt, llm);
        var executor = llmChain.GetExecutor();

        var answer1 = await executor.PromptAsync("The reactivity of your team was very good, thanks!");
        Console.WriteLine(answer1);

        var answer2 = await executor.PromptAsync("I hate you work, it's sloppy and lazy!");
        Console.WriteLine(answer2);

        var answer3 = await executor.PromptAsync("Today is a monday");
        Console.WriteLine(answer3);
        #endregion
    }

    [Test]
    public async Task Example_ModelChainFewShotPromptTemplate_HTMLGeneration()
    {
        #region Example_ModelChainFewShotPromptTemplate_HTMLGeneration
        var example = new PromptTemplate("Description: {description}\nCode: {code}");
        var suffix = new PromptTemplate("Description: {message}:\nCode: ");

        var examples = new List<IDictionary<string, string>>()
        {
            new Dictionary<string, string>() { { "description", "a red button that says stop" }, {"code", "<button style = color:white; background-color:red;>Stop</button>" } },
            new Dictionary<string, string>() { { "description", "a blue box that contains yellow circles with red borders" }, { "code", "<div style = background - color: blue; padding: 20px;><div style = background - color: yellow; border: 5px solid red; border-radius: 50%; padding: 20px; width: 100px; height: 100px;>" } },
        };

        var prompt = new FewShotPromptTemplate(example, suffix, examples);

        var llm = new OpenAIModel(Constants.OpenAIKey, OpenAIModelConfiguration.Default with { Temperature = 0.2f });
        var llmChain = new ModelChain(prompt, llm);
        var executor = llmChain.GetExecutor();

        var answer1 = await executor.PromptAsync("a Headline saying Welcome to AI");
        Console.WriteLine(answer1);
        var answer2 = await executor.PromptAsync("a list of 5 countries in yellow on black background");
        Console.WriteLine(answer2);
        #endregion
    }
}