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
        PlayedAt = DateTimeOffset.FromUnixTimeMilliseconds(dto.Info.GameCreation).UtcDateTime,
        Bans = dto.Info.Teams
            .SelectMany(t => t.Bans.Select(b => new MatchBan
            {
                TeamId = t.TeamId,
                ChampionId = b.ChampionId
            }))
            .Where(b => b.ChampionId != -1)   
            .ToList(),
        Participants = dto.Info.Participants.Select(ToParticipant).ToList()
    };

    private static MatchParticipant ToParticipant(MatchParticipantDto p)
    {
        var participant = new MatchParticipant
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
                .Where(id => id != 0)
                .ToList()
        };

        MapPerks(participant, p.Perks);
        return participant;
    }

    private static void MapPerks(MatchParticipant p, PerksDto? perks)
    {
        if (perks == null) return;

        p.OffensePerk = perks.StatPerks?.Offense ?? 0;
        p.FlexPerk    = perks.StatPerks?.Flex ?? 0;
        p.DefensePerk = perks.StatPerks?.Defense ?? 0;

        var primary = perks.Styles?.FirstOrDefault(s => s.Description == "primaryStyle");
        var sub     = perks.Styles?.FirstOrDefault(s => s.Description == "subStyle");
        if (primary == null || sub == null || primary.Selections.Count == 0)
            return;

        p.PrimaryStyleId = primary.Style;
        p.SubStyleId     = sub.Style;
        p.KeystoneId     = primary.Selections[0].Perk;

        byte slot = 0;
        foreach (var (styleId, sel) in primary.Selections.Select(s => (primary.Style, s))
                                      .Concat(sub.Selections.Select(s => (sub.Style, s))))
        {
            p.Runes.Add(new ParticipantRunes
            {
                Slot = slot++,
                StyleId = styleId,
                PerkId = sel.Perk,
                Var1 = sel.Var1,
                Var2 = sel.Var2,
                Var3 = sel.Var3
            });
        }
    }
}