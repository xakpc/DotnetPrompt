using System.Threading.Tasks.Dataflow;
using DotnetPrompt.Abstractions.Chains;

namespace DotnetPrompt.Tests.Unit.Chains;

public class FakeChain : IChain
{
    private readonly TransformBlock<ChainMessage, ChainMessage> _transformationBlock;

    public FakeChain(Func<ChainMessage, ChainMessage> func)
    {
        _transformationBlock = new TransformBlock<ChainMessage, ChainMessage>(func, new ExecutionDataflowBlockOptions()
        {
            CancellationToken = _cancellationTokenSource.Token
        });
    }

    public ITargetBlock<ChainMessage> InputBlock => _transformationBlock;
    public ISourceBlock<ChainMessage> OutputBlock => _transformationBlock;
    public void Cancel()
    {
        _cancellationTokenSource.Cancel();
    }

    public IList<string> InputVariables { get; } = new List<string>() { "input" };
    public string DefaultOutputKey { get; set; } = "output";

    private readonly CancellationTokenSource _cancellationTokenSource = new(TimeSpan.FromMinutes(1));
}