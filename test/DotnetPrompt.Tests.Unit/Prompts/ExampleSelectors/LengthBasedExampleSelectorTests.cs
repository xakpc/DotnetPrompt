using DotnetPrompt.Prompts.ExampleSelectors;
using DotnetPrompt.Prompts;
using NUnit.Framework;

namespace DotnetPrompt.Tests.Unit.Prompts.ExampleSelectors;

[TestFixture]
public class LengthBasedExampleSelectorTests
{
    private static List<IDictionary<string, string>> _examples;

    [SetUp]
    public void Setup()
    {
        _examples = new List<IDictionary<string, string>>
        {
            new Dictionary<string, string>() {{"question", "Question: who are you?\nAnswer: foo"}},
            new Dictionary<string, string>() {{"question", "Question: who are you?\nAnswer: foo"}}
        };
    }

    [Test]
    public void SelectExamples_ShortQuestion_Valid()
    {
        var selector = BuildSelector();
        var shortQuestion = "Short question?";
        var output = selector.SelectExamples(new Dictionary<string, string> { { "question", shortQuestion } });
        CollectionAssert.AreEqual(_examples, output);
    }

    [Test]
    public void SelectExamples_AddExample_Valid()
    {
        var selector = BuildSelector();
        var newExample = new Dictionary<string, string> { { "question", "Question: what are you?\nAnswer: bar" } };
        selector.AddExample(newExample);

        var shortQuestion = "Short question?";
        var output = selector.SelectExamples(new Dictionary<string, string> { { "question", shortQuestion } });
        var newExamples = _examples.Append(newExample).ToList();
        CollectionAssert.AreEqual(newExamples, output);
    }

    [Test]
    public void SelectExamples_MediumQuestion_TrimsOneExample()
    {
        var selector = BuildSelector();
        var longQuestion = @"I am writing a really long question,
            this probably is going to affect the example right, am I correct?";
        var output = selector.SelectExamples(new Dictionary<string, string> { { "question", longQuestion } });
        Assert.AreEqual(_examples.GetRange(0, 1), output);
    }

    [Test]
    public void SelectExamples_LargeQuestion_TrimsAllExamples()
    {
        var selector = BuildSelector();
        var longestQuestion = @"This question is super super super,
            super super super super super super super super super super super,
            super super super super long, this will affect the example right?";
        var output = selector.SelectExamples(new Dictionary<string, string> { { "question", longestQuestion } });
        Assert.AreEqual(new List<Dictionary<string, string>> { }, output);
    }

    private static IExampleSelector BuildSelector()
    {
        var prompts = new PromptTemplate("{question}", new[] { "question" });
        var selector = new LengthBasedExampleSelector(new List<IDictionary<string, string>>(_examples), prompts)
        {
            MaxLength = 30
        };
        return selector;
    }
}