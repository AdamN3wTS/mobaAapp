using System;

namespace api.DbModels;

public class ChampionStatsInfo
{
    public Guid Id {get;set;}
    public int ChampionId {get;set;}
    public string ChampionName {get;set;}
    public double WinRate {get;set;}
    public double PickRate {get;set;}
    public double BanRate {get;set;}
    public string TeamPosition {get;set;}
    public DateTime CreatedAt {get;set;}
}
