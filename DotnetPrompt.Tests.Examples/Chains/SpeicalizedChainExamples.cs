using System.Text.RegularExpressions;
using DotnetPrompt.Abstractions.Chains;
using DotnetPrompt.Chains;
using DotnetPrompt.Chains.Specialized;
using DotnetPrompt.LLM.OpenAI;
using DotnetPrompt.Prompts;
using NUnit.Framework;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DotnetPrompt.Tests.Examples.Chains;

public class SpeicalizedChainExamples
{
    [Test]
    public async Task Example_SummarizeChain()
    {
        var llmChain = new SummarizeChain(new OpenAIModel(Constants.OpenAIKey, OpenAIModelConfiguration.Default));
        var executor = llmChain.GetExecutor();

        var text =
            "Two independent experiments reported their results this morning at CERN, Europe's high-energy physics laboratory near Geneva in Switzerland. Both show convincing evidence of a new boson particle weighing around 125 gigaelectronvolts, which so far fits predictions of the Higgs previously made by theoretical physicists.\r\n\r\n\"As a layman I would say: 'I think we have it'. Would you agree?\" Rolf-Dieter Heuer, CERN's director-general, asked the packed auditorium. The physicists assembled there burst into applause.";

        var answer = await executor.PromptAsync(text);

        Console.WriteLine(answer);
    }

    [Test]
    public async Task Example_ParseCodeFile()
    {
        var chain = new ConvertDotnetTestsToMdTableChain(NullLogger<ConvertDotnetTestsToMdTableChain>.Instance);
        var input = new Dictionary<string, string>()
        {
            {
                "file",
                @"C:\Users\xakpc\Downloads\ServiceEventSource.cs"
            }
        };

        chain.Run(new ModelChainContext(input));

        var result = await chain.OutputBlock.ReceiveAsync();
        var resultText = result.Values["table"];

        Console.WriteLine(resultText);
    }


    public class ConvertDotnetTestsToMdTableChain : IChain
    {
        private readonly ILogger _logger;
        private readonly TransformBlock<IList<ModelChainContext>, ModelChainContext> _finalizatorBlock;
        private readonly TransformManyBlock<ModelChainContext, ModelChainContext> _transformationBlockOne;

        /// <summary>
        /// List of inner chains of SequentialChain
        /// </summary>
        public IReadOnlyList<IChain> Chains { get; }

        public ConvertDotnetTestsToMdTableChain(ILogger<ConvertDotnetTestsToMdTableChain> logger)
        {
            _logger = logger;

            // "transform" .cs file to list of methods
            _transformationBlockOne = new TransformManyBlock<ModelChainContext, ModelChainContext>(ReadMethodsFromFile);

            // LLM set up with several examples through few-prompt learning
            var llmModelChain = BuildFewPromptLLModelChain();

            // buffer to collect rows
            var batchRowsBlock = new BatchBlock<ModelChainContext>(100);

            // "transform" from list of rows to table
            _finalizatorBlock = new TransformBlock<IList<ModelChainContext>, ModelChainContext>(CombineRowsToTable);

            var linkOptions = new DataflowLinkOptions() { PropagateCompletion = true };

            // set up chain
            _transformationBlockOne.LinkTo(llmModelChain.InputBlock, linkOptions);
            llmModelChain.OutputBlock.LinkTo(batchRowsBlock, linkOptions);
            batchRowsBlock.LinkTo(_finalizatorBlock, linkOptions);
        }

        private IEnumerable<ModelChainContext> ReadMethodsFromFile(ModelChainContext message)
        {
            return ReadClassesFromFile(message.Values[InputVariables[0]]);
        }

        private ModelChainContext CombineRowsToTable(IList<ModelChainContext> message)
        {
            _logger.LogInformation("Finalization");

            var result = message.SelectMany(i => i.Values).Where(i => i.Key == "text").Select(i => i.Value);
            var resultText = string.Join('\n', result);
            return new ModelChainContext(new Dictionary<string, string>() { { DefaultOutputKey, resultText } })
                { Id = message.First().Id };
        }

        public bool Run(ModelChainContext message)
        {
            if (InputBlock.Completion.IsCompleted)
            {
                throw new InvalidOperationException("This chain would not accept any more messages");
            }

            var laucnhed = InputBlock.Post(message);
            InputBlock.Complete();
            return laucnhed;
        }

        private IList<ModelChainContext> ReadClassesFromFile(string arg)
        {
            _logger.LogInformation($"Reading file {arg}");

            var file = File.ReadAllText(arg);

            var regex = new Regex(@"\bpublic\svoid\s([a-zA-Z0-9_]+)\(([^)]*)\)\s*{([^{}]*(?:{[^{}]*}[^{}]*)*)}");

            var methods = regex.Matches(file);

            _logger.LogInformation($"Extracted {methods.Count}");

            var fromFile = new List<ModelChainContext>();
            foreach (var match in methods)
            {
                fromFile.Add(new ModelChainContext(
                    new Dictionary<string, string>()
                    {
                        { "code", match.ToString() }
                    }));
            }

            return fromFile;
        }

        /// <inheritdoc />
        public IList<string> InputVariables => new List<string>() { "file" };

        /// <inheritdoc />
        public string DefaultOutputKey { get; set; } = "table";

// Model setup
        private ModelChain BuildFewPromptLLModelChain()
        {
            var example = new PromptTemplate("Code:\n{code}\nTableRow: {row}");
            var suffix = new PromptTemplate("Code:\n{code}\nTableRow: ");

            var examples = new List<IDictionary<string, string>>()
            {
                new Dictionary<string, string>()
                {
                    {
                        "code",
                        "public void GetDocumentMetadataRequested(string globalId, int[] prefLanguagesEwbCodes)\r\n        {\r\n            WriteDebug(10001, new { globalId, prefLanguagesEwbCodes },\r\n                () => $\"Reading document metadata for id {globalId}.\");\r\n        }"
                    },
                    {
                        "row",
                        "| Debug | 10001 | GetDocumentMetadataRequested |  Reading document metadata for id {globalId}. |"
                    }
                },
                new Dictionary<string, string>()
                {
                    {
                        "code",
                        "public void CreateComparisonMetadataCompleted(Guid comparisonId, TimeSpan timeToFilter)\r\n        {\r\n            WriteInfo(11004, new { comparisonId, timeToFilter },\r\n                () => $\"Get comparison metadata for id {comparisonId} in {timeToFilter:c}\");\r\n        }"
                    },
                    {
                        "row",
                        "| Info | 11004 | CreateComparisonMetadataCompleted | Get comparison metadata for id {comparisonId} in {timeToFilter:c} |"
                    }
                },
                new Dictionary<string, string>()
                {
                    {
                        "code",
                        "public void IndexClientFailed(Exception exception)\r\n        {\r\n            WriteError(13001, new { message = exception.Message },\r\n                () => $\"Failed to execute index client with error {exception.Message}\", exception);\r\n        }"
                    },
                    {
                        "row",
                        "| Error | 13001 | IndexClientFailed | Failed to execute index client with error {exception.Message} |"
                    }
                },
            };

            var prompt = new FewShotPromptTemplate(example, suffix, examples)
            {
                ExampleSeparator = "---"
            };

            var model = new ModelChain(prompt, new OpenAIModel(Constants.OpenAIKey, OpenAIModelConfiguration.Default),
                _logger);
            return model;
        }

        public ITargetBlock<ModelChainContext> InputBlock => _transformationBlockOne;
        public ISourceBlock<ModelChainContext> OutputBlock => _finalizatorBlock;
    }
}