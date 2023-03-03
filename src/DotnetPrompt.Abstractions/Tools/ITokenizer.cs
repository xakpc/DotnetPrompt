using System.Collections.Generic;

namespace DotnetPrompt.Abstractions.Tools;

/// <summary>
/// Interface for tokenizer
/// </summary>
/// <remarks>A tokenizer is a software component that breaks down a piece of text into smaller units called tokens.</remarks>
public interface ITokenizer
{
    /// <summary>
    /// Encode text as a list of numerical tokens
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    IList<int> Encode(string text);
}