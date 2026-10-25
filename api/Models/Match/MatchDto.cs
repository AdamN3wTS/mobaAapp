using System;

namespace api.Models.Match;
public class MatchDto
{
    public MatchMetadataDto Metadata { get; set; } = null!;
    public MatchInfoDto Info { get; set; } = null!;
}

public class MatchMetadataDto
{
    public string MatchId { get; set; } = null!;
}

public class MatchInfoDto
{
    public long GameCreation { get; set; }      
    public int GameDuration { get; set; }       
    public string GameVersion { get; set; } = null!;
    public int QueueId { get; set; }
    public List<MatchParticipantDto> Participants { get; set; } = new();
    public List<MatchTeamDto> Teams { get; set; } = new();
}

public class MatchParticipantDto
{
    public string Puuid { get; set; } = null!;
    public string RiotIdGameName { get; set; } = null!;
    public string RiotIdTagline { get; set; } = null!;

    public int ChampionId { get; set; }
    public string ChampionName { get; set; } = null!;
    public string TeamPosition { get; set; } = null!;
    public int TeamId { get; set; }
    public bool Win { get; set; }

    public int ChampLevel { get; set; }
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public int Assists { get; set; }
    public int TotalMinionsKilled { get; set; }
    public int NeutralMinionsKilled { get; set; }

    public int Summoner1Id { get; set; }
    public int Summoner2Id { get; set; }

    public int Item0 { get; set; }
    public int Item1 { get; set; }
    public int Item2 { get; set; }
    public int Item3 { get; set; }
    public int Item4 { get; set; }
    public int Item5 { get; set; }
    public int Item6 { get; set; }

    public bool GameEndedInEarlySurrender { get; set; }
}

public class MatchTeamDto
{
    public int TeamId { get; set; }
    public bool Win { get; set; }
    public List<MatchBanDto> Bans { get; set; } = new();
}

public class MatchBanDto
{
    public int ChampionId { get; set; }
    public int PickTurn { get; set; }
}
