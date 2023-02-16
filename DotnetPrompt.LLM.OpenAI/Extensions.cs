using System.Collections.Generic;

namespace DotnetPrompt.LLM.OpenAI;

internal static class Extensions
{
    public static void AddRange<T>(this IList<T> list, IList<T> listToAdd)
    {
        foreach (var item in listToAdd)
        {
            list.Add(item);
        }
    }
}