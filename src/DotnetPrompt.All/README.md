﻿## Quick Start

DotnetPrompt is a dotnet library that provides tools for working with Large Language Models (LLMs) individually and combined in chains.
Our library helps you integrate LLMs with other tools and resources to create powerful AI applications. 

For example, let's say we want to create a `ModelChain` that takes user message, formats it with a `PromptTemplate`, 
and sends it to an `ChatGPT` LLM to generate funny responses to the message. This allows us to generate a response based on the user's input.

Note: For that example to work you would need OpenAI API key.

```csharp
var llm = new ChatGptModel(Constants.OpenAIKey, ChatGptModelConfiguration.Default with
        {
            Temperature = 0.9f
        });

var oneInputPrompt = new PromptTemplate("What's a funny response to '{message}'");

var chain = new ModelChain(oneInputPrompt, llm);
var executor = chain.GetExecutor();
```

Now we can run that chain only specifying input value.

```csharp
var result = await executor.PromptAsync("I have some exciting news to share with you!");
Console.WriteLine(result);
```
```text
> I hope it's not that you finally learned how to tie your shoes, because that's not that exciting.
```

Or with another input

```csharp
var result2 = await executor.PromptAsync("Want to grab lunch later?");
Console.WriteLine(result2);
```
```text
> I would, but I'm on a "seefood" diet - I see food and I eat it. So better not tempt me!
```

## What is this?

Welcome to our library, which is designed to support the development of cutting-edge applications powered by Large Language Models (LLMs) in dotnet.

As you may know, LLMs are an exciting and rapidly-evolving technology that offers developers unprecedented natural language processing and generation capabilities. 
However, LLMs can achieve their full potential when used in conjunction with other sources of computation or knowledge.

Some examples of solutions that you could create using our library include:

- Summarization
- Question Answering
- Code Generation
- Chatbots
- and much more

## Documentation

Please see the [full documentation](https://xakpc.github.io/DotnetPrompt/index.html) on:

- Getting started (installation, setting up the environment, simple examples)
- How-To examples (demos, integrations, helper functions)
- Reference (full API docs)
- Resources (high-level explanation of core concepts)

## Links

- [Documentation](https://xakpc.github.io/DotnetPrompt/index.html)
- [Contributing Guidelines](https://github.com/xakpc/DotnetPrompt/blob/main/CONTRIBUTING.md)
- [License](https://github.com/xakpc/DotnetPrompt/blob/main/LICENSE.txt)