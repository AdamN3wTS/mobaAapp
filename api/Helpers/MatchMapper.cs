using System;
using api.DbModels;
using api.Models.Match;

namespace api.Helpers;

public static class MatchMapper
{
    public static Match ToEntity(MatchDto dto) => new()
    {
        MatchId = dto.Metadata.MatchId,
        Patch = string.Join('.', dto.Info.GameVersion.Split('.').Take(2)),   
        DurationSeconds = dto.Info.GameDuration,
        Bans = dto.Info.Teams
            .SelectMany(t => t.Bans.Select(b => new MatchBan
            {
                TeamId = t.TeamId,
                ChampionId = b.ChampionId
            }))
            .Where(b => b.ChampionId != -1)   // -1 = gracz nic nie zbanował
            .ToList(),
        PlayedAt = DateTimeOffset.FromUnixTimeMilliseconds(dto.Info.GameCreation).UtcDateTime,
        Participants = dto.Info.Participants.Select(p => new MatchParticipant
        {
            Puuid = p.Puuid,
            GameName = p.RiotIdGameName,
            TagLine = p.RiotIdTagline,
            ChampionId = p.ChampionId,
            ChampionName = p.ChampionName,
            TeamPosition = p.TeamPosition,
            TeamId = p.TeamId,
            IsWin = p.Win,
            Level = p.ChampLevel,
            Kills = p.Kills,
            Deaths = p.Deaths,
            Assists = p.Assists,
            Cs = p.TotalMinionsKilled + p.NeutralMinionsKilled,
            Spell1Id = p.Summoner1Id,
            Spell2Id = p.Summoner2Id,
            ItemIds = new[] { p.Item0, p.Item1, p.Item2, p.Item3, p.Item4, p.Item5, p.Item6 }
                .Where(id => id != 0).ToList()
        }).ToList()
    };
}
