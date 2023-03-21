using DotnetPrompt.Abstractions.LLM;
using DotnetPrompt.Prompts;
using Microsoft.Extensions.Logging;

namespace DotnetPrompt.Chains.Specialized;

/// <summary>
/// A simple summarize chain
/// </summary>
public class SummarizeChain : ModelChain
{
    private const string Template = """
        Write a concise summary of the following text
        
        Text:
        {text}
        ---
        
        Summary:
        """;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="llm"></param>
    /// <param name="logger"></param>
    public SummarizeChain(ILargeLanguageModel llm, ILogger? logger = null) : base(new PromptTemplate(Template), llm, "summary", logger)
    {

    }
}