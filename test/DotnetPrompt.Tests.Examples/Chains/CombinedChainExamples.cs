using System.Threading.Tasks.Dataflow;
using DotnetPrompt.Abstractions.Chains;
using DotnetPrompt.Chains;
using DotnetPrompt.Chains.Specialized;
using DotnetPrompt.LLM.OpenAI;
using DotnetPrompt.Prompts;
using DotnetPrompt.Tests.Integration;
using NUnit.Framework;

namespace DotnetPrompt.Tests.Examples.Chains;

public class CombinedChainExamples
{
    [Test]
    public async Task SequentialChain_Example()
    {
        #region SequentialChain_Example
        var llm = new OpenAIModel(Constants.OpenAIKey, OpenAIModelConfiguration.Default with { Temperature = 0.9f });
        
        // Setup model to create a company name for a product
        var prompt = new PromptTemplate("What is a good name for a company that makes {product}?", new[] { "product" });
        var chain = new ModelChain(prompt, llm)
        {
            DefaultOutputKey = "company_name"
        };

        // Setup model to create a catchphrase for the product
        var secondPrompt = new PromptTemplate("Write a catchphrase for the following company: {company_name}", new[] { "company_name" });
        var chainTwo = new ModelChain(secondPrompt, llm);

        // Combine the two chains, so that we can create a company name and a catchphrase in a single step.
        var overallChain = new SequentialChain(new[] { chain, chainTwo });

        // Run the chain specifying only the input variable for the first chain.
        var executor = overallChain.GetExecutor();
        var catchphrase = await executor.PromptAsync("colorful socks");

        Console.WriteLine(catchphrase);
        #endregion
    }

    [Test]
    public async Task CustomChain_Example()
    {
        #region CustomChain_Example
        var llm = new OpenAIModel(Constants.OpenAIKey, OpenAIModelConfiguration.Default with { Temperature = 0.9f });

        var prompt1 = new PromptTemplate("What is a good name for a company that makes {product}?", new[] {"product"});
        var chain1 = new ModelChain(prompt1, llm, TestLogger.Create<ModelChain>())
        {
            DefaultOutputKey = "CompanyName"
        };

        var prompt2 = new PromptTemplate("What is a good slogan for a company that makes {product}?", new[] {"product"});
        var chain2 = new ModelChain(prompt2, llm, TestLogger.Create<ModelChain>())
        {
            DefaultOutputKey = "Slogan"
        };

        var concatChain = new ConcatenateChain(chain1, chain2);

        var executor = concatChain.GetExecutor();
        var concatOutput = await executor.PromptAsync("colorful socks");
        Console.WriteLine($"Concatenated output:\n{concatOutput}");
        #endregion
    }

    #region CustomChain_ConcatenateChain
    public class ConcatenateChain : IChain
    {
        private readonly IChain _one;
        private readonly IChain _two;
        private readonly BroadcastBlock<ChainMessage> _broadcast;
        private readonly TransformBlock<Tuple<ChainMessage, ChainMessage>, ChainMessage> _finalTransformation;
        private CancellationTokenSource _cts = new(TimeSpan.FromMinutes(1));


        public ITargetBlock<ChainMessage> InputBlock => _broadcast;
        public ISourceBlock<ChainMessage> OutputBlock => _finalTransformation;
        
        
        public void Cancel()
        {
            _one.Cancel();
            _two.Cancel();
            _cts.Cancel();
        }

        public ConcatenateChain(IChain one, IChain two)
        {
            _one = one;
            _two = two;
            InputVariables = one.InputVariables;

            _broadcast = new BroadcastBlock<ChainMessage>(e => e with {}); // clone input record
            var joinBlock = new JoinBlock<ChainMessage, ChainMessage>();
            var options = new DataflowLinkOptions() { PropagateCompletion = true };

            _broadcast.LinkTo(one.InputBlock, options);
            _broadcast.LinkTo(two.InputBlock, options);

            one.OutputBlock.LinkTo(joinBlock.Target1, options); // we PropagateCompletion so exceptions inside of the chain goes forward
            two.OutputBlock.LinkTo(joinBlock.Target2, options); 

            _finalTransformation =
                new TransformBlock<Tuple<ChainMessage, ChainMessage>, ChainMessage>(list =>
                {
                    var resultOne = list.Item1.Values[one.DefaultOutputKey];
                    var resultTwo = list.Item2.Values[two.DefaultOutputKey];

                    var resultDictionary = new Dictionary<string, string>
                    {
                        { DefaultOutputKey, string.Concat(resultOne, "\n", resultTwo) }
                    };
                    return new ChainMessage(resultDictionary) { Id = list.Item1.Id };
                }, new ExecutionDataflowBlockOptions() { CancellationToken = _cts.Token });

            joinBlock.LinkTo(_finalTransformation, options); 
        }

        public IList<string> InputVariables { get; }
        public string DefaultOutputKey { get; set; } = "text";
    }
    #endregion
}