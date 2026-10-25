using System;
using api.Data;
using api.DbModels;
using api.IDbServices;
using Microsoft.EntityFrameworkCore;

namespace api.DbServices;

public class CrawlPlayerDbService : ICrawlPlayerDbService
{
    private readonly ILogger<CrawlPlayerDbService> _logger;
    private readonly AppDbContext _dbContext;
    public CrawlPlayerDbService(AppDbContext appDbContext,ILogger<CrawlPlayerDbService> logger)
    {
        _logger=logger;
        _dbContext=appDbContext;
    }
    public async Task<bool> AddListPlayersAsync(List<CrawlPlayer> crawlPlayers,CancellationToken ct)
    {
        try
        {
            await _dbContext.crawlPlayers.AddRangeAsync(crawlPlayers,ct);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }
        catch(Exception ex)
        {
            return false;
        }

    }
    public async Task<List<CrawlPlayer>> GetPlayersAsync(CancellationToken ct)
    {
        var players = await _dbContext.crawlPlayers.ToListAsync(ct);
        return players;
    }
    public async Task<List<CrawlPlayer>> GetActivePlayersToInsertMatchesAsync(CancellationToken ct)
    {
        var players = await _dbContext.crawlPlayers.OrderBy(p=>p.LastCrawledAt.HasValue).ThenBy(p=>p.LastCrawledAt).Take(10).ToListAsync(ct);
        return players;
    }
    public async Task<List<string>> GetExistingPuuidListAsync(List<string> puuids,CancellationToken ct)
    {
        var puuidsSelected = await _dbContext.crawlPlayers.Where(p=>puuids.Contains(p.Puuid)).Select(p=>p.Puuid).ToListAsync(ct);
        return puuidsSelected;
    }
    public async Task<CrawlPlayer?> IsPlayerExistByPuuidAsync(string puuid,CancellationToken ct)
    {
        var player = await _dbContext.crawlPlayers.FirstOrDefaultAsync(p=>p.Puuid==puuid,ct);
        return player;
    }
    public async Task<bool> SetCrawlTimeByPuuidAsync(string puuid,CancellationToken ct)
    {
        var player = await _dbContext.crawlPlayers.FirstOrDefaultAsync(p=>p.Puuid==puuid,ct);
        player.LastCrawledAt=DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(ct);
        return true;
    }
    public async Task<int> HowManyPlayersWhoDoesntCrowledOrCrowledLogTimeAgoAsync(CancellationToken ct)
    {
        var players = await _dbContext.crawlPlayers.Where(p=>p.LastCrawledAt==null || p.LastCrawledAt<DateTime.UtcNow.AddDays(-7)).ToListAsync(ct);
        
        return players.Count;
    }
    public async Task<bool> AddPlayerAsync(CrawlPlayer crawlPlayer,CancellationToken ct)
    {
        try
        {
            await _dbContext.crawlPlayers.AddAsync(crawlPlayer,ct);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }
        catch(Exception ex)
        {
            return false;
        }

    }
    
}
