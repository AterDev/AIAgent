namespace KnowledgeBaseMod.Services;

public record TextChunk(int Index, string Content, int TokenCount);

public interface ITextChunker
{
    IReadOnlyList<TextChunk> Split(string text, int maxTokens = 800, int overlapTokens = 160);
}
