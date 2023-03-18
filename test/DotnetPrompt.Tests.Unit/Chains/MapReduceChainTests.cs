using DotnetPrompt.Chains;
using DotnetPrompt.Chains.Specialized;
using DotnetPrompt.Tools;
using NUnit.Framework;

namespace DotnetPrompt.Tests.Unit.Chains
{
    [TestFixture]
    internal class MapReduceChainTests
    {
        [Test]
        public async Task MapReduceChainTest_WithEmptyString_EmptyResults()
        {
            var mapChain = new FakeChain(message =>
            {
                message.Values["output"] = "test output";
                return message;
            });

            var reduceChain = new FakeChain(message =>
            {
                message.Values["output"] = "test output 2";
                return message;
            });

            var chain = new MapReduceChain(mapChain, reduceChain)
            {
                MaxTokens = 10
            };

            // act
            var executor = chain.GetExecutor();

            var result = await executor.PromptAsync("");

            Assert.AreEqual("", result);
        }

        [Test]
        public async Task MapReduceChainTest_WithShortString_SingleChunk()
        {
            
            var intermediate = new List<string>();

            var mapChain = new FakeChain(message =>
            {
                message.Values["output"] = message.Values["input"];

                intermediate.Add(message.Values["input"]);

                return message;
            });

            var reduceChain = new FakeChain(message =>
            {
                message.Values["output"] = message.Values["input"];
                return message;
            });

            var chain = new MapReduceChain(mapChain, reduceChain)
            {
                MaxTokens = 10
            };

            var input = "This is a short string.";
            var executor = chain.GetExecutor();
            
            // act
            var result = await executor.PromptAsync(input);

            // assert
            CollectionAssert.Contains(intermediate, input);
            Assert.AreEqual(input, result);
        }

        [Test]
        public async Task MapReduceChainTest_WithLongString_SingleChunk()
        {
            var intermediate = new List<string>();

            var mapChain = new FakeChain(message =>
            {
                message.Values["output"] = message.Values["input"];

                intermediate.Add(message.Values["input"]);

                return message;
            });

            var reduceChain = new FakeChain(message =>
            {
                message.Values["output"] = message.Values["input"];
                return message;
            });

            var chain = new MapReduceChain(mapChain, reduceChain, fitReduceChain: s => true)
            {
                MaxTokens = 8
            };

            var input = "This is a longer string. It has more words and needs to be split into several chunks. The chunks should end on a punctuation mark or a new line.";
            var expectedChunks = new List<string> { "This is a longer string.", "It has more words and needs to be split into several chunks.",
                "The chunks should end on a punctuation mark or a new line." };

            var executor = chain.GetExecutor();

            // act
            var result = await executor.PromptAsync(input);

            // assert
            CollectionAssert.AreEqual(expectedChunks, intermediate);
            Assert.AreEqual(string.Join("\n\n", expectedChunks), result);
        }

        [Test]
        public async Task MapReduceChainTest_WithConsecutiveSpacesAndNewLines_SeveralChunks()
        {
            var intermediate = new List<string>();

            var mapChain = new FakeChain(message =>
            {
                message.Values["output"] = message.Values["input"];

                intermediate.Add(message.Values["input"]);

                return message;
            });

            var reduceChain = new FakeChain(message =>
            {
                message.Values["output"] = message.Values["input"];
                return message;
            });

            var chain = new MapReduceChain(mapChain, reduceChain, fitReduceChain: s => true)
            {
                MaxTokens = 3
            };

            // Arrange
            var input = "This is\na    string\n  with multiple\n   spaces.";
            var expectedChunks = new List<string> { "This is", "a    string", "with multiple", "spaces." };

            var executor = chain.GetExecutor();

            // act
            var result = await executor.PromptAsync(input);

            // assert
            CollectionAssert.AreEqual(expectedChunks, intermediate);
            Assert.AreEqual(string.Join("\n\n", expectedChunks), result);
        }
    }
}
