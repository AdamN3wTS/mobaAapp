using System;
using System.Net;
using api.Data;
using api.DbModels;
using api.IServices;
using api.Models;
using api.Models.Stats;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace api.Services;

public class StatsService : IStatsService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<StatsService> _logger;

    public StatsService(AppDbContext appDbContext, ILogger<StatsService> logger)
    {
        _dbContext = appDbContext;
        _logger = logger;
    }
    public async Task<ResponseDto<StatsHelperDto>> GetRecentStatsAsync(CancellationToken ct)
    {
        var latest = await _dbContext.championStatsInfos
            .MaxAsync(p => (DateTime?)p.CreatedAt, ct);

        var banRateStats = await _dbContext.championStatsInfos
            .Where(p => p.CreatedAt == latest)
            .GroupBy(p => new { p.ChampionId, p.ChampionName })
            .Select(g => new StatsBansHelperDto
            {
                ChampionId = g.Key.ChampionId,
                ChampionName = g.Key.ChampionName,
                BanRate = g.Max(x => x.BanRate)   
            })
            .OrderByDescending(x => x.BanRate)
            .ToListAsync(ct);
        var rows = await _dbContext.championStatsInfos
            .Where(s => s.Games >= 2 && s.CreatedAt == latest)   
            .Select(s => new { s.TeamPosition, s.ChampionId,s.ChampionName, s.WinRate, s.PickRate,s.Games })
            .ToListAsync(ct);

        string[] order = ["TOP", "JUNGLE", "MIDDLE", "BOTTOM", "UTILITY"];

        var result = rows
            .GroupBy(r => r.TeamPosition)
            .OrderBy(g => Array.IndexOf(order, g.Key))
            .Select(g => new StatsPositionHelperDto
            {
                TeamPosition = g.Key,
                Stats = g
                    .OrderByDescending(r => r.WinRate)
                    .Select(r => new StatsChampionHelperDto
                    {
                        ChampionName= r.ChampionName,
                        ChampionId = r.ChampionId,
                        WinRate = r.WinRate,
                        PickRate = r.PickRate,
                        Games=r.Games
                    })
                    .ToList()
            })
            .ToList();
        var finalStats = new StatsHelperDto()
        {
            StatsBans=banRateStats,
            StatsPositions=result
        };
        return new ResponseDto<StatsHelperDto>(){
            IsSuccess = true,
            StatusCode = HttpStatusCode.OK,
            Data = finalStats
        };
    }
        
    public async Task GenerateStats(CancellationToken ct)
    {
        _logger.LogInformation("Start generowania statystyk");

        var basicStats = await _dbContext.matchParticipants.Where(p => p.TeamPosition != "")
            .GroupBy(p => new { p.ChampionId, p.TeamPosition, p.ChampionName })
            .Select(g => new
            {
                ChampionId = g.Key.ChampionId,
                TeamPosition = g.Key.TeamPosition,
                ChampionName = g.Key.ChampionName,
                Games = g.Count(),
                Wins = g.Count(x => x.IsWin == true)
            }).ToListAsync(ct);
        _logger.LogInformation("Zgrupowano {Count} kombinacji champion/rola", basicStats.Count);

        var totalMatches = await _dbContext.matches.CountAsync(ct);
        _logger.LogInformation("Liczba meczów: {TotalMatches}", totalMatches);

        if (totalMatches == 0)
        {
            _logger.LogWarning("Brak meczów w bazie, pomijam generowanie statystyk");
            return;
        }

        var bans = await _dbContext.matchBans.Where(p => p.ChampionId != -1)
            .GroupBy(p => new { p.ChampionId }).Select(g => new
            {
                ChampionId = g.Key.ChampionId,
                Bans = g.Select(b => b.MatchId).Distinct().Count()
            }).ToDictionaryAsync(x => x.ChampionId, x => x.Bans, ct);
        _logger.LogInformation("Zbanowanych championów: {Count}", bans.Count);

        var statsList = new List<ChampionStatsInfo>();
        var now = DateTime.UtcNow;
        foreach (var item in basicStats)
        {
            var banRate = (double)bans.GetValueOrDefault(item.ChampionId) / totalMatches;
            var winRate = (double)item.Wins / item.Games;
            var PickRate = (double)item.Games / totalMatches;
            var statsInfo = new ChampionStatsInfo()
            {
                Id = Guid.NewGuid(),
                BanRate = banRate,
                PickRate = PickRate,
                WinRate = winRate,
                TeamPosition = item.TeamPosition,
                ChampionId = item.ChampionId,
                ChampionName = item.ChampionName,
                Games=item.Games,
                CreatedAt =now
            };
            statsList.Add(statsInfo);
        }
        _logger.LogInformation("Przygotowano {Count} wierszy statystyk", statsList.Count);

        await _dbContext.championStatsInfos.AddRangeAsync(statsList, ct);
        await _dbContext.SaveChangesAsync(ct);
        _logger.LogInformation("Statystyki zapisane");
    }
}