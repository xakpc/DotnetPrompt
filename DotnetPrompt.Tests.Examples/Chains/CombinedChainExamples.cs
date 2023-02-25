using System.Threading.Tasks.Dataflow;
using DotnetPrompt.Abstractions.Chains;
using DotnetPrompt.Chains;
using DotnetPrompt.LLM.OpenAI;
using DotnetPrompt.Prompts;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace DotnetPrompt.Tests.Examples.Chains;

public class CombinedChainExamples
{
    [Test]
    public async Task SequentialChain_Example()
    {
        #region SequentialChain_Example
        var logger = TestLogger.Create<ModelChain>(minLevel: LogLevel.Information);
        var llm = new OpenAIModel(Constants.OpenAIKey, OpenAIModelConfiguration.Default with { Temperature = 0.9f });
        
        // Setup model to create a company name for a product
        var prompt = new PromptTemplate("What is a good name for a company that makes {product}?", new[] { "product" });
        var chain = new ModelChain(prompt, llm, logger);

        // Setup model to create a catchphrase for the product
        var secondPrompt = new PromptTemplate("Write a catchphrase for the following company: {company_name}", new[] { "company_name" });
        var chainTwo = new ModelChain(secondPrompt, llm, logger);

        // Combine the two chains, so that we can create a company name and a catchphrase in a single step.
        var overallChain = new SequentialChain(new[] { chain, chainTwo });

        // Run the chain specifying only the input variable for the first chain.
        var executor = overallChain.GetExecutor();
        var catchphrase = await executor.PromptAsync("colorful socks");

        Console.WriteLine(catchphrase);
        #endregion
    }

    //public async Task CombinedChain_Example()
    //{
    //    //example https://langchain.readthedocs.io/en/latest/modules/chains/generic/sequential_chains.html#sequential-chain

    //    #region SequentialChain_Example
    //    // todo: invent
    //    var logger = TestLogger.Create<ModelChain>(minLevel: LogLevel.Information);
    //    var llm = new OpenAIModel(Constants.OpenAIKey, OpenAIModelConfiguration.Default with { Temperature = 0.9f });

    //    // Setup model to create a company name for a product
    //    var prompt = new PromptTemplate("What is a good name for a company that makes {product}?", new[] { "product" });
    //    var chain = new ModelChain(prompt, llm, logger);

    //    // Setup model to create a catchphrase for the product
    //    var secondPrompt = new PromptTemplate("Write a catchphrase for the following company: {company_name}", new[] { "company_name" });
    //    var chainTwo = new ModelChain(secondPrompt, llm, logger);

    //    // Combine the two chains, so that we can create a company name and a catchphrase in a single step.
    //    //var overallChain = new CombinedChain((IChain chain, IChain chainTwo) =>
    //    //{
    //    //    //chain.DefaultOutputKey = "tt";
    //    //    //chainTwo.InputVariables = new List<string>() { "tt" };

    //    //    //chain.LinkTo(chainTwo);
    //    //});

    //    // Run the chain specifying only the input variable for the first chain.
    //    var executor = overallChain.GetExecutor();
    //    var catchphrase = await executor.PromptAsync("colorful socks");

    //    Console.WriteLine(catchphrase);
    //    #endregion
    //}

    [Test]
    public async Task CustomChain_Example()
    {
        #region SequentialChain_Example
        var logger = TestLogger.Create<ModelChain>(minLevel: LogLevel.Information);
        var llm = new OpenAIModel(Constants.OpenAIKey, OpenAIModelConfiguration.Default with { Temperature = 0.9f });

        var prompt1 = new PromptTemplate("What is a good name for a company that makes {product}?", new[] {"product"});
        var chain1 = new ModelChain(prompt1, llm, TestLogger.Create<ModelChain>());

        var prompt2 = new PromptTemplate("What is a good slogan for a company that makes {product}?", new[] {"product"});
        var chain2 = new ModelChain(prompt2, llm, TestLogger.Create<ModelChain>());

        var concatChain = new ConcatenateChain(chain1, chain2);

        var executor = concatChain.GetExecutor();
        var concatOutput = await executor.PromptAsync("colorful socks");
        Console.WriteLine("Concatenated output:\n{concat_output}");
        #endregion
    }

    public class ConcatenateChain : IChain
    {
        private readonly BroadcastBlock<ModelChainContext> _broadcast;
        private readonly TransformBlock<Tuple<ModelChainContext, ModelChainContext>, ModelChainContext> _finalTransformation;

        public ITargetBlock<ModelChainContext> InputBlock => _broadcast;
        public ISourceBlock<ModelChainContext> OutputBlock => _finalTransformation;

        public ConcatenateChain(IChain one, IChain two)
        {
            InputVariables = one.InputVariables;

            _broadcast = new BroadcastBlock<ModelChainContext>(e => e with {}); // clone input record
            var joinBlock = new JoinBlock<ModelChainContext, ModelChainContext>();

            _broadcast.LinkTo(one.InputBlock);
            _broadcast.LinkTo(two.InputBlock);

            one.OutputBlock.LinkTo(joinBlock.Target1, new DataflowLinkOptions() { PropagateCompletion = true }); // we PropagateCompletion so exceptions inside of the chain goes forward
            two.OutputBlock.LinkTo(joinBlock.Target2, new DataflowLinkOptions() { PropagateCompletion = true }); 

            _finalTransformation =
                new TransformBlock<Tuple<ModelChainContext, ModelChainContext>, ModelChainContext>(list =>
                {
                    var resultOne = list.Item1.Values[one.DefaultOutputKey];
                    var resultTwo = list.Item2.Values[two.DefaultOutputKey];

                    var resultDictionary = new Dictionary<string, string>
                    {
                        { DefaultOutputKey, string.Concat(resultOne, "\n", resultTwo) }
                    };
                    return new ModelChainContext(resultDictionary);
                });

            joinBlock.LinkTo(_finalTransformation, new DataflowLinkOptions() { PropagateCompletion = true }); 
        }

        public IDisposable LinkTo(IChain target, ChainLinkOptions linkOptions)
        {
            throw new NotImplementedException();
        }

        public IList<string> InputVariables { get; }
        public string DefaultOutputKey { get; set; } = "text";
    }

}