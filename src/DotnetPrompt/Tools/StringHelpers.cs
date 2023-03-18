using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetPrompt.Tools
{
    internal static class StringHelpers
    {
        /// <summary>
        /// Split string into chunks by length or 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="maxLength"></param>
        /// <param name="endChars"></param>
        /// <returns></returns>
        public static IEnumerable<string> SplitStringIntoChunks(string input, int maxLength, char[]? endChars = null)
        {
            if (string.IsNullOrEmpty(input))
            {
                yield break;
            }

            endChars ??= new char[] { '.', ',', ';', ':', '!', '?', '\n' };

            var startIndex = 0;
            var sb = new StringBuilder();

            while (startIndex < input.Length)
            {
                var endIndex = startIndex + maxLength;
                if (endIndex >= input.Length)
                {
                    sb.Append(input, startIndex, input.Length - startIndex);
                    yield return sb.ToString().Trim();
                    yield break;
                }

                var lastPunctuationIndex = input.LastIndexOfAny(endChars, endIndex - 1, endIndex - startIndex);
                if (lastPunctuationIndex == -1)
                {
                    lastPunctuationIndex = input.IndexOfAny(endChars, endIndex);
                }

                if (lastPunctuationIndex == -1)
                {
                    lastPunctuationIndex = endIndex;
                }

                sb.Append(input, startIndex, lastPunctuationIndex - startIndex + 1);
                yield return sb.ToString().Trim();
                sb.Clear();

                startIndex = lastPunctuationIndex + 1;
            }
        }
    }
}
