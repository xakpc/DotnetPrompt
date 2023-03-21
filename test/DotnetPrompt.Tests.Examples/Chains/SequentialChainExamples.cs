using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using DotnetPrompt.Abstractions.Chains;
using DotnetPrompt.Chains;
using DotnetPrompt.Chains.Specialized;
using DotnetPrompt.LLM.OpenAI;
using DotnetPrompt.Prompts;
using DotnetPrompt.Tests.Integration;

namespace DotnetPrompt.Tests.Examples.Chains
{
    internal class SequentialChainExamples
    {
        [Test]
        public async Task Example_SummarizeChain()
        {
            var openAiModel = new OpenAIModel(Constants.OpenAIKey, OpenAIModelConfiguration.Default);

            var rateCommentsPrompt = new PromptTemplate("Generate 5 positive comments about {comments}");

            var rateComments = new ModelChain(rateCommentsPrompt, openAiModel, TestLogger.Create<ModelChain>());
            var summarizeComments = new SummarizeChain(openAiModel, TestLogger.Create<SummarizeChain>());
            var sq = new SequentialChain(new[] { rateComments, summarizeComments })
            {
                DefaultOutputKey = summarizeComments.DefaultOutputKey
            };

            (sq as IChain).Run(new ChainMessage(new Dictionary<string, string>()
                {
                    {"comments","iphone"}
                }));

            var result = await sq.OutputBlock.ReceiveAsync();
        }
    }
}
