using DotnetPrompt.Abstractions.LLM;
using DotnetPrompt.Prompts;
using Microsoft.Extensions.Logging;

namespace DotnetPrompt.Chains.Specialized;

/// <summary>
/// Chain to answer question based on input context
/// </summary>
/// <remarks>
/// Input Variables: context, question
/// Output Variables: answer
/// </remarks>
public class QuestionAnsweringChain : ModelChain
{
    private const string Template = "Answer the question based on the context below, and if the question can't be answered based on the context, say \"I don't know\"\n\n" +
                                    "Context: {context}\n\n" +
                                    "---\n\n" +
                                    "Question: {question}\n" +
                                    "Answer:";

    /// <inheritdoc />
    public override string DefaultOutputKey => "answer";

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="llm"></param>
    /// <param name="logger"></param>
    public QuestionAnsweringChain(ILargeLanguageModel llm, ILogger<ModelChain>? logger = null) 
        : base(new PromptTemplate(Template), llm, logger)
    {

    }
}