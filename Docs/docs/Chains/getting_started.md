# Getting Started

Using an LLM in isolation is fine for some simple applications, but many more complex ones require chaining LLMs - 
either with each other or with other components. 
DotnetPrompt provides a standard interface `IChains`, as well as some common implementations of chains for ease of use.

## Why do we need chains?

Chaining multiple LLM runs together (with the output of one step being the input to the next) can help users accomplish more complex tasks, 
and in a way that is perceived to be more transparent and controllable. 

## Query an LLM with the ModelChain

The `ModelChain` is a simple chain that takes in a prompt template, formats it with the user input and returns the response from an LLM.

To use the `ModelChain`, first create a prompt template.

[!code-csharp[](../../../DotnetPrompt.Tests.Examples/Chains/ChainsGettingStartedExamples.cs#Example_QueryLLM_Prompt)]

We can now create a very simple chain. To run the chain and get a result back we could use extension method `PromptAsync`.

[!code-csharp[](../../../DotnetPrompt.Tests.Examples/Chains/ChainsGettingStartedExamples.cs#Example_QueryLLM_Model)]

```text
> CheeryToes Sockery.
```

This is one of the simpler types of chains, but understanding how it works will set you up well for working with more complex chains.
More example how to use `ModelChain` could be found [here](./howto/model_chain.md).

## Creating sequential chains

The next step after calling a language model is make a series of calls to a language model. We can do this using sequential chains, 
which are chains that execute their links in a predefined order. Specifically, we will use the `SequentialChain`. 
This is the simplest form of sequential chains, where each step has a singular input/output, and the output of one step is the input to the next.

[!code-csharp[](../../../DotnetPrompt.Tests.Examples/Chains/CombinedChainExamples.cs#SequentialChain_Example)]

The result could look like this

```text
> Bring out your true colors with Soxicolor!
```

## Create a one-step custom chain with the `ModelChain` class

DotnetPrompt provides set of specialized chains out of the box, 
but sometimes you may want to create a custom chains for your specific use case. 

The simplest way to create your own chain is to inherit `ModelChain` and provide custom prompt template. 
This is useful to do when you need a chain which is a part of other, larger chains.

```charp
public class SummarizeChain : ModelChain
{
    private const string template = "Summarize the following text.\r\n\r\n" +
                                    "Text:\r\n" +
                                    "{text}\r\n" +
                                    "---\r\n\r\n" +
                                    "Summary:";

    public override string DefaultOutputKey => "summary";

    public SummarizeChain(ILargeLanguageModel llm, ILogger<ModelChain>? logger = null) : base(new PromptTemplate(template), llm, logger)
    {

    }
}
```

In example above `SummarizeChain` will have _input variables_ same as created prompt and output variables equal to `DefaultOutputKey`.

## Create your own chain

> [!TIP]
> Chains designed as an extension of [TPL.Dataflow](https://learn.microsoft.com/en-us/dotnet/standard/parallel-programming/dataflow-task-parallel-library) blocks.
> If you want to master building your own chains it is recomended to make yourself familiar with it.

You could combine several chains together or link other Dataflow blocks to built a chain for your own purposes. 

Imagine that you want to generate name and slogan for you company. For that task we could write a specialized chain that
take two other chains and broadcast a single input to both of them and combine result afterwards.

Our custom chain would implement `IChain` interface. In constructor of the chain we would get two chains and link them in a dataflow. 

```csharp
    public ConcatenateChain(IChain one, IChain two)
    {
        InputVariables = one.InputVariables;

        _broadcast = new BroadcastBlock<ChainMessage>(e => e with {}); 
        var joinBlock = new JoinBlock<ChainMessage, ChainMessage>();

        _broadcast.LinkTo(one.InputBlock);
        _broadcast.LinkTo(two.InputBlock);

        // we PropagateCompletion so exceptions inside of the chain goes forward
        one.OutputBlock.LinkTo(joinBlock.Target1, new DataflowLinkOptions() { PropagateCompletion = true }); 
        two.OutputBlock.LinkTo(joinBlock.Target2, new DataflowLinkOptions() { PropagateCompletion = true }); 
```

Note that we link our chains' `InputBlock` to interanl `BroadcastBlock`. It will broadcast our propmt as `ChainMessage` to each chain. 

We also create a `JoinBlock` to join results of our models.

The final step is to combine results in a single result string. The result would be provided through `OutputBlock` inside `ChainMessage.Values` with `DefaultOutputKey`

```csharp

        _finalTransformation =
            new TransformBlock<Tuple<ChainMessage, ChainMessage>, ChainMessage>(list =>
            {
                var resultOne = list.Item1.Values[one.DefaultOutputKey];
                var resultTwo = list.Item2.Values[two.DefaultOutputKey];

                var resultDictionary = new Dictionary<string, string>
                {
                    { DefaultOutputKey, string.Concat(resultOne, "\n", resultTwo) }
                };
                return new ChainMessage(resultDictionary)  { Id = list.Item1.Id };
            });

        joinBlock.LinkTo(_finalTransformation, new DataflowLinkOptions() { PropagateCompletion = true }); 
    }
```

> [!IMPORTANT]
> It is crucial to set the `Id` of the final message to be the same as the `Id` of the input message: `return new ChainMessage(resultDictionary) { Id = list.Item1.Id };`
> This is because the executor expects to receive a message with the same `Id`, and if it's missed, it will not be received. 

The full class would look like this

[!code-csharp[](../../../DotnetPrompt.Tests.Examples/Chains/CombinedChainExamples.cs#CustomChain_ConcatenateChain)]

Then to use it we will create a couple `ModelChain` and provide them to constructor of `ConcatenateChain`.

[!code-csharp[](../../../DotnetPrompt.Tests.Examples/Chains/CombinedChainExamples.cs#CustomChain_Example)]

When we execute this code, the result could look like this:

```text
> Concatenated output:

Rainbow Steps Socks

"Step Into Color with Our Socks!"
```

One top of this both calls to LLM was done in parallel, which is one of the benefits of using Dataflow.