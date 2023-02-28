# Getting Started

As a developer, you can use our library to create effective prompts for NLP models using prompt templates. 
In this article, we'll explain what prompt templates are and how they can help you create high-quality inputs for your models. 
You'll learn how to design prompt templates tailored to your specific needs and how to use few-shot learning to generate more examples. 
We'll also cover the importance of selecting diverse examples to ensure your prompt template produces high-quality inputs for your NLP model.

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

The `FewShotPromptTemplate` is a feature in DotnetPrompts that utilizes few-shot learning to improve machine learning model 
accuracy with only a few examples. 

By providing examples alongside the input text, the model's performance can be significantly enhanced.
This part provides a list of examples and their results using the FewShotPromptTemplate class, 
which combines prefixes, examples, and suffixes into a prompt suitable for Natural Language Processing models.

Full list of examples and use-cases for few-shot learning could be found [here](./few_shots_example.md).

Let's take an example from before and rebuilt it with `FewShotPromptTemplate`

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

TODO EXAMPLES