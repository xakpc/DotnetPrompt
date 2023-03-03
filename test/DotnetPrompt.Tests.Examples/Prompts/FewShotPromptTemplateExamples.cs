using DotnetPrompt.Prompts;
using NUnit.Framework;

namespace DotnetPrompt.Tests.Examples.Prompts;

public class FewShotPromptTemplateExamples
{
    [Test]
    public void Example_FewShotPromptTemplate_Simple()
    {
        #region Example_FewShotPromptTemplate_Simple
        var prefix = new PromptTemplate("I want you to act as a naming consultant for new companies.\n" +
                                        "Here are some examples of good company names:");
        var example = new PromptTemplate("- {product}, {company}");
        var suffix = new PromptTemplate("The name should be short, catchy and easy to remember.\n" +
                                        "What is a good name for a company that makes {product}?\n");

        var examples = new List<IDictionary<string, string>>()
        {
            new Dictionary<string, string>()
                { { "product", "search engine" }, { "company", "Google" } },
            new Dictionary<string, string>()
                { { "product", "social media" }, { "company", "Facebook" } },
            new Dictionary<string, string>()
                { { "product", "video sharing" }, { "company", "YouTube" } },
        };

        var prompt = new FewShotPromptTemplate(prefix, example, suffix, examples)
        {
            ExampleSeparator = "\n"
        };

        var values = new Dictionary<string, string>()
        {
            { "product", "toy cars" }
        };

        Console.WriteLine(prompt.Format(values));
        #endregion
    }
}