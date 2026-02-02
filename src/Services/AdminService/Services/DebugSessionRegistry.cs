using System.Collections.Concurrent;

namespace AdminService.Services;

public sealed class DebugSessionRegistry
{
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _sessions = new();

    public CancellationTokenSource Create(string requestId)
    {
        var cts = new CancellationTokenSource();
        _sessions[requestId] = cts;
        return cts;
    }

    public bool TryCancel(string requestId)
    {
        if (_sessions.TryRemove(requestId, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
            return true;
        }

        return false;
    }

    public void Remove(string requestId)
    {
        if (_sessions.TryRemove(requestId, out var cts))
        {
            cts.Dispose();
        }
    }
}
