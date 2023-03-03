using DotnetPrompt.Abstractions.Prompts;
using DotnetPrompt.Prompts;
using NUnit.Framework;

namespace DotnetPrompt.Tests.Unit.Prompts;

[TestFixture]
public class FewShotPromptTemplateTests
{
    private readonly IPromptTemplate _examplePromptTemplate = new
        PromptTemplate("{question}: {answer}", new[] { "question", "answer" });

    private readonly IPromptTemplate _suffixPromptTemplate = new PromptTemplate("This is a {foo} test.");

    [Test]
    public void Format_SuffixOnly_ValidText()
    {
        // Test prompt works with just a suffix.
        var prompt = new FewShotPromptTemplate(_examplePromptTemplate, _suffixPromptTemplate, 
            examples: new List<IDictionary<string, string>>());

        var inputValues = new Dictionary<string, string>()
        {
            { "foo", "bar" } 
        };
        var output = prompt.Format(inputValues);

        Assert.AreEqual("This is a bar test.", output);
    }

    [Test]
    public void Format_PrefixAndSuffix_ValidText()
    {
        // Test when missing in prefix
        var prompt = new FewShotPromptTemplate(new PromptTemplate("foo"), _examplePromptTemplate, _suffixPromptTemplate,
            examples: new List<IDictionary<string, string>>());

        var inputValues = new Dictionary<string, string>()
        {
            { "foo", "bar" }
        };
        var output = prompt.Format(inputValues);

        Assert.AreEqual("foo\n\nThis is a bar test.", output);
    }

    [Test]
    public void Format_InvalidValues_ArgumentException()
    {
        // Test error is raised when there are too many input variables.
        Assert.Throws<ArgumentException>(() =>
        {
            var prompt = new FewShotPromptTemplate(_examplePromptTemplate, _suffixPromptTemplate,
                examples: new List<IDictionary<string,string>>());

            var inputValues = new Dictionary<string, string>()
            {
                { "bar", "bar" }
            };
            prompt.Format(inputValues);
        });
    }

    [Test]
    public void Format_ValidExamples_ValidOutput()
    {
        // Test that few shot works with examples.
        var prefix = new PromptTemplate("This is a test about {content}.");
        var suffix = new PromptTemplate("Now you try to talk about {new_content}.");
        var example = new PromptTemplate("{question}: {answer}", new[] { "question", "answer" });
        List<IDictionary<string, string>> examples = new ()
        {
            new Dictionary<string,string> {{"question", "foo"},{"answer", "bar"}},
            new Dictionary<string, string> {{"question", "baz"},{"answer", "foo"}},
        };

        // better
        var prompt = new FewShotPromptTemplate(prefix, example, suffix, examples);

        var inputValues = new Dictionary<string, string>()
        {
            {"content","animals"},
            {"new_content","party"}
        };

        var output = prompt.Format(inputValues);
        var expectedOutput = "This is a test about animals.\n\nfoo: bar\n\nbaz: foo\n\nNow you try to talk about party.";
        Assert.AreEqual(expectedOutput, output);
    }
}
