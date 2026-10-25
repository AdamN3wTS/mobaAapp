using System.Collections.Generic;
using api.Models.Rank;

namespace api.Models.Account
{
    public class AccountDto
    {
        public string Puuid { get; set; } = string.Empty;
        public string GameName { get; set; } = string.Empty;
        public string TagLine { get; set; } = string.Empty;
    }

    public class SummonerDto
    {
        public string Id { get; set; } = string.Empty;
        public string AccountId { get; set; } = string.Empty;
        public string Puuid { get; set; } = string.Empty;
        public int SummonerLevel { get; set; }
        public int ProfileIconId { get; set; }
    }

    public class FullAccountInfoDto
    {
        public string Puuid { get; set; } = string.Empty;
        public string GameName { get; set; } = string.Empty;
        public string TagLine { get; set; } = string.Empty;
        public int SummonerLevel { get; set; }
        public List<string> RecentMatchIds { get; set; } = new List<string>();
        public List<RankDto?> Ranks {get;set;} = new List<RankDto?>();
    }
}