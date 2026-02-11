using System.Collections.Concurrent;

namespace Mozart.Workers.Gateway;

public interface IChannelAggregator
{
    void Track(int id);
    void Untrack(int id);

    string Create(ClientSession session, int expected, Func<ChannelAggregator.AggregateResult, Task> callback);
    bool Add(string requestId, ChannelStats stats);
    Task<ChannelAggregator.AggregateResult> Acquire(string requestId);

    void Drop(string requestId);
}

public class ChannelAggregator : IChannelAggregator
{
    public class AggregateResult
    {
        public required ClientSession Session { get; init; }
        public int ExpectedCount { get; set; } = 0;
        public List<ChannelStats> AcquiredStats { get; } = [];
        public Func<AggregateResult, Task> Callback { get; init; } = _ => Task.CompletedTask;
    }

    private readonly ConcurrentDictionary<int, ChannelStats> _stats = [];
    private readonly ConcurrentDictionary<string, AggregateResult> _queue = [];

    public void Track(int id)
    {
        if (_stats.ContainsKey(id))
            throw new InvalidOperationException("Channel id is already tracked");

        _stats[id] = new ChannelStats();
    }

    public void Untrack(int id)
    {
        if (!_stats.TryRemove(id, out _))
            return;

        foreach ((string _, var stats) in _queue)
        {
            if (stats.ExpectedCount > 1)
                stats.ExpectedCount -= 1;

            if (stats.ExpectedCount == stats.AcquiredStats.Count)
                stats.Callback?.Invoke(stats);
        }
    }

    public string Create(ClientSession session, int expected, Func<AggregateResult, Task> callback)
    {
        string requestId = Guid.NewGuid().ToString().ToUpperInvariant();
        if (!_queue.TryAdd(requestId, new AggregateResult() { Session = session, ExpectedCount = expected, Callback = callback}))
            throw new ArgumentOutOfRangeException(nameof(requestId));

        return requestId;
    }

    public bool Add(string requestId, ChannelStats stats)
    {
        if (!_queue.TryGetValue(requestId, out var queue))
            throw new InvalidOperationException("Queue request id is not registered");

        queue.AcquiredStats.Add(stats);
        return queue.AcquiredStats.Count == queue.ExpectedCount;
    }

    public async Task<AggregateResult> Acquire(string requestId)
    {
        if (!_queue.TryGetValue(requestId, out var queue))
            throw new InvalidOperationException("Queue request id is not registered");

        if (queue.AcquiredStats.Count < queue.ExpectedCount)
            throw new InvalidOperationException("Queue is not meet expected count yet");

        var acquired = queue;
        _queue.TryRemove(requestId, out _);

        var task = acquired.Callback?.Invoke(acquired);
        if (task != null)
            await task;

        return acquired;
    }

    public void Drop(string requestId)
    {
        _queue.TryRemove(requestId, out _);
    }
}
