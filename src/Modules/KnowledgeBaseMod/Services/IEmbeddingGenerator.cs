namespace KnowledgeBaseMod.Services;

public interface IEmbeddingGenerator
{
    float[] Generate(string text, int size);
}
