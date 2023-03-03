using System.Text.RegularExpressions;
using System.Threading.Tasks.Dataflow;
using DotnetPrompt.Abstractions.Chains;
using DotnetPrompt.Chains;
using DotnetPrompt.LLM.OpenAI;
using DotnetPrompt.Prompts;
using DotnetPrompt.Tests.Integration;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace DotnetPrompt.Tests.Examples.UseCases;

public class UseCaseGenerateDocumentation
{
    [Test]
    public async Task Example_ParseCodeFile()
    {
        #region GenerateDocumentation
        var chain = new ConvertDotnetTestsToMdTableChain(TestLogger.Create<ConvertDotnetTestsToMdTableChain>());
        var input = new Dictionary<string, string>()
        {
            {
                "file",
                @"Data\Logger.cs"
            }
        };

        chain.Run(new ChainMessage(input));

        var result = await chain.OutputBlock.ReceiveAsync();
        var resultText = result.Values["table"];

        Console.WriteLine(resultText);
        #endregion
    }


    public class ConvertDotnetTestsToMdTableChain : IChain
    {
        private readonly ILogger _logger;

        #region BuildDataflow
        private readonly TransformBlock<IList<ChainMessage>, ChainMessage> _finalizatorBlock;
        private readonly TransformManyBlock<ChainMessage, ChainMessage> _transformationBlockOne;

        public ConvertDotnetTestsToMdTableChain(ILogger<ConvertDotnetTestsToMdTableChain> logger)
        {
            _logger = logger;

            // "transform" .cs file to list of methods
            _transformationBlockOne = new TransformManyBlock<ChainMessage, ChainMessage>(ReadMethodsFromFile);

            // LLM set up with several examples through few-prompt learning
            var llmModelChain = BuildFewPromptLLModelChain();

            // buffer to collect rows
            var batchRowsBlock = new BatchBlock<ChainMessage>(100);

            // "transform" from list of rows to table
            _finalizatorBlock = new TransformBlock<IList<ChainMessage>, ChainMessage>(CombineRowsToTable);

            var linkOptions = new DataflowLinkOptions() { PropagateCompletion = true };

            // set up chain
            _transformationBlockOne.LinkTo(llmModelChain.InputBlock, linkOptions);
            llmModelChain.OutputBlock.LinkTo(batchRowsBlock, linkOptions);
            batchRowsBlock.LinkTo(_finalizatorBlock, linkOptions);
        }
        #endregion


        private IEnumerable<ChainMessage> ReadMethodsFromFile(ChainMessage message)
        {
            return ReadClassesFromFile(message.Values[InputVariables[0]]);
        }

        #region CombineRowsToTable
        private ChainMessage CombineRowsToTable(IList<ChainMessage> message)
        {
            _logger.LogInformation("Finalization");

            var result = message.SelectMany(i => i.Values).Where(i => i.Key == "text").Select(i => i.Value);
            var resultText = string.Join('\n', result);
            return new ChainMessage(new Dictionary<string, string>() { { DefaultOutputKey, resultText } })
                { Id = message.First().Id };
        }
        #endregion

        #region RunMethod
        public bool Run(ChainMessage message)
        {
            if (InputBlock.Completion.IsCompleted)
            {
                throw new InvalidOperationException("This chain would not accept any more messages");
            }

            var launched = InputBlock.Post(message);
            InputBlock.Complete();
            return launched;
        }
        #endregion

        #region ReadMethodsFromFile
        private IEnumerable<ChainMessage> ReadClassesFromFile(string arg)
        {
            _logger.LogInformation($"Reading file {arg}");

            var file = File.ReadAllText(arg);

            var regex = new Regex(@"\bpublic\svoid\s([a-zA-Z0-9_]+)\(([^)]*)\)\s*{([^{}]*(?:{[^{}]*}[^{}]*)*)}");

            var methods = regex.Matches(file);

            _logger.LogInformation($"Extracted {methods.Count}");

            var fromFile = new List<ChainMessage>();
            foreach (var match in methods)
            {
                fromFile.Add(new ChainMessage(
                    new Dictionary<string, string>()
                    {
                        { "code", match.ToString() }
                    }));
            }

            return fromFile;
        }
        #endregion

        /// <inheritdoc />
        public IList<string> InputVariables => new List<string>() { "file" };

        /// <inheritdoc />
        public string DefaultOutputKey { get; set; } = "table";

        #region ModelSetup
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
                        "public void GetDocumentCodes(string id, int[] codes)\r\n {\r\n WriteDebug(10001, new { id, codes },\r\n () => $\"Reading document metadata for id {id}.\");\r\n }"
                    },
                    {
                        "row",
                        "| Debug | 10001 | GetDocumentCodes |  Reading document metadata for id {globalId}. |"
                    }
                },
                new Dictionary<string, string>()
                {
                    {
                        "code",
                        "public void CreateMetadataCompleted(Guid id, TimeSpan timeToFilter)\r\n {\r\n WriteInfo(11004, new { id, timeToFilter },\r\n () => $\"Get comparison metadata for id {id} in {timeToFilter:c}\");\r\n }"
                    },
                    {
                        "row",
                        "| Info | 11004 | CreateComparisonMetadataCompleted | Get comparison metadata for id {id} in {timeToFilter:c} |"
                    }
                },
                new Dictionary<string, string>()
                {
                    {
                        "code",
                        "public void IndexClientFailed(Exception exception)\r\n {\r\n WriteError(13001, new { message = exception.Message },\r\n () => $\"Failed to execute index client with error {exception.Message}\", exception);\r\n }"
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
        #endregion

        #region InputOutput

        public ITargetBlock<ChainMessage> InputBlock => _transformationBlockOne;
        public ISourceBlock<ChainMessage> OutputBlock => _finalizatorBlock;

        #endregion

    }
}