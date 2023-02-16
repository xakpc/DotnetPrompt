using DotnetPrompt.LLM.OpenAI.Encoder;
using NUnit.Framework;

namespace DotnetPrompt.Tests.Unit.LLM.OpenAI;

public class TokenizerTests
{
    [TestCase("hello world", new[] { 31373, 995 })]
    [TestCase("The GPT family of models process text using tokens", new[] { 464, 402, 11571, 1641, 286, 4981, 1429, 2420, 1262, 16326 })]
    [TestCase("The models understand the statistical relationships between these tokens", new[] { 464, 4981, 1833, 262, 13905, 6958, 1022, 777, 16326 })]
    public void Gpt3Tokenizer_Encode_EqualToGpt2Tiktoken(string input, int[] expected)
    {
        var result = Gpt3Tokenizer.Encode(input);
        CollectionAssert.AreEqual(result, expected);
    }
}