using ThreadAbortException = System.Threading.ThreadAbortException;

# Quickstart Guide

The tutorial provides step-by-step instructions for creating a complete language model application using DotnetPrompt.

## Installation

To get started, add NuGet meta-package

```ps
> dotnet add package DotnetPrompt.All --version 1.0.0-alpha.1
```

You could also install separate packages

```ps
> dotnet add package DotnetPrompt --version 1.0.0-alpha.1
> dotnet add package DotnetPrompt.LLM.OpenAI --version 1.0.0-alpha.1
```

DotnetPrompt provides many blocks that can be used to build language model applications. 
Blocks can be combined to create complex applications, or be used individually for simple applications.

## LLMs: Get predictions from a language model

DotnetPrompt's fundamental component is its `ILargeLanguageModel`, 
which serves as a client that calls a language model on a given input.
We provide several out of the box implementations of the interface for different providers.

Let's utilize the `OpenAIModel`, which employs the OpenAI REST API to generate completions based on the input prompt. 

To create the Model, we would need a valid OpenAI key. 
In this example, you may want the outputs to be more diverse so we'll initialize the Model with a high temperature.

```csharp
var llm = new OpenAIModel(Constants.OpenAIKey, OpenAIModelConfiguration.Default with
        {
            Temperature = 0.9f
        });
```

For simplicity we provide `PromptAsync` extension method, 
which take a single string as input and return generated result.

For example, if your friend sent you a message saying `I'm getting so old` you might input `What's a funny response to 'I'm getting so old'?` as the prompt. 

```csharp
var text = "What's a funny response to 'I'm getting so old'?";
var response = await llm.PromptAsync(text);
Console.WriteLine(response);
```

```text
> Me too, but let's not get ahead of ourselves - we're still young at heart!
```

For more details on how to use LLMs within DotnetPrompt, see the [LLM getting started guide](./llms/getting_started.md).

## Prompt Templates: Manage prompts for LLMs

While calling a language model (LLM) is a crucial initial step, it's merely the beginning of the process.
When using an LLM in an application, user input is typically not sent directly to the model. 
Instead, the input is used to construct a prompt, which is then sent to the LLM.

For instance, in the previous example, the text we provided was hardcoded to request a 
response for theoretical friend message. 
In a real-world scenario, we would only take the user 
input, actual message, and utilize that information to format the prompt.


First lets define the prompt template:

```csharp
var oneInputPrompt = new PromptTemplate(template: "What's a funny response to '{message}'", 
    inputVariables: new[] { "message" });
```

Let's now see how this works! We can call the `Format` method to format it.

```csharp
var valuesOneInput = new Dictionary<string, string>
    {
        { "message", "I have some exciting news to share with you!" }
    };

var text = oneInputPrompt.Format(valuesOneInput);
Console.WriteLine(text);
```

```text
> What's a funny response to 'I have some exciting news to share with you!'
```

For more details, [check out the getting started guide for prompts.](./prompts/getting_started.md)

### Few Shot Learning

Few-shot learning is a type of machine learning technique where a model is trained to learn from a small set of examples, 
typically a few dozen or less, and can generalize to new examples with similar characteristics. 

For example, few-shot learning could be used to train a chatbot to understand and 
respond to user queries in a new domain with minimal training data.

By leveraging a few-shot learning model, developers can quickly train a system to perform a specific task with minimal data, reducing the time and cost required to develop a fully-fledged AI system. 

For instance, OpenAI's GPT-3 can be used to perform a wide range of 
[tasks](./prompts/few_shots_example.md), such as [summarization](./prompts/few_shots_example.md#summarization), 
[question answering](./prompts/few_shots_example.md#question-answering), 
and [language translation](./prompts/few_shots_example.md##machine-translation), with only a few examples of each task. 

DotnetPrompt provides a `FewShotPromptTemplate` to ease utilizing Few Shot Learning efficiently and with minimal hassle. 

## Chains: Combine LLMs and prompts in multi-step workflows

In real applications we usually need to do more actions, data transformation or even use several different prompts/models.

In DotnetPrompt, we combine different building blocks to create a chain of actions. 
These building blocks are called chains (even if they constins only one action), and they can be `Models` or other chains.

Each chain has an `Input` and an `Output` property, which can be connected to other chains or blocks.
The most basic type of chain is called a `ModelChain`. It's made up of two parts: a `PromptTemplate` and an LLM.

![Image](../images/ichain.png)

For example, let's say we want to create a `ModelChain` that takes user input, formats it with a `PromptTemplate`, 
and sends it to an LLM. This allows us to generate a response based on the user's input.

```csharp
var llm = new OpenAIModel(Constants.OpenAIKey, OpenAIModelConfiguration.Default with
{
    Temperature = 0.9f
});

var oneInputPrompt = new PromptTemplate(template: "What's a funny response to '{message}'", inputVariables: new[] { "message" });

var valuesOneInput = new Dictionary<string, string>
{
    { "message", "I have some exciting news to share with you!" }
};
```

We can now create a very simple chain that will take user input, format the prompt with it, and then send it to the LLM:

```csharp
var chain = new ModelChain(oneInputPrompt, llm);
var executor = chain.GetExecutor();
```

Now we can run that chain only specifying the product!

```csharp
var result = await executor.PromptAsync("I have some exciting news to share with you!");
Console.WriteLine(result);
```
```text
> I'm all ears! Hit me with it!
```
Or with another input

```csharp
var result2 = await executor.PromptAsync("Want to grab lunch later?");
Console.WriteLine(result2);
```
```text
> Sure! What's on the menu?
```

This is one of the simpler types of chains, but understanding how it works will set you up well for working with 
more complex chains.

For more details, [check out the getting started guide for chains.](./chains/getting_started.md)

### Building dataflows

Chains are based on [TPL.Dataflow](https://learn.microsoft.com/en-us/dotnet/standard/parallel-programming/dataflow-task-parallel-library), a programming library that helps manage multiple tasks that need to communicate 
with each other asynchronously, and it uses a dataflow model to promote actor-based programming.

For simple chaining we provide a special kind of chain called `SequentialChain`. 
It consume several other chains and linking them together by assinging outputs to inputs between chains.

[!code-csharp[](../../DotnetPrompt.Tests.Examples/Chains/CombinedChainExamples.cs#SequentialChain_Example)]

The result

```text
> Bring out your true colors with Soxicolor!
```


## Dependency Injecton

Every block in DotnetPrompt could be created manually using constructor. But in complex applications in might be wise to use Dependency Injection especially if you want to use `ILogger` and `IDistributedCache`.