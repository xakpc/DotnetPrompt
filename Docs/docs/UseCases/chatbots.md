# Chatbots

Large Language Models are good for chatbots because they can understand natural language and generate human-like responses. 
With the ability to analyze and learn from vast amounts of data, these models can quickly adapt to different types of conversations and provide relevant and personalized responses.

## Few-shots learning example

By using a `ModelChain` with history of previous messages you could construct chatbot capable to talk and remember previous discussion.

[!code-csharp[](../../../DotnetPrompt.Tests.Examples/Chains/ChatGptModelChainExamples.cs#Example_ChatMLPromptTemplate_Chatbot)]

## Conversational Chain

When we combine Chain, Memory and ChatML model we could construct ultimative chatbot that could talk on any topics.

**ConversationalChain - Coming Soon**