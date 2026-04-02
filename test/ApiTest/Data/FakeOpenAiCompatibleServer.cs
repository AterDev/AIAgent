using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace ApiTest.Data;

/// <summary>
/// Minimal local OpenAI-compatible chat completion server for integration tests.
/// </summary>
public sealed class FakeOpenAiCompatibleServer : IAsyncDisposable
{
    private readonly HttpListener _listener = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _serverTask;
    private int _requestCount;

    private FakeOpenAiCompatibleServer(int port, string assistantContent)
    {
        var prefix = $"http://127.0.0.1:{port}/";
        _listener.Prefixes.Add(prefix);
        _listener.Start();

        BaseUri = new Uri($"{prefix}v1/");
        AssistantContent = assistantContent;
        _serverTask = Task.Run(ProcessLoopAsync);
    }

    public Uri BaseUri { get; }

    public string AssistantContent { get; }

    public int RequestCount => Volatile.Read(ref _requestCount);

    public ConcurrentQueue<string> ReceivedBodies { get; } = new();

    public static Task<FakeOpenAiCompatibleServer> StartAsync(string assistantContent)
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();

        return Task.FromResult(new FakeOpenAiCompatibleServer(port, assistantContent));
    }

    private async Task ProcessLoopAsync()
    {
        while (!_cts.IsCancellationRequested)
        {
            HttpListenerContext? context = null;
            try
            {
                context = await _listener.GetContextAsync().WaitAsync(_cts.Token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (HttpListenerException) when (_cts.IsCancellationRequested)
            {
                break;
            }
            catch (ObjectDisposedException) when (_cts.IsCancellationRequested)
            {
                break;
            }

            if (context is not null)
            {
                await HandleRequestAsync(context);
            }
        }
    }

    private async Task HandleRequestAsync(HttpListenerContext context)
    {
        try
        {
            using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding ?? Encoding.UTF8);
            var body = await reader.ReadToEndAsync();
            ReceivedBodies.Enqueue(body);
            Interlocked.Increment(ref _requestCount);

            if (context.Request.HttpMethod == HttpMethod.Post.Method
                && string.Equals(context.Request.Url?.AbsolutePath, "/v1/chat/completions", StringComparison.OrdinalIgnoreCase))
            {
                var modelName = TryReadModelName(body) ?? "fake-model";
                await WriteJsonAsync(context.Response, new
                {
                    id = $"chatcmpl_{Guid.NewGuid():N}",
                    @object = "chat.completion",
                    created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    model = modelName,
                    choices = new[]
                    {
                        new
                        {
                            index = 0,
                            message = new
                            {
                                role = "assistant",
                                content = AssistantContent,
                            },
                            finish_reason = "stop",
                        }
                    },
                    usage = new
                    {
                        prompt_tokens = 11,
                        completion_tokens = 7,
                        total_tokens = 18,
                    }
                });

                return;
            }

            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            await WriteJsonAsync(context.Response, new
            {
                error = new
                {
                    message = $"Unsupported route: {context.Request.HttpMethod} {context.Request.Url?.AbsolutePath}",
                }
            });
        }
        catch (Exception ex) when (ex is ObjectDisposedException or HttpListenerException or IOException)
        {
            if (_cts.IsCancellationRequested)
            {
                return;
            }

            if (context.Response.OutputStream.CanWrite)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }
        catch
        {
            if (context.Response.OutputStream.CanWrite)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }
        finally
        {
            try
            {
                context.Response.OutputStream.Close();
                context.Response.Close();
            }
            catch
            {
                // Ignore shutdown/transport cleanup errors in tests.
            }
        }
    }

    private static async Task WriteJsonAsync(HttpListenerResponse response, object payload)
    {
        var json = JsonSerializer.Serialize(payload);
        var buffer = Encoding.UTF8.GetBytes(json);
        response.ContentType = "application/json";
        response.ContentEncoding = Encoding.UTF8;
        response.ContentLength64 = buffer.Length;
        await response.OutputStream.WriteAsync(buffer);
    }

    private static string? TryReadModelName(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            if (document.RootElement.TryGetProperty("model", out var modelElement))
            {
                return modelElement.GetString();
            }
        }
        catch (JsonException)
        {
            return null;
        }

        return null;
    }

    public async ValueTask DisposeAsync()
    {
        if (!_cts.IsCancellationRequested)
        {
            _cts.Cancel();
        }

        if (_listener.IsListening)
        {
            _listener.Close();
        }

        try
        {
            await _serverTask.WaitAsync(TimeSpan.FromSeconds(5));
        }
        catch
        {
            // Ignore shutdown timing issues in tests.
        }

        _cts.Dispose();
    }
}