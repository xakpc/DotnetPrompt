# Few-Shot Prompt Template 

Few-Shot Prompt Template  used in few-shot learning. It is a technique that enables a machine learning model to make predictions with only a few examples. 
Models like GPT-3 are so large that they can easily adjust to different contexts without needing to be retrained.

Providing the model with a few examples can significantly improve its accuracy. 
In Natural Language Processing, these examples are passed along with the text input.

This is a list of a few examples of few-shot learning done with DotnetPrompts' `FewShotPromptTemplate` and their results. 
The `FewShotPromptTemplate` class would take your prefix, examples and suffix and combine it with separator into a suitable prompt for LLM.

All of these examples can be found as unit tests in the [GitHub repository](). 

Most of these examples are quite basic, and better results can be achieved more efficiently by using [chains](../Chains/getting_started.md).

## Sentiment Analysis 

[!code-csharp[](../../../DotnetPrompt.Tests.Examples/Chains/FewShotModelExamples.cs#Example_ModelChainFewShotPromptTemplate_SentimentAnalysis)]

```text
> answer1: Positive
> answer2: Negative
> answer3: Neutral
```

## HTML code generation

[!code-csharp[](../../../DotnetPrompt.Tests.Examples/Chains/FewShotModelExamples.cs#Example_ModelChainFewShotPromptTemplate_HTMLGeneration)]

Generated HTML

```html
<h1 style = "color:black;">Welcome to AI!</h1>
```
```html
<ul style="background-color:black; color:yellow;">
  <li>Country 1</li>
  <li>Country 2</li>
  <li>Country 3</li>
  <li>Country 4</li>
  <li>Country 5</li>
</ul>
```

## SQL code generation

For this example a helper method was introduced to convert from anonymous object to `IDictionary` to pass it into the LLM

[!code-csharp[](../../../DotnetPrompt.Tests.Examples/Chains/FewShotModelExamples.cs#Example_ModelChainFewShotPromptTemplate_SQLGeneration)]

[!code-csharp[](../../../DotnetPrompt.Tests.Examples/Chains/FewShotModelExamples.cs#Example_ObjectExtension)]

Generated SQL

```sql
> SELECT * FROM Employee LIMIT 3;
```

```sql
> SELECT e.EMPLOYEE_ID, e.FIRST_NAME, e.LAST_NAME, c.COMPANY_NAME FROM Employee e INNER JOIN Company c ON e.COMPANY = c.COMPANY_NAME;
```

## Advanced Entity Extraction (NER)

[!code-csharp[](../../../DotnetPrompt.Tests.Examples/Chains/FewShotModelExamples.cs#Example_ModelChainFewShotPromptTemplate_NERGeneration)]

```text
>  David Melvin
> [Position]: Senior Adviser
> [Company]: CITIC CLSA

```

```text
>  Pat Gelsinger
> [Position]: Chief Technology Officer
> [Company]: Intel
```

Note, that we asked to complete text after `[Name]: ` text, and that exactly what model did - `[Name]` is not included in generated string.

## Question Answering

[!code-csharp[](../../../DotnetPrompt.Tests.Examples/Chains/FewShotModelExamples.cs#Example_ModelChainFewShotPromptTemplate_QuestionAnswering)]

```text
> The "Pro" plan.
```
```text
> English
```

## Grammar and Spelling Correction

[!code-csharp[](../../../DotnetPrompt.Tests.Examples/Chains/FewShotModelExamples.cs#Example_ModelChainFewShotPromptTemplate_GrammarCorrection)]

```text
> I do not want to go.
```
```text
> What are you doing?
```

## Machine Translation

[!code-csharp[](../../../DotnetPrompt.Tests.Examples/Chains/FewShotModelExamples.cs#Example_ModelChainFewShotPromptTemplate_MachineTranslation)]

```text
> Do you speak French?
```
```text
> How are you?
```

## Tweet Generation

[!code-csharp[](../../../DotnetPrompt.Tests.Examples/Chains/FewShotModelExamples.cs#Example_ModelChainFewShotPromptTemplate_TweetGeneration)]

```text
> Life is like a box of cats, you never know what you're gonna get! 
```
```text
> NLP is changing the way we interact with machines, and making them smarter than ever before.
```

## Chatbot and Conversational AI

[!code-csharp[](../../../DotnetPrompt.Tests.Examples/Chains/FewShotModelExamples.cs#Example_ModelChainFewShotPromptTemplate_Chatbot)]

```text
> I'm sorry to hear that. Is there anything I can do to help?
```
```text
> That sounds like a great thing, so why are you sad?
```

Note that this is not a real chatbot, but just an example. GPT-3 are "stateless" model, meaning that every request you make is new and the AI is not 
going to remember anything about the previous requests you made.

In many Natural Language Processing situations it's not a problem (summarization, classification, paraphras, etc), 
but as far as chatbots are concerned it's definitely an issue because we do want our chatbot to memorize the discussion history in order to make more relevant responses.

## Intent Classification

[!code-csharp[](../../../DotnetPrompt.Tests.Examples/Chains/FewShotModelExamples.cs#Example_ModelChainFewShotPromptTemplate_IntentClassification)]

```text
> teach Chinese
```
```text
> open fridge
```

## Paraphrasing

[!code-csharp[](../../../DotnetPrompt.Tests.Examples/Chains/FewShotModelExamples.cs#Example_ModelChainFewShotPromptTemplate_Paraphrasing)]

```text
> In 2023, nature positive travel is expected to become popular as travelers look for ways to reduce and undo their environmental footprint.
```
```text
> The rise of remote work in 2022 made digital nomadism a popular lifestyle.
```

## Summarization

[!code-csharp[](../../../DotnetPrompt.Tests.Examples/Chains/FewShotModelExamples.cs#Example_ModelChainFewShotPromptTemplate_Summarization)]

```text
> The season three finale of Succession saw Logan Roy defy his children by selling Waystar Royco to a Swedish tech bro, setting up a bold new future for the show. Season four could pick up at the moment of Logan's betrayal and focus on the Roy siblings' efforts to reorganize their rebellion against him.
```
```text
> Boris Roizman is the founder of the Museum of Nevyansk Icon, the first private museum to collect icon-paintings, located in Yekaterinburg, Russia. The museum houses over 600 exhibits, including icons, gospel covers, crosses, books, and wooden sculptures, ranging from The Egyptian Mother of God (1734) to Christ Pantocrator (1919). Roizman was responsible for finding, searching, and restoring the icons.
```
Note: Roizman actual name is [Yevgeny](https://en.wikipedia.org/wiki/Yevgeny_Roizman)

## Zero-shot text classification

[!code-csharp[](../../../DotnetPrompt.Tests.Examples/Chains/FewShotModelExamples.cs#Example_ModelChainFewShotPromptTemplate_TextClassification)]

```text
> cooking
```
```text
> travel
```

## Keyword and Keyphrase Extraction

[!code-csharp[](../../../DotnetPrompt.Tests.Examples/Chains/FewShotModelExamples.cs#Example_ModelChainFewShotPromptTemplate_KeywordExtraction)]

```text
> transformer, token, errors, inputting, document, paragraphs, mean pooling
```
```text
> classes, user, embeddings
```

Keyword extraction is the process of identifying the main ideas from a text. Keyphrase extraction is similar, but it involves extracting multiple words.

[!code-csharp[](../../../DotnetPrompt.Tests.Examples/Chains/FewShotModelExamples.cs#Example_ModelChainFewShotPromptTemplate_KeyphraseExtraction)]

```text
> transformer models, token limit, large documents, splitting, mean pooling
```
```text
classes, zero-shot classification, embeddings, class label
```

This time, instead of extracting one single word, we want to extract several words (known as keyphrases) from the same example.

## Product Description and Ad Generation

[!code-csharp[](../../../DotnetPrompt.Tests.Examples/Chains/FewShotModelExamples.cs#Example_ModelChainFewShotPromptTemplate_AdGeneration)]

```text
> Stylish t-shirts for men, only $39.
```
```text
> Get a car for your dog for just 199$.
```

## A little extra

These examples could as well be created by use few-shots prompts to create them for us. Here is an snippet how to generate prompts based on example.

[!code-csharp[](../../../DotnetPrompt.Tests.Examples/Chains/FewShotModelExamples.cs#Example_ModelChainFewShotPromptTemplate_PromptGeneration)]

```csharp
var example = new PromptTemplate("Keyword: {keyword}\nTweet: {tweet}");
var suffix = new PromptTemplate("Keyword: {keyword}\nTweet: ");
```

This obviously could be expanded to entire codebase generation.