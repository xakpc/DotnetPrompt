using DotnetPrompt.Abstractions.LLM;
using DotnetPrompt.Prompts;
using Microsoft.Extensions.Logging;

namespace DotnetPrompt.Chains.Specialized;

/// <summary>
/// Simple pre-built chain for conversation generation
/// </summary>
public class ConversationChain : ModelChain
{
    private const string template = "The following is a friendly conversation between a human and an AI. The AI is talkative and provides lots of specific details from its context. If the AI does not know the answer to a question, it truthfully says it does not know.\r\n\r\n" +
                                    "Current conversation::\r\n" +
                                    "{history}\r\n" +
                                    "---\r\n" +
                                    "Human:{input}\r\n" +
                                    "AI: ";

    /// <inheritdoc />
    public override string DefaultOutputKey => "response";

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="llm"></param>
    /// <param name="logger"></param>
    public ConversationChain(ILargeLanguageModel llm, ILogger<ConversationChain> logger = null) : base(new PromptTemplate(template), llm, logger)
    {

    }
}