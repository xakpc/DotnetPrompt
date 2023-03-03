using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotnetPrompt.Abstractions.Tools;

/// <summary>
/// Interface for embedding models.
/// </summary>
public interface IEmbeddings
{
    /// <summary>
    /// Embed search docs.
    /// </summary>
    /// <param name="texts"></param>
    /// <returns></returns>
    Task<IList<TextEmbedding>> EmbedAsync(IList<string> texts);

    /// <summary>
    /// Embed query text.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    Task<TextEmbedding> EmbedAsync(string text);
}

/// <summary>
/// 
/// </summary>
/// <param name="Embedding"></param>
/// <param name="Text"></param>
public record TextEmbedding(IReadOnlyList<float> Embedding, string Text);
