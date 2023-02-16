using DotnetPrompt.Prompts;
using NUnit.Framework;

namespace DotnetPrompt.Tests.Unit.Prompts;

public class PromptTemplateTests
{
    [Test]
    public void Constructor_ValidInput_ValidOutput()
    {
        var prompt = new PromptTemplate("This is a {foo} test.", new[] { "foo" });

        Assert.AreEqual("This is a {foo} test.", prompt.Template);
        Assert.AreEqual(new[] { "foo" }, prompt.InputVariables);
    }

    [Test]
    public void Format_ProperValues_ProperTextGenerated()
    {
        // Test prompt works with just a suffix.
        var prompt = new PromptTemplate("This is a {foo} test.", new[] { "foo" });

        var output = prompt.Format(new Dictionary<string, string> { {"foo","bar"} });

        Assert.AreEqual("This is a bar test.", output);
    }

    [TestCase("This is a foo test.", new string[] { })]
    [TestCase("This is a {foo} test.", new[] { "foo" })]
    [TestCase("This {bar} is a {foo} test.", new[] { "bar", "foo" })]
    [TestCase("This {bar} is a {foo} test {foo}.", new[] { "bar", "foo" })]
    public void FromTemplate_ProperString_ProperNumberOfVariables(string template, string[] inputVariables)
    {
        var prompt = new PromptTemplate(template);
        var expectedPrompt = new PromptTemplate(template, inputVariables);

        CollectionAssert.AreEqual(expectedPrompt.InputVariables, prompt.InputVariables);
    }

    [Test]
    public void Constructor_InputVariablesNotProvided_ArgumentExceptionRaised()
    {
        const string template = "This is a {foo} test.";
        var inputVariables = new List<string>();

        Assert.Throws<ArgumentException>(() => new PromptTemplate(template, inputVariables));
    }

    [Test]
    public void Constructor_TooManyInputVariables_ArgumentExceptionRaised()
    {
        const string template = "This is a {foo} test.";
        var inputVariables = new List<string> { "foo", "bar" };

        Assert.Throws<ArgumentException>(() => new PromptTemplate(template, inputVariables));
    }

    [Test]
    public void Constructor_JsonInjection_ConvertedProperly()
    {
        const string template = "This is a {foo} with {{\"json\":\"injection\"}} test.";
        var inputVariables = new List<string> { "foo" };
        var prompt = new PromptTemplate(template, inputVariables);

        Assert.AreEqual("This is a {foo} with {\"json\":\"injection\"} test.", prompt.Template);
        Assert.AreEqual(new[] { "foo" }, prompt.InputVariables);
    }

    [Test]
    public void PromptTemplate_WrongInputVariableName_ErrorRaised()
    {
        const string template = "This is a {foo} test.";
        var inputVariables = new List<string> { "bar" };

        Assert.Throws<ArgumentException>(() => new PromptTemplate(template, inputVariables));
    }

    /// <summary>
    /// Test prompt can be successfully constructed from examples.
    /// </summary>
    [Test]
    public void PromptTemplate_FromExamples_Valid()
    {
        var template = "Test Prompt:\n\n" +
                       "Question: who are you?\n" +
                       "Answer: foo\n\n" +
                       "Question: what are you?\n" +
                       "Answer: bar\n\n" +
                       "Question: {question}\n" +
                       "Answer:";
        var inputVariables = new List<string> { "question" };

        var exampleSeparator = "\n\n";
        var prefix = "Test Prompt:";
        var suffix = "Question: {question}\nAnswer:";
        var examples = new List<string>
        {
            "Question: who are you?\nAnswer: foo",
            "Question: what are you?\nAnswer: bar"
        };

        var promptFromExamples = PromptTemplate.FromExamples(examples, suffix, inputVariables, prefix, exampleSeparator);

        var promptFromTemplate = new PromptTemplate(template, inputVariables);

        Assert.AreEqual(promptFromExamples.Template, promptFromTemplate.Template);
        Assert.AreEqual(promptFromExamples.InputVariables, promptFromTemplate.InputVariables);
    }

    /// <summary>
    /// Test prompt can be successfully constructed from a file.
    /// </summary>
    [Test]
    public void PromptFromFile_TemplateFile_SuccessfulConstruction()
    {
        // Arrange
        var templateFile = "data/prompt_file.txt"; // file is saved with Unix line-ending (CF or \n)
        var inputVariables = new[] { "question" };

        // Act
        var prompt = PromptTemplate.FromFile(templateFile, inputVariables);

        // Assert
        Assert.AreEqual("Question: {question}\nAnswer:", prompt.Template);
    }
}