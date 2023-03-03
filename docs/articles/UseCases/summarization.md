# Summarizing Texts

Summarizing texts is an essential task for anyone who deals with large amounts of information. 
It can help save time and improve comprehension by condensing a lengthy text into its most crucial points. 

Fortunately, with the development of artificial intelligence, we can now use machine learning models to summarize texts automatically. 
In this article, we will look at how to use the `DotnetPrompt` library to summarize texts.

## Summarizing Texts Using Few-Shots DotnetPrompt

To summarize texts using DotnetPrompt, we will follow these simple steps:

### Define the Prompt Template

A prompt template is a string that defines the example format for the text to be summarized. 
It should include placeholders for the original text and the summary. For example:

```csharp
var template = new PromptTemplate("Original: {original}\nSummary: {summary}");
```

### Define the Suffix Template

The suffix template is a string that defines the output format for the summary. It should include a placeholder for the original text. For example:

```csharp
var suffix = new PromptTemplate("Original: {original}\nSummary: ");
```

### Prepare Examples
Prepare a list of examples that the model can learn from. Each example should include the original text and its summary. For example:

```csharp
var examples = new List<object>()
{
    new
    {
        original = "This is an example of the original text.",
        summary = "This is a summary of the original text."
    },
    // Add more examples here
}.Select(i => i.ToDictionary()).ToList();
```

### Create the Few-Shot Prompt Template
Create a new Few-Shot Prompt Template object by passing in the prompt and suffix templates and the list of examples. For example:

```csharp
var prompt = new FewShotPromptTemplate(template, suffix, examples);
```

### Load the Language Model
Create a new instance of the OpenAI language model using your OpenAI API key. For example:
```csharp
var llm = new OpenAIModel(Constants.OpenAIKey, OpenAIModelConfiguration.Default with { Temperature = 0.1f, MaxTokens = 100 });
```

### Create the `ModelChain`

Create a new instance of the Model Chain class by passing in the Few-Shot Prompt Template and the OpenAI language model. For example:

```csharp
var llmChain = new ModelChain(prompt, llm);
```

### Generate the Summary

Call the `PromptAsync` method on the `ModelChain` instance, passing in the original text. The method will return a string containing the summary. For example:

```csharp
var summary = await llmChain.PromptAsync("This is the original text to be summarized.");
```

In this article, we have looked at how to summarize texts using the `DotnetPrompt` library. 

We have seen that it is a straightforward process that involves defining the prompt and suffix templates, preparing examples, 
creating a few-shot prompt template, loading the language model, creating the model chain, and generating the summary. 

With this knowledge, you should be able to summarize texts with ease using DotnetPrompt.

## Summarizing Documents Using Summarizing Chain

**Comming soon**