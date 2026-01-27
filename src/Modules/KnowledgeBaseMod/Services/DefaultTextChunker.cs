using System.Text;

namespace KnowledgeBaseMod.Services;

/// <summary>
/// 简易分块（按词数近似 token）
/// </summary>
public class DefaultTextChunker : ITextChunker
{
    public IReadOnlyList<TextChunk> Split(string text, int maxTokens = 800, int overlapTokens = 160)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (words.Length == 0)
        {
            return [];
        }

        var chunks = new List<TextChunk>();
        var index = 0;
        var start = 0;
        while (start < words.Length)
        {
            var builder = new StringBuilder();
            var count = 0;
            var i = start;
            for (; i < words.Length; i++)
            {
                if (count >= maxTokens)
                {
                    break;
                }

                builder.Append(words[i]);
                builder.Append(' ');
                count++;
            }

            var content = builder.ToString().Trim();
            if (!string.IsNullOrWhiteSpace(content))
            {
                chunks.Add(new TextChunk(index++, content, count));
            }

            if (i >= words.Length)
            {
                break;
            }

            start = Math.Max(0, i - overlapTokens);
        }

        return chunks;
    }
}
