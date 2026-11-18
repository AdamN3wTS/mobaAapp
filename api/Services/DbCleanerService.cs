using System;
using api.Client;
using api.IDbServices;
using api.IServices;

namespace api.Services;

public class DbCleanerService : IDbCleanerService
{
    private readonly IMatchDbService _matchDbService;
    private readonly IRiotClient _riotClient;
    private readonly ILogger<DbCleanerService> _logger;
    public DbCleanerService(IMatchDbService matchDbService,IRiotClient riotClient,ILogger<DbCleanerService> logger)
    {
        _matchDbService=matchDbService;
        _riotClient=riotClient;
        _logger=logger;
    }
    public async Task CleanDatabase(CancellationToken ct)
    {
        
        string? riotPatch = await _riotClient.GetCurrentVersionAsync();
        string? dbPatch = (await _matchDbService.GetLatestMatch(ct))?.Patch;
        var latestPatch = new[] { riotPatch, dbPatch }.Where(p => p != null)
            .Select(p => Version.Parse(p!))
            .Max()
            ?.ToString(2);  
        if (latestPatch == null) return;
        await _matchDbService.DeleteMatchesBeforePatchAsync(latestPatch,ct);
        
        

    }
}
