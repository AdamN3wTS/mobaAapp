using System;
using api.Models;
using api.Models.Stats;

namespace api.IServices;

public interface IStatsService
{
    public Task GenerateStats(CancellationToken ct);
    public Task<ResponseDto<StatsHelperDto>> GetRecentStatsAsync(CancellationToken ct);

}
