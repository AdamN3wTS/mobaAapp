using System;

namespace api.Client;

public interface IRiotClient
{
    public Task<T?> GetAsync<T>(string url, CancellationToken ct);

}
