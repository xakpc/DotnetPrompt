using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using DotnetPrompt.Abstractions.Chains;
using DotnetPrompt.Tools;

namespace DotnetPrompt.Chains.Specialized;

/// <summary>
/// 
/// </summary>
public class MapReduceChain : IChain
{
    /// <inheritdoc />
    public ITargetBlock<ChainMessage> InputBlock => _mapPhaseBlock;

    /// <inheritdoc />
    public ISourceBlock<ChainMessage> OutputBlock => _reduceChain.OutputBlock;

    /// <inheritdoc />
    public IList<string> InputVariables => new[] { TextVariable };

    /// <inheritdoc />
    public string DefaultOutputKey { get; set; }

    private readonly CancellationTokenSource _cancellationTokenSource = new(TimeSpan.FromMinutes(1));

    public int MaxTokens { get; set; } = 1000;

    #region Private Members

    private readonly TransformManyBlock<ChainMessage, ChainMessage> _mapPhaseBlock;
    private readonly TransformBlock<IList<ChainMessage>, ChainMessage> _intermediateBlock;
    private IDisposable _linkedBufferIn;
    private IDisposable _linkedBufferOut;

    private readonly IChain _mapChain;
    private readonly IChain _reduceChain;
    private readonly Func<string, bool>? _fitReduceChain;
    private readonly Func<IEnumerable<string>, IEnumerable<string>>? _sortFunc;
    private readonly Func<IEnumerable<string>, string>? _mergeFunc;

    private readonly Func<string, IEnumerable<string>> _chunkFunc;

    private const string TextVariable = "input_text";

    #endregion

    private string MapChainInput => _mapChain.InputVariables[0];

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mapChain"></param>
    /// <param name="reduceChain"></param>
    /// <param name="chunkFunc"></param>
    /// <param name="mergeFunc"></param>
    /// <param name="sortFunc"></param>
    /// <param name="fitReduceChain"></param>
    public MapReduceChain(IChain mapChain, IChain reduceChain,
        Func<string, IEnumerable<string>>? chunkFunc = default,
        Func<IEnumerable<string>, string>? mergeFunc = default,
        Func<IEnumerable<string>, IEnumerable<string>>? sortFunc = default,
        Func<string, bool>? fitReduceChain = default)
    {
        _mapChain = mapChain;
        _reduceChain = reduceChain;
        _fitReduceChain = fitReduceChain ?? DefaultFitReduceChain;
        _sortFunc = sortFunc ?? DefaultSort;
        _mergeFunc = mergeFunc ?? DefaultMerge;
        _chunkFunc = chunkFunc ?? DefaultChunk;

        DefaultOutputKey = reduceChain.DefaultOutputKey;

        _mapPhaseBlock = new TransformManyBlock<ChainMessage, ChainMessage>(MapPhaseFunction,
            new ExecutionDataflowBlockOptions()
            {
                CancellationToken = _cancellationTokenSource.Token
            });

        _intermediateBlock = new TransformBlock<IList<ChainMessage>, ChainMessage>(messages =>
            {
                // terminate bufferBlock?
                _linkedBufferIn?.Dispose();
                _linkedBufferOut?.Dispose();

                // Shuffle and Sort: The intermediate results are shuffled and sorted by key to group together summaries of the same chunk.
                var totalMessages = messages.SelectMany(p => p.Values).Where(kv => kv.Key == _mapChain.DefaultOutputKey)
                    .Select(kv => kv.Value).ToList();

                var sortedMessages = _sortFunc(totalMessages);

                Trace.TraceInformation("IntermediateBlock Done");

                // Pre Reduce Function: takes each group of summaries and combines them into a final input
                return new ChainMessage(new Dictionary<string, string>
                {
                    {
                        reduceChain.InputVariables[0],
                        _mergeFunc(sortedMessages)
                    }
                })
                {
                    Id = messages[0].Id
                };
            }
            , new ExecutionDataflowBlockOptions()
            {
                CancellationToken = _cancellationTokenSource.Token
            });

        // Link blocks together
        // Map Function: Apply a summarization algorithm to each chunk, which could include methods such as extracting the most important sentences, identifying the main topics, or generating a set of keywords that capture the main ideas.
        _mapPhaseBlock.LinkTo(mapChain.InputBlock, new DataflowLinkOptions { PropagateCompletion = true });

        // Reduce Function: The reducer function takes each group of summaries for the same chunk and combines them into a final summary for that chunk.
        _intermediateBlock.LinkTo(reduceChain.InputBlock, new DataflowLinkOptions() { PropagateCompletion = true },
            message => _fitReduceChain(message.Values[reduceChain.InputVariables[0]]));

        // restart entire process if message still too big 
        // todo could forever stuck here
        _intermediateBlock.LinkTo(_mapPhaseBlock, new DataflowLinkOptions() { PropagateCompletion = true },
            message => !_fitReduceChain(message.Values[reduceChain.InputVariables[0]]));
    }

    /// <inheritdoc />
    public void Cancel()
    {
        _cancellationTokenSource.Cancel();
        _mapChain.Cancel();
        _reduceChain.Cancel();
    }

    private IEnumerable<ChainMessage> MapPhaseFunction(ChainMessage chainMessage)
    {
        // Map Phase: Divide the long text into smaller chunks (e.g., paragraphs, sentences) and assign each chunk to a mapper.
        var text = chainMessage.Values[TextVariable];
        var split = _chunkFunc(text).ToList();

        if (!split.Any())
        {
            Cancel();
            yield break;
        }

        var batchBlock = new BatchBlock<ChainMessage>(split.Count(), new GroupingDataflowBlockOptions()
        {
            CancellationToken = _cancellationTokenSource.Token
        });

        _linkedBufferIn = _mapChain.OutputBlock.LinkTo(batchBlock, new DataflowLinkOptions() { PropagateCompletion = true, MaxMessages = split.Count() });
        _linkedBufferOut = batchBlock.LinkTo(_intermediateBlock, new DataflowLinkOptions() { PropagateCompletion = true, MaxMessages = split.Count() });

        var i = 0;
        foreach (var chunk in split)
        {
            yield return chainMessage with
            {
                Values = new Dictionary<string, string>()
                {
                    { MapChainInput, chunk },
                    { $"pos", $"{i++}" }
                }
            };
        }

        Trace.TraceInformation("MapPhaseFunction Done");
    }

    private IEnumerable<string> DefaultChunk(string text) =>
        StringHelpers.SplitStringIntoChunks(text, MaxTokens * 4); // 1 token ~= 4 chars in English, 500 tokens by default

    private bool DefaultFitReduceChain(string str) => true; // by default we assume that any result would fit reduce chain

    private static IEnumerable<string> DefaultSort(IEnumerable<string> arg) => arg;

    private static string DefaultMerge(IEnumerable<string> items) => string.Join("\n\n", items);
}