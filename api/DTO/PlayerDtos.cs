namespace Api.DTOs
{
    // --- Model dla Profilu Gracza ---
    public class RankedInfoDTO
    {
        public string QueueType { get; set; } = string.Empty;
        public string Tier { get; set; } = string.Empty;
        public string Rank { get; set; } = string.Empty;
        public int LeaguePoints { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public double WinRate => (Wins + Losses) > 0 ? Math.Round((double)Wins / (Wins + Losses) * 100, 2) : 0;
        public bool HotStreak { get; set; }
    }

    public class PlayerProfileDTO
    {
        public string Puuid { get; set; } = string.Empty;
        public string GameName { get; set; } = string.Empty;
        public string TagLine { get; set; } = string.Empty;
        public int ProfileIconId { get; set; }
        public long SummonerLevel { get; set; }
        public List<RankedInfoDTO> Ranks { get; set; } = new();
    }

    // --- Model dla Historii Meczów ---
    public class MatchPlayerStatsDTO
    {
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Assists { get; set; }
        public double Kda => Deaths > 0 ? Math.Round((double)(Kills + Assists) / Deaths, 2) : (Kills + Assists);
        public int TotalMinionsKilled { get; set; }
        public int DamageDealtToChampions { get; set; }
        public int VisionScore { get; set; }
    }

    public class MatchSummaryDTO
    {
        public string MatchId { get; set; } = string.Empty;
        public bool IsWin { get; set; }
        public long GameDurationInSeconds { get; set; }
        public string QueueType { get; set; } = string.Empty;
        public int LpChange { get; set; }
        public DateTimeOffset GameCreationTimestamp { get; set; }
        public string ChampionName { get; set; } = string.Empty;
        public int ChampionId { get; set; }
        public MatchPlayerStatsDTO Stats { get; set; } = new();
    }

    public class PaginatedMatchHistoryDTO
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public List<MatchSummaryDTO> Matches { get; set; } = new();
    }
}