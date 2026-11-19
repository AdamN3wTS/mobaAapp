using System;
using api.Client;
using api.Data;
using api.DbModels;
using api.IDbServices;
using Microsoft.EntityFrameworkCore;

namespace api.DbServices;

public class MatchDbService : IMatchDbService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<MatchDbService> _logger;
    public MatchDbService(AppDbContext appDbContext,ILogger<MatchDbService> logger)
    {
        _logger=logger;
        _dbContext=appDbContext;
    }
    public async Task<bool> IsMatchExistByMatchIdAsync(string matchId,CancellationToken ct)
    {
        var match= await _dbContext.matches.FirstOrDefaultAsync(p=>p.MatchId==matchId,ct);
        return match==null ? false :true;
    }
    public async Task<HashSet<string>> GetExistingMatchIdsAsync(List<string> ids, CancellationToken ct) =>
    (await _dbContext.matches
        .Where(m => ids.Contains(m.MatchId))
        .Select(m => m.MatchId)
        .ToListAsync(ct))
    .ToHashSet();
    public async Task<bool> AddMatchAsync(Match match, CancellationToken ct)
    {
        
        await _dbContext.matches.AddAsync(match,ct);            
        await _dbContext.SaveChangesAsync(ct);    
        return true;
        
        
    }
    public async Task DeleteMatchesBeforePatchAsync(string patch, CancellationToken ct)
    {
        var toDelete = await _dbContext.matches.CountAsync(m => m.Patch != patch, ct);
        _logger.LogInformation("Patch do zachowania: {Patch}, meczów do usunięcia: {Count}", patch, toDelete);

        _dbContext.Database.SetCommandTimeout(TimeSpan.FromMinutes(5));

        var total = 0;
        try
        {
            while (true)
            {
                var ids = await _dbContext.matches
                    .Where(m => m.Patch != patch)
                    .Select(m => m.MatchId)
                    .Take(500)
                    .ToListAsync(ct);

                if (ids.Count == 0) break;

                total += await _dbContext.matches
                    .Where(m => ids.Contains(m.MatchId))
                    .ExecuteDeleteAsync(ct);

                _logger.LogInformation("Usunięto {Total}/{All}", total, toDelete);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Błąd przy usuwaniu meczów (usunięto {Total})", total);
            throw;
        }

    }
    public async Task<Match?> GetLatestMatch(CancellationToken ct)
    {
        var match = await _dbContext.matches.OrderByDescending(p=>p.PlayedAt).FirstOrDefaultAsync(ct);
        return match;
    }
    public async Task<long> GetFirstMatchInCurrnetPatch(string riotPatch)
    {
        string? dbPatch = (await GetLatestMatch(new CancellationToken()))?.Patch;
        var latestPatch = new[] { riotPatch, dbPatch }.Where(p => p != null)
            .Select(p => Version.Parse(p!))
            .Max()
            ?.ToString(2);  
        if (latestPatch == null)  return new DateTimeOffset(DateTime.UtcNow.AddDays(-2)).ToUnixTimeSeconds();
        var match = await _dbContext.matches.Where(p=>p.Patch==latestPatch).OrderBy(p=>p.PlayedAt).FirstOrDefaultAsync();
        if (match == null)
        {
            return new DateTimeOffset(DateTime.UtcNow.AddDays(-2)).ToUnixTimeSeconds();
        }
        return new DateTimeOffset(match.PlayedAt).ToUnixTimeSeconds();
    }

    public async Task<int> GetMatchesCountAsync(CancellationToken ct)
    {
        var matchesCount = await _dbContext.matches.CountAsync(ct);
        return matchesCount;
    }
}
