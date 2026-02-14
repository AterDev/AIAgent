using System.Security.Cryptography;
using System.Text;

namespace CoreMod.Services.Embedding;

/// <summary>
/// 简易嵌入向量生成（占位）
/// </summary>
public class HashEmbeddingGenerator
{
    public float[] Generate(string text, int size)
    {
        if (size <= 0)
        {
            size = 8;
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            return Enumerable.Repeat(0f, size).ToArray();
        }

        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(text));
        var result = new float[size];
        for (var i = 0; i < size; i++)
        {
            var b = bytes[i % bytes.Length];
            result[i] = b / 255f;
        }
        return result;
    }
}