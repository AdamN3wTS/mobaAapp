using System.Collections.Concurrent;

namespace api.Client;

public class RiotRateLimiter
{
    private static readonly (int Limit, TimeSpan Window)[] Limits =
    {
        (18, TimeSpan.FromSeconds(1)),
        (90, TimeSpan.FromMinutes(2))
    };

    private readonly ConcurrentDictionary<string, Bucket> _buckets = new();

    public Task WaitAsync(string region, CancellationToken ct = default) =>
        _buckets.GetOrAdd(region, _ => new Bucket()).WaitAsync(ct);

    private class Bucket
    {
        private readonly SemaphoreSlim _lock = new(1, 1);
        private readonly List<DateTime> _history = new();

        public async Task WaitAsync(CancellationToken ct)
        {
            await _lock.WaitAsync(ct);
            try
            {
                while (true)
                {
                    var now = DateTime.UtcNow;
                    _history.RemoveAll(t => now - t > TimeSpan.FromMinutes(2));

                    var wait = TimeSpan.Zero;
                    foreach (var (limit, window) in Limits)
                    {
                        var inWindow = _history.Where(t => now - t < window).ToList();
                        if (inWindow.Count >= limit)
                        {
                            var w = inWindow[inWindow.Count - limit] + window - now;
                            if (w > wait) wait = w;
                        }
                    }

                    if (wait <= TimeSpan.Zero)
                    {
                        _history.Add(now);
                        return;
                    }

                    await Task.Delay(wait, ct);
                }
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}