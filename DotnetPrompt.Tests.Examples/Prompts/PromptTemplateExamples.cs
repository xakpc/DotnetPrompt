using DotnetPrompt.Abstractions.LLM;
using DotnetPrompt.Abstractions.Prompts;
using DotnetPrompt.LLM.OpenAI;
using DotnetPrompt.Prompts;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace DotnetPrompt.Tests.Examples.Prompts;

public class PromptTemplateExamples
{
    [Test]
    public void Example_CreatingPromptTemplate()
    {
        #region Example_CreatingPromptTemplate
        var template = "I want you to act as a naming consultant for new companies.\n\n" +
                       "Here are some examples of good company names:\n\n" +
                       "- search engine, Google\n" +
                       "- social media, Facebook\n" +
                       "- video sharing, YouTube\n\n" +
                       "The name should be short, catchy and easy to remember.\n\n" +
                       "What is a good name for a company that makes {product}?\n";

        var prompt = new PromptTemplate(
            template: template,
            inputVariables: new[] { "product" });
        #endregion

        #region Example_CreatingSeveralPromptTemplate
        // An example prompt with no input variables
        var noInputPrompt = new PromptTemplate("Tell me a joke.");
        Console.WriteLine(noInputPrompt.Format(new Dictionary<string, string>()));
        //> "Tell me a joke."

        //An example prompt with one input variable
        var oneInputPrompt =
            new PromptTemplate(template: "Tell me a {adjective} joke.", inputVariables: new[] { "adjective" });

        var valuesOneInput = new Dictionary<string, string>
        {
            { "adjective", "funny" }
        };
        Console.WriteLine(oneInputPrompt.Format(valuesOneInput));
        //> "Tell me a funny joke."

        //An example prompt with multiple input variables
        var multipleInputPrompt = new PromptTemplate("Tell me a {adjective} joke about {content}.",
            new[] { "adjective", "content" });

        var valuesMultipleInput = new Dictionary<string, string>
        {
            { "adjective", "funny" },
            { "content", "chickens" }
        };
        Console.WriteLine(multipleInputPrompt.Format(valuesMultipleInput));
        //> "Tell me a funny joke about chickens."
        #endregion
    }
}