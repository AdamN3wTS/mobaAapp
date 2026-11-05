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
            .Where(s => s.Games >= 80 && s.CreatedAt == latest)   
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

        var totalMatches = await _dbContext.matches.CountAsync(ct);
        _logger.LogInformation("Liczba meczów: {TotalMatches}", totalMatches);

        if (totalMatches == 0)
        {
            _logger.LogWarning("Brak meczów w bazie, pomijam generowanie statystyk");
            return;
        }

        var participants = await _dbContext.matchParticipants
            .AsNoTracking()
            .Where(p => p.TeamPosition != "")
            .Select(p => new
            {
                p.ChampionId,
                p.ChampionName,
                p.TeamPosition,
                p.IsWin,
                p.Spell1Id,
                p.Spell2Id,
                p.ItemIds,
                p.KeystoneId,
                p.PrimaryStyleId,
                p.SubStyleId,
                p.OffensePerk,
                p.FlexPerk,
                p.DefensePerk,
                RunePerkIds = p.Runes.Select(r => r.PerkId).ToList()
            })
            .ToListAsync(ct);

        _logger.LogInformation("Pobrano {Count} uczestników", participants.Count);

        var bans = await _dbContext.matchBans
            .Where(p => p.ChampionId != -1)
            .GroupBy(p => p.ChampionId)
            .Select(g => new
            {
                ChampionId = g.Key,
                Bans = g.Select(b => b.MatchId).Distinct().Count()
            })
            .ToDictionaryAsync(x => x.ChampionId, x => x.Bans, ct);

        _logger.LogInformation("Zbanowanych championów: {Count}", bans.Count);

        var now = DateTime.UtcNow;
        var statsList = new List<ChampionStatsInfo>();

        foreach (var group in participants.GroupBy(p => new { p.ChampionId, p.TeamPosition }))
        {
            var games = group.Count();
            var wins = group.Count(x => x.IsWin);

            var statsInfo = new ChampionStatsInfo
            {
                Id = Guid.NewGuid(),
                ChampionId = group.Key.ChampionId,
                ChampionName = group.First().ChampionName,
                TeamPosition = group.Key.TeamPosition,
                Games = games,
                WinRate = (double)wins / games,
                PickRate = (double)games / totalMatches,
                BanRate = (double)bans.GetValueOrDefault(group.Key.ChampionId) / totalMatches,
                CreatedAt = now
                
            };

            // Spelle
            statsInfo.Spells = group
                .GroupBy(p => new
                {
                    A = Math.Min(p.Spell1Id, p.Spell2Id),
                    B = Math.Max(p.Spell1Id, p.Spell2Id)
                })
                .Select(g => new ChampionStatsSpells
                {
                    Id = Guid.NewGuid(),
                    Spell1Id = g.Key.A,
                    Spell2Id = g.Key.B,
                    PickRate = Math.Round(g.Count() * 100.0 / games, 2)
                })
                .OrderByDescending(s => s.PickRate)
                .ToList();

            // Itemy
            statsInfo.Items = group
                .SelectMany(p => p.ItemIds
                    .Where(id => id != 0)
                    .Distinct()
                    .Select(id => new { ItemId = id, p.IsWin }))
                .GroupBy(x => x.ItemId)
                .Select(g => new ChampionStatsItems
                {
                    Id = Guid.NewGuid(),
                    ItemId = g.Key,
                    Games = g.Count(),
                    WinRate = Math.Round(g.Count(x => x.IsWin) * 100.0 / g.Count(), 2),
                    PickRate = Math.Round(g.Count() * 100.0 / games, 2)
                })
                .OrderByDescending(i => i.PickRate)
                .ToList();
            var withRunes = group.Where(p => p.KeystoneId != 0).ToList();
            var runeGames = withRunes.Count;

            if (runeGames > 0)
            {
                // Strony run
                statsInfo.RunePages = withRunes
                    .GroupBy(p => new { p.KeystoneId, p.PrimaryStyleId, p.SubStyleId })
                    .Select(g => new ChampionStatsRunePage
                    {
                        Id = Guid.NewGuid(),
                        KeystoneId = g.Key.KeystoneId,
                        PrimaryStyleId = g.Key.PrimaryStyleId,
                        SubStyleId = g.Key.SubStyleId,
                        Games = g.Count(),
                        WinRate = Math.Round(g.Count(x => x.IsWin) * 100.0 / g.Count(), 2),
                        PickRate = Math.Round(g.Count() * 100.0 / runeGames, 2)
                    })
                    .OrderByDescending(r => r.Games)
                    .Take(10)
                    .ToList();

                // Pojedyncze runy
                statsInfo.Runes = withRunes
                    .SelectMany(p => p.RunePerkIds.Select(perkId => new { PerkId = perkId, p.IsWin }))
                    .GroupBy(x => x.PerkId)
                    .Select(g => new ChampionStatsRune
                    {
                        Id = Guid.NewGuid(),
                        PerkId = g.Key,
                        Games = g.Count(),
                        WinRate = Math.Round(g.Count(x => x.IsWin) * 100.0 / g.Count(), 2),
                        PickRate = Math.Round(g.Count() * 100.0 / runeGames, 2)
                    })
                    .OrderByDescending(r => r.Games)
                    .ToList();

                // Odłamki
                statsInfo.StatsShards = withRunes
                    .GroupBy(p => new { p.OffensePerk, p.FlexPerk, p.DefensePerk })
                    .Select(g => new ChampionStatsShard
                    {
                        Id = Guid.NewGuid(),
                        OffensePerk = g.Key.OffensePerk,
                        FlexPerk = g.Key.FlexPerk,
                        DefensePerk = g.Key.DefensePerk,
                        Games = g.Count(),
                        WinRate = Math.Round(g.Count(x => x.IsWin) * 100.0 / g.Count(), 2),
                        PickRate = Math.Round(g.Count() * 100.0 / runeGames, 2)
                    })
                    .OrderByDescending(s => s.Games)
                    .Take(5)
                    .ToList();
            }
            statsList.Add(statsInfo);
        }

        _logger.LogInformation("Przygotowano {Count} wierszy statystyk", statsList.Count);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);

        await _dbContext.championStatsInfos.AddRangeAsync(statsList, ct);
        await _dbContext.SaveChangesAsync(ct);

        var deleted = await _dbContext.championStatsInfos
            .Where(s => s.CreatedAt < now)
            .ExecuteDeleteAsync(ct);

        await transaction.CommitAsync(ct);

        _logger.LogInformation("Statystyki zapisane, usunięto {Deleted} starych wpisów", deleted);
    }
}