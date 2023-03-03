using System.Threading.Tasks.Dataflow;
using DotnetPrompt.Abstractions.Chains;
using DotnetPrompt.Chains;
using DotnetPrompt.Prompts;
using DotnetPrompt.Tests.Unit.LargeLanguageModels;
using NUnit.Framework;

namespace DotnetPrompt.Tests.Unit.Chains;

[TestFixture]
public class ModelChainTests
{
    public IChainExecutor FakeLLMChain()
    {
        // Create the PromptTemplate
        var inputVariables = new List<string> { "bar" };
        var template = "This is a {bar}:";
        var prompt = new PromptTemplate(template, inputVariables);

        // Create the FakeLLM and return the LLMChain
        var llm = new FakeLargeLanguageModel();
        var model = new ModelChain(prompt, llm);
        return new OneShotChainExecutor(model);
    }

    [Test]
    public void TestMissingInputs()
    {
        var oneTimeModelExecutor = FakeLLMChain();

        // Act and Assert
        Assert.ThrowsAsync<ArgumentException>(() => oneTimeModelExecutor.PromptAsync(new Dictionary<string, string> { { "foo", "bar" } }));
    }

    [Test]
    public async Task TestValidCall()
    {
        var oneTimeModelExecutor = FakeLLMChain();

        // Test with valid input.
        var output = await oneTimeModelExecutor.PromptAsync(new Dictionary<string, string> { { "bar", "baz" } });
        CollectionAssert.AreEqual(new Dictionary<string, string> { { "bar", "baz" }, { "text", "foo" } }, output);

        // Test with stop words.
        output = await oneTimeModelExecutor.PromptAsync(new Dictionary<string, string> { { "bar", "baz" } },
            new List<string> { "foo" });

        // Response should be `bar` now.
        CollectionAssert.AreEqual(new Dictionary<string, string> { { "bar", "baz" }, { "text", "bar" } }, output);
    }

    [Test]
    public async Task PromptAsync_SingleStringInput_ValidSingleStringOutput()
    {
        var oneTimeModelExecutor = FakeLLMChain();

        // Test with valid input.
        var output = await oneTimeModelExecutor.PromptAsync("baz");
        Assert.AreEqual("foo", output);
    }

    [Test]
    public async Task TestPredictAndParse()
    {
        // Create the PromptTemplate
        var template = "{foo}";
        //var outputParser = new FakeOutputParser(); todo parsers
        var prompt = new PromptTemplate(template/* outputParser*/);

        // Create the FakeLLM and LLMChain
        var queries = new Dictionary<string, string> { { "foo", "foo bar" } };
        var llm = new FakeLargeLanguageModel(queries);
        var chain = new ModelChain(prompt, llm);
        var oneTimeModelExecutor = new OneShotChainExecutor(chain);

        // Test predict and parse method
        var output = await oneTimeModelExecutor.PromptAsync(new Dictionary<string, string> { { "foo", "foo" } });
        CollectionAssert.AreEqual(new List<string> { "foo", "foo bar" }, output.Values);
    }

    [Test]
    public async Task TestConnectors()
    {
        // Create the PromptTemplate
        var template = "{foo}";
        //var outputParser = new FakeOutputParser(); todo parsers
        var prompt = new PromptTemplate(template/* outputParser*/);

        // Create the FakeLLM and LLMChain
        var queries = new Dictionary<string, string> { { "foo", "foo bar" } };
        var llm = new FakeLargeLanguageModel(queries);
        var chain = new ModelChain(prompt, llm);

        bool passed = false;
        var bufferBlock = new BufferBlock<ChainMessage>();
        var actionBlock = new ActionBlock<ChainMessage>(context => passed = true);

        bufferBlock.LinkTo(chain.InputBlock, new DataflowLinkOptions() { PropagateCompletion = true });
        chain.OutputBlock.LinkTo(actionBlock, new DataflowLinkOptions() { PropagateCompletion = true });

        bufferBlock.Post(new ChainMessage(new Dictionary<string, string> { { "foo", "foo" } }));
        bufferBlock.Complete();

        await actionBlock.Completion;
        Assert.IsTrue(passed);
    }

    [Test]
    public async Task TestConnectors_LinkTwoChains_ProperResult()
    {
        // Arrange
        var queries = new Dictionary<string, string>
        {
            { "I send foo", "bar" }, // stub result of first fake model
            { "I send bar and test", "You send bar and test" } // stub result of second fake model
        };
        var llm = new FakeLargeLanguageModel(queries);

        // Create the PromptTemplate
        var prompt1 = new PromptTemplate("I send {foo}");
        var chain1 = new ModelChain(prompt1, llm)
        {
            DefaultOutputKey = "bar"
        };

        var prompt2 = new PromptTemplate("I send {bar} and {test}");
        var chain2 = new ModelChain(prompt2, llm);

        chain1.LinkTo(chain2);

        ChainMessage actual = null;
        var actionBlock = new ActionBlock<ChainMessage>(context => actual = context);

        chain2.OutputBlock.LinkTo(actionBlock, new DataflowLinkOptions() { PropagateCompletion = true });

        var inputValues = new Dictionary<string, string>
        {
            { "foo", "foo" },
            { "test", "test"},
        };
        var context = new ChainMessage(inputValues);
        
        // Act
        chain1.InputBlock.Post(context);
        chain1.InputBlock.Complete();
        await actionBlock.Completion;

        // Assert
        CollectionAssert.AreEqual(
            new Dictionary<string,string>()
            {
                { "foo", "foo" }, // input 1
                { "test", "test"}, // input 2
                { "bar", "bar" }, // result 1
                { "text", "You send bar and test" } // result 2
            },
            actual.Values);
    }
}