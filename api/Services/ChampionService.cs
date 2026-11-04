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

    public async Task<ResponseDto<ChampionDto?>> GetChampionStatsAsync(string championName,string TeamPosition, CancellationToken ct)
    {
        var champion = await _dbContext.championStatsInfos.OrderByDescending(p=>p.CreatedAt).FirstOrDefaultAsync(p=>p.ChampionName.ToLower()==championName.ToLower() && p.TeamPosition.ToLower()==TeamPosition.ToLower());
        if (champion == null)
        {
            _logger.LogError("champion==null");
            return new ResponseDto<ChampionDto?>{StatusCode=System.Net.HttpStatusCode.NotFound,IsSuccess=false,Message="Not Found",Data=null};
        }
        var dto = new ChampionDto()
        {
            ChampionId=champion.ChampionId,
            BanRate=champion.BanRate,
            WinRate=champion.WinRate,
            PickRate=champion.PickRate,
            TeamPosition=champion.TeamPosition,
            Games=champion.Games,
            ChampionName=champion.ChampionName,
        };
       var participantsS = await _dbContext.matchParticipants
            .Where(p => p.ChampionId == champion.ChampionId && p.TeamPosition == dto.TeamPosition)
            .Select(p => new { p.ItemIds, p.IsWin, p.Spell1Id, p.Spell2Id })
            .ToListAsync(ct);

        int totalGamesS = participantsS.Count;

        dto.Spells = participantsS
            .GroupBy(p => new { p.Spell1Id, p.Spell2Id })
            .Select(g => new ChampionSpellsDto
            {
                Spell1Id = g.Key.Spell1Id,
                Spell2Id = g.Key.Spell2Id,
                PickRate = totalGamesS == 0 ? 0 : Math.Round(g.Count() * 100.0 / totalGamesS, 2)
            })
            .OrderByDescending(s => s.PickRate)
            .ToList();


        var participants = await _dbContext.matchParticipants
            .Where(p => p.ChampionId == champion.ChampionId && p.TeamPosition == dto.TeamPosition)
            .Select(p => new { p.ItemIds, p.IsWin })
            .ToListAsync();

        int totalGames = participants.Count;

        var items = participants
            .SelectMany(p => p.ItemIds
                .Where(id => id != 0)          
                .Distinct()                    
                .Select(id => new { ItemId = id, p.IsWin }))
            .GroupBy(x => x.ItemId)
            .Select(g => new ChampionItemDto
            {
                ItemId   = g.Key,
                Games    = g.Count(),
                WinRate  = Math.Round(g.Count(x => x.IsWin) * 100.0 / g.Count(), 2),
                PickRate = totalGames == 0 ? 0 : Math.Round(g.Count() * 100.0 / totalGames, 2)
            })
            .OrderByDescending(i => i.PickRate)
            .ToList();

        dto.Items = items;
        return new ResponseDto<ChampionDto?>(){IsSuccess=true,StatusCode=System.Net.HttpStatusCode.OK,Message="Ok",Data=dto};
        
        
    }
}
