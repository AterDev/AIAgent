using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using AdminService.Services;
using AGUI.Abstractions;
using AIAgentMod.Models.AgentDebugDtos;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

namespace UnitTest.AIAgentMod;

public class AguiTransportTests
{
    [Test]
    public async Task OfficialHost_StreamsTextToolsMetricsAndCompletion()
    {
        await using var app = await StartAsync(new StubChatClient());
        using var client = Client(app);
        using var response = await client.PostAsync("/debug", Input());
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync();
        var events = new List<JsonElement>();
        await foreach (var item in SseParser.Create(stream).EnumerateAsync())
            events.Add(JsonDocument.Parse(item.Data).RootElement.Clone());
        var types = events.Select(e => e.GetProperty("type").GetString()).ToList();
        await Assert.That(types.First()).IsEqualTo("RUN_STARTED");
        await Assert.That(types.Last()).IsEqualTo("RUN_FINISHED");
        await Assert.That(types.Contains("TEXT_MESSAGE_CONTENT")).IsTrue();
        await Assert.That(types.Contains("TOOL_CALL_START")).IsTrue();
        await Assert.That(types.Contains("TOOL_CALL_ARGS")).IsTrue();
        await Assert.That(types.Contains("TOOL_CALL_RESULT")).IsTrue();
        var text = events.First(e => e.GetProperty("type").GetString() == "TEXT_MESSAGE_CONTENT");
        await Assert.That(text.GetProperty("delta").GetString()).IsEqualTo("你好 👋");
        var call = events.First(e => e.GetProperty("type").GetString() == "TOOL_CALL_START");
        var result = events.First(e => e.GetProperty("type").GetString() == "TOOL_CALL_RESULT");
        await Assert.That(result.GetProperty("toolCallId").GetString()).IsEqualTo(call.GetProperty("toolCallId").GetString());
        var metrics = events.First(e => e.GetProperty("type").GetString() == "CUSTOM");
        await Assert.That(metrics.GetProperty("name").GetString()).IsEqualTo("debug.metrics");
    }

    [Test]
    public async Task OfficialHost_EmitsTerminalErrorWithoutSuccess()
    {
        await using var app = await StartAsync(new StubChatClient(error: true));
        using var client = Client(app);
        using var response = await client.PostAsync("/debug", Input());
        var body = await response.Content.ReadAsStringAsync();
        await Assert.That(body.Contains("RUN_ERROR")).IsTrue();
        await Assert.That(body.Contains("RUN_FINISHED")).IsFalse();
        await Assert.That(body.Contains("Agent not found")).IsTrue();
    }

