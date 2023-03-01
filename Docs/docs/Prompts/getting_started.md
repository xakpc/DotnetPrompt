# Getting Started

One of the key component of any LLM and DotnetPrompt is a `PromptTemplate`.

This article aims to clarify the concept of prompt templates and how they can assist you in producing high-quality inputs for your models. 
You will understand how to create prompt templates that are customized to your specific requirements and how to use few-shot learning to generate additional examples.

## What is a prompt template?

A prompt template is a pre-defined structure that can be filled in with specific information to generate a prompt. The beauty of prompt templates lies in their ability to be highly reproducible. 
By providing a standardized format for prompts, prompt templates can help ensure consistency in the quality of inputs that are fed into the model.

At its core, a prompt template consists of a text string or "the template," which can take in parameters from the end user to generate a prompt. These parameters can include language model instructions, few-shot examples to improve the model's response, or specific questions for the model to answer.

To help you better understand prompt templates, we've included a code snippet below that showcases a basic prompt template. With the ability to customize this template with specific parameters, you can generate a wide range of different prompts to suit your needs and tasks.

```text
I want you to act as a naming consultant for new companies.
Here are some examples of good company names:
- search engine, Google
- social media, Facebook
- video sharing, YouTube
The name should be short, catchy and easy to remember.
What is a good name for a company that makes {product}?
```

### Input Variables

Input variables are the variables that are used to fill in the template string. In the example above, the input variable is a `{product}`.

Given an _input variables_, the `PromptTemplate` can generate a prompt by filling in the template string with _input values_. 
For example, if the input value is `mobile phone`, the template string can be formatted by `IPromptTemplate.Format` method to generate the following prompt:

```text
I want you to act as a naming consultant for new companies.
Here are some examples of good company names:
- search engine, Google
- social media, Facebook
- video sharing, YouTube
The name should be short, catchy and easy to remember." +
What is a good name for a company that makes mobile phone?
```

## Create a Prompt Template

You can create prompts using the `PromptTemplate` class. Prompt templates can take any number of *input variables*, and can be formatted with *input values* to generate a prompt.

[!code-csharp[Example_CreatingPromptTemplate](../../../DotnetPrompt.Tests.Examples/Prompts/PromptTemplateExamples.cs#Example_CreatingPromptTemplate)]

When you want to fill template with values you need to use `Dictionary<string, string>` where keys should be the same as your input variables and values could be
any valid string that need to be fill in template.

```csharp
var values = new Dictionary<string, string>()
{
    { "product", "toy car" }
};

var finalPrompt = prompt.Format(values);
```

You could read more about prompt template and how to use it [here](./prompt_template.md).

> [!NOTE]
> Currently, the template should be formatted as a C# formatted string. In the future, we will add more templating languages such as Mustache.

## Pass few shot examples to a prompt template

The `FewShotPromptTemplate` class combines prefixes, examples, and suffixes into a prompt that is suitable for LLMs. 
This allows the user to create prompts with a few examples that can significantly enhance the model's accuracy.

For a complete list of examples and use cases for few-shot learning, you can refer to [this page](./few_shots_example.md). 
Here's an example to demonstrate how FewShotPromptTemplate works:

[!code-csharp[](../../../DotnetPrompt.Tests.Examples/Prompts/FewShotPromptTemplateExamples.cs#Example_FewShotPromptTemplate_Simple)]

```text
I want you to act as a naming consultant for new companies.
Here are some examples of good company names:
- search engine, Google
- social media, Facebook
- video sharing, YouTube
The name should be short, catchy and easy to remember.
What is a good name for a company that makes toy cars?
```

## Select examples for a prompt template

If you have a large number of examples, you can use the implementation of `IExampleSelector` to select a subset of examples that will be most informative for the Language Model. 
This will help you generate a prompt that is more likely to generate a good response.

There are a lot of different approaches to select examples, but currently only one is supported: `LengthBasedExampleSelector` which try to fit your examples into maximum allowed size of input 
for a model input size.