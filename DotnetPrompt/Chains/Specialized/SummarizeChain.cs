using DotnetPrompt.Abstractions.LLM;
using DotnetPrompt.Prompts;
using Microsoft.Extensions.Logging;

namespace DotnetPrompt.Chains.Specialized;

/// <summary>
/// A simple summarize chain
/// </summary>
public class SummarizeChain : ModelChain
{
    private const string Template = "Summarize the following text.\r\n\r\n" +
                                    "Text:\r\n" +
                                    "{text}\r\n" +
                                    "---\r\n\r\n" +
                                    "Summary:";

    /// <inheritdoc />
    public override string DefaultOutputKey => "summary";

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="llm"></param>
    /// <param name="logger"></param>
    public SummarizeChain(ILargeLanguageModel llm, ILogger<ModelChain>? logger = null) : base(new PromptTemplate(Template), llm, logger)
    {

    }
}