# Key Concepts

## Large Language Models

DotnetPrompt offers a layer, or multiple layers of abstraction over Large Language Models (LLMs), with a specific emphasis on their "generate" capability. 
The central feature of our model abstraction is the `GenerateAsync` method, which generates an `ModelResult` that includes the outputs for each string in the input list.

Each LLM is an implementation of `ILargeLanguageModel` and inherits from `BaseModel`, which provides caching and resilience.

```charp
public interface ILargeLanguageModel
{
    /// <summary>
    /// Run the LLM on the given prompts and stops.
    /// </summary>
    /// <param name="prompts"></param>
    /// <param name="stop"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">When cache asked without </exception>
    Task<ModelResult> GenerateAsync(IList<string> prompts, IList<string> stop = null);

    /// <summary>
    /// Keyword for model type, used for serialization
    /// </summary>
    string LLMType { get; }
}
```

## ModelResult

The full output of a call to the `GenerateAsync` method of the model class. Since the `GenerateAsync` method takes as input a list of strings, 
this returns a list of results. Each result consists of a list of generations (since you could request N generations per input string). 
This also contains an `Output` attribute which contains provider-specific information about the call.

```csharp
/// <summary>
/// Class that contains all relevant information for an LLM Result.
/// </summary>
public record ModelResult
{
    /// <summary>
    /// List of the things generated.
    /// This is List[List[]] because each input could have multiple generations/generated completions.
    /// </summary>
    public IList<IList<Generation>> Generations { get; set; }

    /// <summary>
    /// For arbitrary LLM provider specific output.
    /// </summary>
    public IDictionary<string, object> Output { get; set; }
}
```

## Caching (WIP)

Usually, there is a cost associated with calling a model. 
However, if you need to call the model multiple times with the same input, you can opt for the caching option to save money.

Constructor ov every model has a `IDistributedCache` option and `UseCache` property. 
```csharp
BaseModel(ILogger logger, IDistributedCache cache)
```
By setting both - cache to `MemoryCahche` for example and property to true you will enable cache that will produce the same result for the same input and model configuration.

## Resilience (WIP)

We plan to use [Polly](https://github.com/App-vNext/Polly) to ensure that requests to LLM always completed sucessfully.