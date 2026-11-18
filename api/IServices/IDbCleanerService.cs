using System;

namespace api.IServices;

public interface IDbCleanerService
{
    public Task CleanDatabase(CancellationToken ct);
}
