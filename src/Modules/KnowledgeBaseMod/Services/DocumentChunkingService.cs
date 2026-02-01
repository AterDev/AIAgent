using System.Text;

namespace KnowledgeBaseMod.Services;

/// <summary>
/// 文档分块服务
/// </summary>
public class DocumentChunkingService
{
    private const int DefaultChunkSize = 1500; // tokens
    private const int DefaultOverlapSize = 75; // tokens
    private const int MinChunkSize = 500;
    private const int MaxChunkSize = 2000;

    public List<TextChunk> ChunkText(
        string text, 
        int chunkSize = DefaultChunkSize, 
        int overlapSize = DefaultOverlapSize)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        // Validate parameters
        chunkSize = Math.Clamp(chunkSize, MinChunkSize, MaxChunkSize);
        overlapSize = Math.Clamp(overlapSize, 0, chunkSize / 2);

        var chunks = new List<TextChunk>();
        
        // Split by paragraphs first
        var paragraphs = SplitIntoParagraphs(text);
        
        var currentChunk = new StringBuilder();
        var currentTokenCount = 0;
        var chunkIndex = 0;

        foreach (var paragraph in paragraphs)
        {
            var paragraphTokens = EstimateTokens(paragraph);
            
            // If a single paragraph is too large, split it further
            if (paragraphTokens > chunkSize)
            {
                // Save current chunk if not empty
                if (currentChunk.Length > 0)
                {
                    chunks.Add(CreateChunk(currentChunk.ToString(), chunkIndex++));
                    currentChunk.Clear();
                    currentTokenCount = 0;
                }
                
                // Split large paragraph into sentences
                var sentences = SplitIntoSentences(paragraph);
                foreach (var sentence in sentences)
                {
                    var sentenceTokens = EstimateTokens(sentence);
                    
                    if (currentTokenCount + sentenceTokens > chunkSize)
                    {
                        if (currentChunk.Length > 0)
                        {
                            chunks.Add(CreateChunk(currentChunk.ToString(), chunkIndex++));
                            
                            // Add overlap from previous chunk
                            var overlap = GetOverlapText(currentChunk.ToString(), overlapSize);
                            currentChunk.Clear();
                            currentChunk.Append(overlap);
                            currentTokenCount = EstimateTokens(overlap);
                        }
                    }
                    
                    currentChunk.Append(sentence);
                    currentChunk.Append(' ');
                    currentTokenCount += sentenceTokens;
                }
            }
            else
            {
                // Check if adding this paragraph exceeds chunk size
                if (currentTokenCount + paragraphTokens > chunkSize)
                {
                    // Save current chunk
                    chunks.Add(CreateChunk(currentChunk.ToString(), chunkIndex++));
                    
                    // Add overlap from previous chunk
                    var overlap = GetOverlapText(currentChunk.ToString(), overlapSize);
                    currentChunk.Clear();
                    currentChunk.Append(overlap);
                    currentTokenCount = EstimateTokens(overlap);
                }
                
                currentChunk.AppendLine(paragraph);
                currentChunk.AppendLine();
                currentTokenCount += paragraphTokens;
            }
        }
        
        // Add the last chunk
        if (currentChunk.Length > 0)
        {
            chunks.Add(CreateChunk(currentChunk.ToString(), chunkIndex));
        }

        return chunks;
    }

    private static List<string> SplitIntoParagraphs(string text)
    {
        // Split by double newlines, markdown headers, or blank lines
        var paragraphs = text.Split(["\r\n\r\n", "\n\n"], StringSplitOptions.RemoveEmptyEntries);
        
        return paragraphs
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToList();
    }

    private static List<string> SplitIntoSentences(string text)
    {
        // Simple sentence splitting by common punctuation
        var sentences = new List<string>();
        var current = new StringBuilder();
        
        for (int i = 0; i < text.Length; i++)
        {
            current.Append(text[i]);
            
            // Check for sentence-ending punctuation
            if (text[i] is '.' or '!' or '?' or '。' or '！' or '？')
            {
                // Look ahead to ensure it's not part of an abbreviation
                if (i + 1 >= text.Length || char.IsWhiteSpace(text[i + 1]) || char.IsUpper(text[i + 1]))
                {
                    sentences.Add(current.ToString().Trim());
                    current.Clear();
                }
            }
        }
        
        // Add remaining text
        if (current.Length > 0)
        {
            sentences.Add(current.ToString().Trim());
        }
        
        return sentences.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
    }

    private static string GetOverlapText(string text, int overlapTokens)
    {
        if (overlapTokens <= 0 || string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        // Get the last N tokens worth of text
        var sentences = SplitIntoSentences(text);
        var overlap = new StringBuilder();
        int tokenCount = 0;

        // Start from the end and work backwards
        for (int i = sentences.Count - 1; i >= 0 && tokenCount < overlapTokens; i--)
        {
            var sentence = sentences[i];
            var sentenceTokens = EstimateTokens(sentence);
            
            if (tokenCount + sentenceTokens <= overlapTokens)
            {
                overlap.Insert(0, sentence + " ");
                tokenCount += sentenceTokens;
            }
            else
            {
                break;
            }
        }

        return overlap.ToString().Trim();
    }

    private static TextChunk CreateChunk(string content, int index)
    {
        return new TextChunk(
            index,
            content.Trim(),
            EstimateTokens(content)
        );
    }

    private static int EstimateTokens(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return 0;
        }

        // Simple estimation: average 4 characters per token
        return Math.Max(1, text.Length / 4);
    }
}
