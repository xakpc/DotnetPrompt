using DotnetPrompt.Abstractions.Prompts;
using DotnetPrompt.Prompts;
using NUnit.Framework;

namespace DotnetPrompt.Test.Examples.Prompts;

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

    [Test]
    public void Example_CustomPromptTemplate()
    {
        #region Example_CustomPromptTemplate
        var funcExplainer = new FunctionExplainerPromptTemplate();

        // Generate a prompt for the function "format" if python file "data/few_shot.py"
        var values = new Dictionary<string, string>()
        {
            {"function_file","data/few_shot.py"},
            {"function_name","format"},
        };

        var prompt = funcExplainer.Format(values);
        Console.WriteLine(prompt);
        #endregion
    }

    #region Example_CustomPromptTemplate_FunctionExplainerPromptTemplate
    internal class FunctionExplainerPromptTemplate : IPromptTemplate
    {
        public FunctionExplainerPromptTemplate()
        {
            InputVariables = new List<string>() { "function_file", "function_name" };
        }

        public IList<string> InputVariables { get; set; }

        public string Format(IDictionary<string, string> values)
        {
            // Get the source code of the function
            var methods = PythonHelpers.GetPythonMethods(values["function_file"]);
            var method = methods.First(m => m.Name == values["function_name"]);

            // Generate the prompt to be sent to the language model
            var prompt = "Given the function name and source code, generate an English language explanation of the function.\n" +
                         $"Function Name: {values["function_name"]}\n\n" +
                         "Source Code:\n" +
                         method.Def +
                         method.Body +
                         "Explanation:\n";
            return prompt;
        }
    }
    #endregion
}