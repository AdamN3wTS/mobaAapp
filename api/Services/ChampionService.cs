using System;
using api.Data;
using api.IServices;
using api.Models;
using api.Models.Champion;
using Microsoft.EntityFrameworkCore;

namespace api.Services;

public class ChampionService : IChampionService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<ChampionService> _logger;
    private const int TrendPreviousPatches = 10;
    public ChampionService(AppDbContext dbContext,ILogger<ChampionService> logger)
    {
        _dbContext=dbContext;
        _logger=logger;
    }

    

    
    private static Version ParsePatch(string? patch)
    {
        var parts = (patch ?? "").Split('.').Take(2);
        return Version.TryParse(string.Join('.', parts), out var v) ? v : new Version(0, 0);
    }

    public async Task<ResponseDto<ChampionDto?>> GetChampionStatsAsync(string championName, string teamPosition, CancellationToken ct)
    {
        var name = championName.ToLower();
        var position = teamPosition.ToLower();

        var baseQuery = _dbContext.championStatsInfos
            .AsNoTracking()
            .Where(p => p.ChampionName.ToLower() == name && p.TeamPosition.ToLower() == position);

        var champion = await baseQuery
            .Include(p => p.Spells)
            .Include(p => p.Items)
            .Include(p => p.RunePages)
            .Include(p => p.Runes)
            .Include(p => p.StatsShards)
            .AsSplitQuery()
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (champion == null)
        {
            _logger.LogWarning("Brak statystyk dla {Champion} / {Position}", championName, teamPosition);
            return new ResponseDto<ChampionDto?>
            {
                StatusCode = System.Net.HttpStatusCode.NotFound,
                IsSuccess = false,
                Message = "Not Found",
                Data = null
            };
        }

        var snapshots = await baseQuery
            .Select(p => new ChampionTrendPointDto
            {
                Patch = p.Patch,
                CreatedAt = p.CreatedAt,
                Games = p.Games,
                WinRate = p.WinRate,
                PickRate = p.PickRate,
                BanRate = p.BanRate
            })
            .ToListAsync(ct);

        var currentPatch = champion.Patch;
        var currentVersion = ParsePatch(currentPatch);

        var currentPatchHistory = snapshots
            .Where(s => s.Patch == currentPatch)
            .OrderBy(s => s.CreatedAt)
            .ToList();

        var previousPatches = snapshots
            .Where(s => s.Patch != currentPatch && ParsePatch(s.Patch) < currentVersion)
            .GroupBy(s => s.Patch)
            .Select(g => g.OrderByDescending(s => s.CreatedAt).First())
            .OrderByDescending(s => ParsePatch(s.Patch))
            .Take(TrendPreviousPatches)
            .OrderBy(s => ParsePatch(s.Patch))
            .ToList();

        var dto = new ChampionDto
        {
            ChampionId = champion.ChampionId,
            ChampionName = champion.ChampionName,
            TeamPosition = champion.TeamPosition,
            Patch = champion.Patch,
            Games = champion.Games,
            WinRate = champion.WinRate,
            PickRate = champion.PickRate,
            BanRate = champion.BanRate,

            Trend = new ChampionTrendDto
            {
                CurrentPatch = currentPatch,
                CurrentPatchHistory = currentPatchHistory,
                PreviousPatches = previousPatches
            },

            Spells = champion.Spells
                .OrderByDescending(s => s.PickRate)
                .Select(s => new ChampionSpellsDto
                {
                    Spell1Id = s.Spell1Id,
                    Spell2Id = s.Spell2Id,
                    PickRate = s.PickRate
                })
                .ToList(),

            Items = champion.Items
                .OrderByDescending(i => i.PickRate)
                .Select(i => new ChampionItemDto
                {
                    ItemId = i.ItemId,
                    Games = i.Games,
                    WinRate = i.WinRate,
                    PickRate = i.PickRate
                })
                .ToList(),

            RunePages = champion.RunePages
                .OrderByDescending(r => r.Games)
                .Select(r => new ChampionRunePageDto
                {
                    KeystoneId = r.KeystoneId,
                    PrimaryStyleId = r.PrimaryStyleId,
                    SubStyleId = r.SubStyleId,
                    Games = r.Games,
                    WinRate = r.WinRate,
                    PickRate = r.PickRate
                })
                .ToList(),

            Runes = champion.Runes
                .OrderByDescending(r => r.Games)
                .Select(r => new ChampionRuneDto
                {
                    PerkId = r.PerkId,
                    Games = r.Games,
                    WinRate = r.WinRate,
                    PickRate = r.PickRate
                })
                .ToList(),

            Shards = champion.StatsShards
                .OrderByDescending(s => s.Games)
                .Select(s => new ChampionShardDto
                {
                    OffensePerk = s.OffensePerk,
                    FlexPerk = s.FlexPerk,
                    DefensePerk = s.DefensePerk,
                    Games = s.Games,
                    WinRate = s.WinRate,
                    PickRate = s.PickRate
                })
                .ToList()
        };

        return new ResponseDto<ChampionDto?>
        {
            IsSuccess = true,
            StatusCode = System.Net.HttpStatusCode.OK,
            Message = "Ok",
            Data = dto
        };
    }
    public async Task<ResponseDto<List<ChampionSearchDto>?>> SearchChampionsAsync(string query, CancellationToken ct)
    {
        var q = (query ?? "").Trim().ToLower();

        if (q.Length == 0)
        {
            return new ResponseDto<List<ChampionSearchDto>?>
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                IsSuccess = false,
                Message = "Query is empty",
                Data = null
            };
        }

        var rows = await _dbContext.championStatsInfos
            .AsNoTracking()
            .Where(p => p.ChampionName.ToLower().Contains(q))
            .Select(p => new { p.ChampionId, p.ChampionName, p.TeamPosition, p.Games })
            .ToListAsync(ct);

        var results = rows
            .GroupBy(r => r.ChampionId)
            .Select(g => new ChampionSearchDto
            {
                ChampionId = g.Key,
                ChampionName = g.First().ChampionName,
                MainPosition = g
                    .GroupBy(r => r.TeamPosition)
                    .OrderByDescending(pg => pg.Sum(r => r.Games))
                    .First().Key
            })
            .OrderByDescending(r => r.ChampionName.ToLower() == q)
            .ThenByDescending(r => r.ChampionName.ToLower().StartsWith(q))
            .ThenBy(r => r.ChampionName)
            .Take(8)
            .ToList();

        return new ResponseDto<List<ChampionSearchDto>?>
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            IsSuccess = true,
            Message = results.Count == 0 ? "No champions found" : "Ok",
            Data = results
        };
    }

    public async Task<ResponseDto<List<ChampionPositionDto>?>> GetChampionPositionsAsync(string championName, CancellationToken ct)
    {
        var positions = await _dbContext.championStatsInfos
            .AsNoTracking()
            .Where(p => p.ChampionName.ToLower() == championName.ToLower())
            .OrderByDescending(p => p.Games)
            .Select(p => new ChampionPositionDto
            {
                TeamPosition = p.TeamPosition,
                Games = p.Games
            })
            .ToListAsync(ct);

        if (positions.Count == 0)
        {
            return new ResponseDto<List<ChampionPositionDto>?>
            {
                StatusCode = System.Net.HttpStatusCode.NotFound,
                IsSuccess = false,
                Message = "Not Found",
                Data = null
            };
        }

        return new ResponseDto<List<ChampionPositionDto>?>
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            IsSuccess = true,
            Message = "Ok",
            Data = positions
        };
    }
}