    [Test]
    public async Task Disconnect_CancelsTheBackendRun()
    {
        var stub = new StubChatClient(wait: true);
        await using var app = await StartAsync(stub);
        using var client = Client(app);
        using var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "/debug") { Content = Input() },
            HttpCompletionOption.ResponseHeadersRead);
        await stub.Started.Task.WaitAsync(TimeSpan.FromSeconds(10));
        response.Dispose();
        await stub.Cancelled.Task.WaitAsync(TimeSpan.FromSeconds(10));
        await Assert.That(stub.Cancelled.Task.IsCompletedSuccessfully).IsTrue();
    }

    [Test]
    public async Task ToolLoggingOff_DoesNotEmitToolPayloads()
    {
        var events = AgentDebugChatClient.ConvertEvent(new AgentDebugStreamEvent
        { Type = "tool", ToolCall = new() { Name = "private", Input = "{}", Output = "secret" } }, false).ToList();
        await Assert.That(events.Count).IsEqualTo(0);
    }

    [Test]
    public async Task JavascriptHttpAgent_ConsumesOfficialDotnetHost()
    {
        await using var app = await StartAsync(new StubChatClient());
        using var client = Client(app);
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "AIAgent.slnx")))
            root = root.Parent;
        if (root is null) throw new InvalidOperationException("Repository root was not found.");
        var info = new System.Diagnostics.ProcessStartInfo("node")
        {
            WorkingDirectory = Path.Combine(root.FullName, "src", "ClientApp", "WebApp"),
            RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false,
            CreateNoWindow = true
        };
        info.ArgumentList.Add("scripts/test-agui-transport.cjs");
        info.ArgumentList.Add(new Uri(client.BaseAddress!, "/debug").ToString());
        using var process = System.Diagnostics.Process.Start(info)!;
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        try { await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(20)); }
        finally { if (!process.HasExited) process.Kill(entireProcessTree: true); }
        Console.WriteLine(await output);
        Console.WriteLine(await error);
        await Assert.That(process.ExitCode).IsEqualTo(0);
    }
    [Test]
    public async Task ModelAdapter_PreservesWhitespaceAndUsage()
    {
        var session = new ModelMod.Models.ModelDebugDtos.ModelDebugStreamSession
        { RequestId = "model-test", ModelName = "mock-model", Stream = ModelChunks(false) };
        var updates = new List<ChatResponseUpdate>();
        await foreach (var update in ModelDebugChatClient.StreamSessionAsync(session)) updates.Add(update);
        var text = string.Concat(updates.SelectMany(u => u.Contents).OfType<TextContent>().Select(t => t.Text));
        await Assert.That(text).IsEqualTo("hello world");
        await Assert.That(updates.Where(u => u.Contents.OfType<TextContent>().Any()).Select(u => u.MessageId).Distinct().Count()).IsEqualTo(1);
        var metrics = (CustomEvent)updates.Single(u => u.RawRepresentation is CustomEvent).RawRepresentation!;
        await Assert.That(metrics.Name).IsEqualTo("model.metrics");
        var value = JsonSerializer.SerializeToElement(metrics.Value);
        await Assert.That(value.GetProperty("totalTokens").GetInt32()).IsEqualTo(7);
        await Assert.That(value.GetProperty("content").GetString()).IsEqualTo("hello world");
    }

    [Test]
    public async Task ModelAdapter_DoesNotCompleteAnErroredStream()
    {
        var session = new ModelMod.Models.ModelDebugDtos.ModelDebugStreamSession
        { RequestId = "model-error", ModelName = "mock-model", Stream = ModelChunks(true) };
        var updates = new List<ChatResponseUpdate>();
        await foreach (var update in ModelDebugChatClient.StreamSessionAsync(session)) updates.Add(update);
        await Assert.That(updates.Last().RawRepresentation is RunErrorEvent).IsTrue();
        await Assert.That(updates.Any(u => u.RawRepresentation is CustomEvent)).IsFalse();
        await Assert.That(updates.Any(u => u.FinishReason is not null)).IsFalse();
    }

    private static async IAsyncEnumerable<global::CoreMod.Models.ModelStreamChunk> ModelChunks(bool error)
    {
        await Task.Yield();
        yield return new() { Delta = "hello" };
        yield return new() { Delta = " " };
        if (error)
        {
            yield return new() { ErrorMessage = "Mock provider error" };
            yield break;
        }
        yield return new() { Delta = "world", IsFinal = true,
            Usage = new() { PromptTokens = 3, CompletionTokens = 4, TotalTokens = 7 } };
    }

    private static StringContent Input() => new(
        """{"threadId":"isolated-test","runId":"test-run","messages":[{"id":"user-1","role":"user","content":"hello"}],"tools":[],"context":[],"state":{},"forwardedProps":{}}""",
        Encoding.UTF8, "application/json");

    private static async Task<WebApplication> StartAsync(IChatClient client)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing" });
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Services.AddAGUIServer();
        builder.Services.ConfigureHttpJsonOptions(options =>
            options.SerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull);
        var app = builder.Build();
        app.MapAGUIServer("/debug", new ChatClientAgent(client, name: "MockDebug"));
        await app.StartAsync();
        return app;
    }

    private static HttpClient Client(WebApplication app) => new()
    {
        BaseAddress = new Uri(app.Services.GetRequiredService<IServer>()
            .Features.Get<IServerAddressesFeature>()!.Addresses.Single())
    };

    private sealed class StubChatClient(bool error = false, bool wait = false) : IChatClient
    {
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Cancelled { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
            IEnumerable<Microsoft.Extensions.AI.ChatMessage> messages, ChatOptions? options = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (error)
            {
                foreach (var update in AgentDebugChatClient.ConvertEvent(new() { Type = "error", Error = "Agent not found" }, true))
                    yield return update;
                yield break;
            }
            foreach (var update in AgentDebugChatClient.ConvertEvent(new()
                { Type = "message", Message = new() { Role = "assistant", Content = "你好 👋" } }, true))
                yield return update;
            if (wait)
            {
                Started.TrySetResult();
                try { await Task.Delay(Timeout.Infinite, cancellationToken); }
                finally { Cancelled.TrySetResult(); }
            }
            foreach (var update in AgentDebugChatClient.ConvertEvent(new()
                { Type = "tool", ToolCall = new() { Name = "lookup", Input = "{\"query\":\"test\"}", Output = new { success = true } } }, true))
                yield return update;
            foreach (var update in AgentDebugChatClient.ConvertEvent(new()
                { Type = "done", Metrics = new() { TotalTokens = 12, ToolCallCount = 1 } }, true))
                yield return update;
        }
        public Task<ChatResponse> GetResponseAsync(IEnumerable<Microsoft.Extensions.AI.ChatMessage> messages,
            ChatOptions? options = null, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public object? GetService(Type serviceType, object? serviceKey = null) => null;
        public void Dispose() { }
    }
}
