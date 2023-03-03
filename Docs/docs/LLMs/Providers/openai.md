# OpenAI

Installation

```
> dotnet add package DotnetPrompt.LLM.OpenAI --version 1.0.0-alpha.1
```

You would need to [get an API key](https://platform.openai.com/account/api-keys) to use this model.

Create model

[!code-csharp[](../../../../DotnetPrompt.Tests.Examples/LLMs/OpenAIModelExamples.cs#CreateOpenAiModel)]

Send prompt to get result

[!code-csharp[](../../../../DotnetPrompt.Tests.Examples/LLMs/OpenAIModelExamples.cs#Example_PromptLLM)]

Output

```text
> Why did the chicken cross the road?
> To get to the other side!
```