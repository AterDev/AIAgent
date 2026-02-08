using System.Text;
using System.Text.RegularExpressions;

namespace CoreMod.Services;

/// <summary>
/// 文本分块器，支持中英文混合文本
/// 优先在语义边界（段落、句子）处分割，保持上下文连贯性
/// </summary>
public partial class DefaultTextChunker : Share.Services.ITextChunker
{
    /// <summary>
    /// 每个 token 大约对应的字符数（中文约 1.5 字符/token，英文约 4 字符/token，取加权平均）
    /// </summary>
    private const double CharsPerToken = 2.5;

    public IReadOnlyList<TextChunk> Split(string text, int maxTokens = 800, int overlapTokens = 160)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        // 按段落分割
        var paragraphs = SplitParagraphs(text);
        if (paragraphs.Count == 0)
        {
            return [];
        }

        var maxChars = (int)(maxTokens * CharsPerToken);
        var overlapChars = (int)(overlapTokens * CharsPerToken);

        var chunks = new List<TextChunk>();
        var buffer = new StringBuilder();
        var index = 0;

        foreach (var paragraph in paragraphs)
        {
            // 如果当前段落加入后会超出限制
            if (buffer.Length > 0 && buffer.Length + paragraph.Length + 1 > maxChars)
            {
                // 保存当前 buffer 为一个 chunk
                var content = buffer.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(content))
                {
                    chunks.Add(new TextChunk(index++, content, EstimateTokens(content)));
                }

                // 取 overlap 部分作为下一段的起始
                var overlapText = GetOverlapText(content, overlapChars);
                buffer.Clear();
                if (!string.IsNullOrWhiteSpace(overlapText))
                {
                    buffer.Append(overlapText);
                    buffer.Append('\n');
                }
            }

            // 如果单个段落超过 maxChars，按句子进一步切分
            if (paragraph.Length > maxChars)
            {
                var sentences = SplitSentences(paragraph);
                foreach (var sentence in sentences)
                {
                    if (buffer.Length > 0 && buffer.Length + sentence.Length + 1 > maxChars)
                    {
                        var content = buffer.ToString().Trim();
                        if (!string.IsNullOrWhiteSpace(content))
                        {
                            chunks.Add(new TextChunk(index++, content, EstimateTokens(content)));
                        }

                        var overlapText = GetOverlapText(content, overlapChars);
                        buffer.Clear();
                        if (!string.IsNullOrWhiteSpace(overlapText))
                        {
                            buffer.Append(overlapText);
                        }
                    }

                    // 如果单个句子仍然超过 maxChars，按字符硬切
                    if (sentence.Length > maxChars)
                    {
                        var remaining = sentence.AsSpan();
                        while (remaining.Length > 0)
                        {
                            var takeLen = Math.Min(maxChars - buffer.Length, remaining.Length);
                            if (takeLen <= 0)
                            {
                                var content = buffer.ToString().Trim();
                                if (!string.IsNullOrWhiteSpace(content))
                                {
                                    chunks.Add(new TextChunk(index++, content, EstimateTokens(content)));
                                }

                                buffer.Clear();
                                takeLen = Math.Min(maxChars, remaining.Length);
                            }

                            buffer.Append(remaining[..takeLen]);
                            remaining = remaining[takeLen..];
                        }
                    }
                    else
                    {
                        if (buffer.Length > 0)
                        {
                            buffer.Append(' ');
                        }

                        buffer.Append(sentence);
                    }
                }
            }
            else
            {
                if (buffer.Length > 0)
                {
                    buffer.Append('\n');
                }

                buffer.Append(paragraph);
            }
        }

        // 处理剩余内容
        var final = buffer.ToString().Trim();
        if (!string.IsNullOrWhiteSpace(final))
        {
            chunks.Add(new TextChunk(index, final, EstimateTokens(final)));
        }

        return chunks;
    }

    private static List<string> SplitParagraphs(string text)
    {
        return text.Split(["\r\n\r\n", "\n\n", "\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToList();
    }

    private static List<string> SplitSentences(string text)
    {
        // 匹配中英文句子结束符
        var parts = SentenceEndRegex().Split(text);
        var sentences = new List<string>();
        var current = new StringBuilder();

        foreach (var part in parts)
        {
            if (string.IsNullOrWhiteSpace(part))
            {
                continue;
            }

            current.Append(part);

            // 如果是句子结束符，则把累积的内容作为一个句子
            if (SentenceEndRegex().IsMatch(part) && current.Length > 0)
            {
                var sentence = current.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(sentence))
                {
                    sentences.Add(sentence);
                }

                current.Clear();
            }
        }

        if (current.Length > 0)
        {
            var rest = current.ToString().Trim();
            if (!string.IsNullOrWhiteSpace(rest))
            {
                sentences.Add(rest);
            }
        }

        return sentences;
    }

    private static string GetOverlapText(string text, int overlapChars)
    {
        if (string.IsNullOrWhiteSpace(text) || overlapChars <= 0)
        {
            return string.Empty;
        }

        if (text.Length <= overlapChars)
        {
            return text;
        }

        return text[^overlapChars..];
    }

    private static int EstimateTokens(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return 0;
        }

        return Math.Max(1, (int)(text.Length / CharsPerToken));
    }

    [GeneratedRegex(@"(?<=[。！？.!?\n])")]
    private static partial Regex SentenceEndRegex();
}
