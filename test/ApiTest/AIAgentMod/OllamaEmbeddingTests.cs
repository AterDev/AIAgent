using System.Net.Http.Json;
using System.Net.Sockets;

namespace ApiTest.AIAgentMod;

/// <summary>
/// Ollama 本地 embedding 集成测试：验证 Aspire AddOllama + bge-m3 通过 OpenAI 兼容 <c>/v1/embeddings</c> 返回 1024 维向量。
/// 依赖 AppHost 已启动并且 bge-m3:latest 已 pull 完成。
/// 若 TCP 11434 不可达（例如 Docker 未启动或首次拉模型尚未完成），则跳过测试，避免 CI 失败。
/// </summary>
public class OllamaEmbeddingTests
{
    private const string OllamaHost = "127.0.0.1";
    private const int OllamaPort = 11434;
    private const string OllamaEmbeddingsUrl = "http://127.0.0.1:11434/v1/embeddings";
    private const string EmbeddingModel = "bge-m3";
    private const int ExpectedDimensions = 1024;

    [Test]
    public async Task Ollama_ShouldReturn1024DimEmbedding_ViaOpenAiCompatible()
    {
        if (!await IsOllamaReachableAsync())
        {
            Console.WriteLine($"[SKIP] Ollama not reachable at {OllamaHost}:{OllamaPort}");
            return;
        }

        using var http = new HttpClient { BaseAddress = new Uri(OllamaEmbeddingsUrl) };
        http.Timeout = TimeSpan.FromMinutes(2);

        var response = await http.PostAsJsonAsync(OllamaEmbeddingsUrl, new
        {
            model = EmbeddingModel,
            input = new[] { "Hello, world", "你好，世界" }
        });

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[Ollama] status={(int)response.StatusCode} body={err}");
        }

        await Assert.That(response.IsSuccessStatusCode).IsTrue();

        var result = await response.Content.ReadFromJsonAsync<OllamaEmbeddingResponse>();
        await Assert.That(result).IsNotNull();
        await Assert.That(result!.Data).IsNotNull();
        await Assert.That(result.Data!.Length).IsEqualTo(2);
        await Assert.That(result.Data[0].Embedding!.Length).IsEqualTo(ExpectedDimensions);
        await Assert.That(result.Data[1].Embedding!.Length).IsEqualTo(ExpectedDimensions);
    }

    private static async Task<bool> IsOllamaReachableAsync()
    {
        try
        {
            using var tcp = new TcpClient();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            await tcp.ConnectAsync(OllamaHost, OllamaPort, cts.Token);
            return tcp.Connected;
        }
        catch
        {
            return false;
        }
    }

    private sealed class OllamaEmbeddingResponse
    {
        public OllamaEmbeddingItem[]? Data { get; set; }
    }

    private sealed class OllamaEmbeddingItem
    {
        public float[]? Embedding { get; set; }
    }
}
