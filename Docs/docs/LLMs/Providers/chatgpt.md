# ChatGPT from OpenAI

ChatGPT is a latest model `gpt-3.5-turbo` from OpenAI.

It is 10 times cheaper than a previous models and provide a new Chat Markup Language.

Installation

```
> dotnet add package DotnetPrompt.LLM.OpenAI --version 1.0.0-alpha.1
```

You would need to [get an API key](https://platform.openai.com/account/api-keys) to use this model.

Create model

[!code-csharp[](../../../../DotnetPrompt.Tests.Examples/LLMs/ChatGptModelExamples.cs#CreateOpenAiModel)]

Send prompt to get result

[!code-csharp[](../../../../DotnetPrompt.Tests.Examples/LLMs/ChatGptModelExamples.cs#Example_PromptLLM)]

Output

```text
> Why did the tomato turn red?
> Because it saw the salad dressing!
```

## Advanced usage

The majority of the tasks will require more than just using `PromptAsync` to obtain a single answer. So, let's consider how we could utilize the `ChatGptModel`.

To establish a `ChatMLPromptTemplate`, you must generate a `PromptTemplate` that includes the necessary _input variables_. 
When you format the _input values_, a ChatML formatted message will be created.

```csharp
// create prompt template
var oneInputPrompt = new PromptTemplate(template: "What's a funny response to '{message}'", inputVariables: new[] { "message" });
var chatTemplate = new ChatMLPromptTemplate(oneInputPrompt, system: "You are my buddy");

// create value
var valuesOneInput = new Dictionary<string, string>
{
    { "message", "I have some exciting news to share with you!" }
};

// create text for llm
var text = chatTemplate.Format(valuesOneInput);
Console.WriteLine(text);
```

result:

```json
[
    { "role": "system", "content": "You are my buddy" },
    { "role": "user", "content": "What's a funny response to 'I have some exciting news to share with you!'" }
]
```

You could also use shorter option to create `chatTemplate` by passing template string.

```csharp
var chatTemplate = new ChatMLPromptTemplate(What's a funny response to '{message}, system: "You are my buddy");

// create value
var valuesOneInput = new Dictionary<string, string>
{
    { "message", "I have some exciting news to share with you!" }
};

// create text for llm
var text = chatTemplate.Format(valuesOneInput);
Console.WriteLine(text);
```

result would be the same

```json
[
    { "role": "system", "content": "You are my buddy" },
    { "role": "user", "content": "What's a funny response to 'I have some exciting news to share with you!'" }
]
```

Next, we could call LLM directly:

```csharp
// get answer from llm
var response = await llm.PromptAsync(text);
Console.WriteLine(response);
```

resulting message:

```text
> Don't tell me you finally learned how to juggle flaming pineapples!
```

Or even better, we could use `ModelChain` to do all prompt formatting for us

```csharp
// create chain
var chain = new ModelChain(chatTemplate, llm);
var executor = chain.GetExecutor();

// get answer from the chain providing different values
var result = await executor.PromptAsync("I have some exciting news to share with you!");
Console.WriteLine(result);
var result2 = await executor.PromptAsync("Want to grab lunch later?");
Console.WriteLine(result2);
```

resulting messages:

```text
> Oh boy, did you finally learn how to juggle flaming pineapples?
> Sure, as long as you don't mind me wearing my taco costume.
```