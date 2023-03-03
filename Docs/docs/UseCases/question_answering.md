# Question Answering

Question answering is an important task in natural language processing that involves providing answers to questions based on a given context. 
With recent advancements in large language models, such as OpenAI's GPT series, it has become easier to build highly accurate question answering systems. 
In this article, we will explore how to build a simple question answering system using the `DotnetPrompt` and OpenAI API.

## Few-shots learning example

By using a `ModelChain` with history of previous messages and context you could construct question answering chain.

[!code-csharp[](../../../DotnetPrompt.Tests.Examples/Chains/ChatGptModelChainExamples.cs#Example_ModelChainFewShotPromptTemplate_QuestionAnswering)]

## Web Question Answering

When we combine Chain, Web parse and Vector Database we could achieve a chain what could answer on questions on any website.

**WebQAChain - Coming Soon**