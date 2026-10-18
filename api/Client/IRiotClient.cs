using System;
using api.Models;

namespace api.Client;

public interface IRiotClient
{
    public Task<ResponseRiotClientDto<T?>> GetAsync<T>(string url, CancellationToken ct);

}
