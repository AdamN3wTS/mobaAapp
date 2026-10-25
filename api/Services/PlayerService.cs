using System;
using api.Client;
using api.IServices;
using api.Models;
using Api.DTOs;

namespace api.Services;

public class PlayerService : IPlayerService
{
    private readonly IRiotClient _riotClient;
    public PlayerService(IRiotClient riotClient)
    {
        _riotClient=riotClient;
    }

    public async Task<ResponseDto<PlayerProfileDTO?>> GetProfileAsync(string gameName, string tagLine)
    {
        var playerRiot = await _riotClient.GetAccountByRiotIdAsync(gameName,tagLine);
        if (playerRiot.StatusCode != System.Net.HttpStatusCode.OK)
        {
            return new ResponseDto<PlayerProfileDTO?>(){Data=null,Message="Not Found",StatusCode=playerRiot.StatusCode,IsSuccess=false};
        }
        var summoner = await _riotClient.GetSummonerByPuuidAsync("eun1",playerRiot.Data.Puuid);
        if (summoner.StatusCode != System.Net.HttpStatusCode.OK)
        {
            return new ResponseDto<PlayerProfileDTO?>(){Data=null,Message="Not Found",StatusCode=summoner.StatusCode,IsSuccess=false};
        }
        var ranks = await _riotClient.GetRanksByPuuidAsync("eun1",summoner.Data.Puuid);
        if (ranks.StatusCode != System.Net.HttpStatusCode.OK)
        {
            return new ResponseDto<PlayerProfileDTO?>(){Data=null,Message="Not Found",StatusCode=ranks.StatusCode,IsSuccess=false};
        }
        var ranksDto = ranks.Data.Select(p=> new RankedInfoDTO{LeaguePoints=p.LeaguePoints,HotStreak=p.HotStreak,Wins=p.Wins,Losses=p.Losses,Rank=p.Rank,Tier=p.Tier,QueueType=p.QueueType}).ToList();
        
        var dto = new PlayerProfileDTO()
        {
            Puuid=summoner.Data.Puuid,
            ProfileIconId=summoner.Data.ProfileIconId,
            SummonerLevel=summoner.Data.SummonerLevel,
            GameName=gameName,
            TagLine=tagLine,
            Ranks=ranksDto
        };
        return new ResponseDto<PlayerProfileDTO?>(){Message="success",IsSuccess=true,StatusCode=System.Net.HttpStatusCode.OK,Data=dto};
        
    }
    public async Task<ResponseDto<PaginatedMatchHistoryDTO?>> GetMatchHistoryAsync(string puuid, int page, int pageSize)
    {
        int skip = (page - 1) * pageSize;

        if (skip >= 100)
        {
            return new ResponseDto<PaginatedMatchHistoryDTO?>
            {
                IsSuccess = true,
                StatusCode = System.Net.HttpStatusCode.OK,
                Message = "success",
                Data = new PaginatedMatchHistoryDTO { Page = page, PageSize = pageSize, TotalItems = 0, Matches = new List<MatchSummaryDTO>() }
            };
        }

        int count = Math.Min(skip + pageSize, 100);

        var idsResponse = await _riotClient.GetRecentMatchIdsAsync("europe", puuid, count);
        if (idsResponse.StatusCode != System.Net.HttpStatusCode.OK || idsResponse.Data == null)
        {
            return new ResponseDto<PaginatedMatchHistoryDTO?>() { Data = null, Message = "Not Found", StatusCode = idsResponse.StatusCode, IsSuccess = false };
        }

        var pageIds = idsResponse.Data.Skip(skip).Take(pageSize).ToList();

        var matchResponses = await Task.WhenAll(pageIds.Select(id => _riotClient.GetMatchAsync(id)));

        var matches = matchResponses
            .Where(r => r.StatusCode == System.Net.HttpStatusCode.OK && r.Data != null)
            .Select(r => r.Data!)
            .Select(m =>
            {
                var p = m.Info.Participants.FirstOrDefault(x => x.Puuid == puuid);
                if (p == null) return null;

                return new MatchSummaryDTO
                {
                    MatchId = m.Metadata.MatchId,
                    IsWin = p.Win,
                    GameDurationInSeconds = (int)m.Info.GameDuration,
                    QueueType = MapQueue(m.Info.QueueId),
                    LpChange = 0, 
                    GameCreationTimestamp = DateTimeOffset.FromUnixTimeMilliseconds(m.Info.GameCreation),
                    ChampionName = p.ChampionName,
                    ChampionId = p.ChampionId,
                    Stats = new MatchPlayerStatsDTO
                    {
                        Kills = p.Kills,
                        Deaths = p.Deaths,
                        Assists = p.Assists,
                        TotalMinionsKilled = p.TotalMinionsKilled + p.NeutralMinionsKilled,
                    }
                };
            })
            .Where(x => x != null)
            .Select(x => x!)
            .OrderByDescending(x => x.GameCreationTimestamp)
            .ToList();

        var dto = new PaginatedMatchHistoryDTO
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = idsResponse.Data.Count, 
            Matches = matches
        };

        return new ResponseDto<PaginatedMatchHistoryDTO?>() { Message = "success", IsSuccess = true, StatusCode = System.Net.HttpStatusCode.OK, Data = dto };
    }

    private static string MapQueue(int queueId) => queueId switch
    {
        420 => "Ranked Solo",
        440 => "Ranked Flex",
        400 => "Normal Draft",
        430 => "Normal Blind",
        450 => "ARAM",
        490 => "Quickplay",
        1700 => "Arena",
        _ => "Other"
    };
}
