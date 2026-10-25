using System;

namespace api.Models.Rank;

public class RankDto
{
    public string Puuid {get;set;}
    public string QueueType {get;set;}
    public string Tier {get;set;}
    public string Rank {get;set;}
    public int LeaguePoints {get;set;}
    public int Wins {get;set;}
    public int Losses {get;set;}
    public bool Veteran {get;set;}
    public bool Inactive {get;set;}
    public bool FreshBlood {get;set;}
    public bool HotStreak {get;set;}
}
