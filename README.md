## Quick Start

Install a NuGet metapackage

To get started, add NuGet meta-package

```ps
> dotnet add package DotnetPrompt.All --version 1.0.0-alpha.1
```

Let's utilize the `ChatGptModel`, which employs the OpenAI ChatGPT API to generate completions based on the input prompt. 

To create the Model, we would need a valid OpenAI key. 
In this example, you may want the outputs to be more diverse so we'll initialize the Model with a high temperature.

```csharp
var llm = new ChatGptModel(Constants.OpenAIKey, ChatGptModelConfiguration.Default with
        {
            Temperature = 0.9f
        });
```

For simplicity we provide `PromptAsync` extension method, which take a single string as input and return generated result.

For example, if your friend sent you a message saying `I'm getting so old` you might input `What's a funny response to 'I'm getting so old'?` as the prompt. 

```csharp
var text = "What's a funny response to 'I'm getting so old'?";
var response = await llm.PromptAsync(text);
Console.WriteLine(response);
```

The result could be

```text
> Me too, but let's not get ahead of ourselves - we're still young at heart!
```

## What is this?

Welcome to our library, which is designed to support the development of cutting-edge applications powered by Large Language Models (LLMs) in dotnet.

As you may know, LLMs are an exciting and rapidly-evolving technology that offers developers unprecedented natural language processing and generation capabilities. 
However, LLMs can achieve their full potential when used in conjunction with other sources of computation or knowledge.

Our library helps you integrate LLMs with other tools and resources to create powerful and sophisticated applications. 
Some examples of solutions that you could create using our library include:

- Summarization
- Question Answering
- Code Generation
- Chatbots
- and much more

## 📖 Documentation

Full documentation is availible here

## Important Milestones

Full roadmap availible [here](ROADMAP.md)

[] Porting `tiktoken` and `huggingface` tokenizers to support more models, currently there is a partial support
[] Streaming (using SignalR maybe?)
[] VectorDB support
[] ML.NET Agents/Chains/Integrations
[] More end-to-end examples

## Contributing to DotnetPrompt

We welcome contributions to DotnetPrompt! To ensure that your contributions are effective and easy to incorporate, please follow [these guidelines](CONTRIBUTING.md).
