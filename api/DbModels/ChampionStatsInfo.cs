using System;

namespace api.DbModels;

public class ChampionStatsInfo
{
    public Guid Id {get;set;}
    public int ChampionId {get;set;}
    public string ChampionName {get;set;}
    public int Games {get;set;} =0;

    public double WinRate {get;set;}
    public double PickRate {get;set;}
    public double BanRate {get;set;}
    public string TeamPosition {get;set;}
    public DateTime CreatedAt {get;set;}
    public List<ChampionStatsSpells> Spells {get;set;} = new();
    public List<ChampionStatsItems> Items {get;set;} = new();
     public List<ChampionStatsRunePage> RunePages {get;set;} = new();
    public List<ChampionStatsRune> Runes {get;set;} = new();
     public List<ChampionStatsShard> StatsShards {get;set;} = new();
}
public class ChampionStatsSpells
{
    public Guid Id {get;set;}
    public ChampionStatsInfo ChampionStatsInfo {get;set;}
    public Guid ChampionStatsInfoId {get;set;}
    public int Spell1Id {get;set;}
    public int Spell2Id {get;set;}
    public double PickRate {get;set;}
}
public class ChampionStatsItems
{
    public Guid Id {get;set;}
    public ChampionStatsInfo ChampionStatsInfo {get;set;}
    public Guid ChampionStatsInfoId {get;set;}
    public int ItemId { get; set; }
    public int Games { get; set; }
    public double WinRate { get; set; }
    public double PickRate { get; set; }
}
public class ChampionStatsRunePage
{
    public Guid Id { get; set; }
    public ChampionStatsInfo ChampionStatsInfo { get; set; } = null!;
    public Guid ChampionStatsInfoId { get; set; }

    public int KeystoneId { get; set; }
    public int PrimaryStyleId { get; set; }
    public int SubStyleId { get; set; }

    public int Games { get; set; }
    public double WinRate { get; set; }
    public double PickRate { get; set; }
}

public class ChampionStatsRune
{
    public Guid Id { get; set; }
    public ChampionStatsInfo ChampionStatsInfo { get; set; } = null!;
    public Guid ChampionStatsInfoId { get; set; }

    public int PerkId { get; set; }

    public int Games { get; set; }
    public double WinRate { get; set; }
    public double PickRate { get; set; }
}

public class ChampionStatsShard
{
    public Guid Id { get; set; }
    public ChampionStatsInfo ChampionStatsInfo { get; set; } = null!;
    public Guid ChampionStatsInfoId { get; set; }

    public int OffensePerk { get; set; }
    public int FlexPerk { get; set; }
    public int DefensePerk { get; set; }

    public int Games { get; set; }
    public double WinRate { get; set; }
    public double PickRate { get; set; }
}