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

    public ChampionService(AppDbContext dbContext,ILogger<ChampionService> logger)
    {
        _dbContext=dbContext;
        _logger=logger;
    }

    public async Task<ResponseDto<ChampionDto?>> GetChampionStatsAsync(string championName, string teamPosition, CancellationToken ct)
    {
        var champion = await _dbContext.championStatsInfos
            .AsNoTracking()
            .Include(p => p.Spells)
            .Include(p => p.Items)
            .Include(p => p.RunePages)
            .Include(p => p.Runes)
            .Include(p => p.StatsShards)
            .AsSplitQuery()
            .Where(p => p.ChampionName.ToLower() == championName.ToLower()
                    && p.TeamPosition.ToLower() == teamPosition.ToLower())
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

        var dto = new ChampionDto
        {
            ChampionId = champion.ChampionId,
            ChampionName = champion.ChampionName,
            TeamPosition = champion.TeamPosition,
            Games = champion.Games,
            WinRate = champion.WinRate,
            PickRate = champion.PickRate,
            BanRate = champion.BanRate,

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
