using System;

namespace api.Client;

public class RiotRateLimiter
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private DateTime _lastRequest = DateTime.MinValue;
    private static readonly TimeSpan MinInterval = TimeSpan.FromMilliseconds(1250);

    public async Task WaitAsync()
    {
        await _lock.WaitAsync();
        try
        {
            var wait = _lastRequest + MinInterval - DateTime.UtcNow;
            if (wait > TimeSpan.Zero)
                await Task.Delay(wait);
            _lastRequest = DateTime.UtcNow;
        }
        finally
        {
            _lock.Release();
        }
    }
}
