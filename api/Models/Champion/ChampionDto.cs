using System;

namespace api.Models.Champion;

public class ChampionDto
{
    public int ChampionId { get; set; }
    public string ChampionName { get; set; } = null!;
    public int Games { get; set; }
    public double WinRate { get; set; }
    public double PickRate { get; set; }
    public double BanRate { get; set; }
    public string TeamPosition { get; set; } = null!;
    public List<ChampionSpellsDto> Spells { get; set; } = new();
    public List<ChampionItemDto> Items { get; set; } = new();
    public List<ChampionRunePageDto> RunePages { get; set; } = new();
    public List<ChampionRuneDto> Runes { get; set; } = new();
    public List<ChampionShardDto> Shards { get; set; } = new();
}

public class ChampionRunePageDto
{
    public int KeystoneId { get; set; }
    public int PrimaryStyleId { get; set; }
    public int SubStyleId { get; set; }
    public int Games { get; set; }
    public double WinRate { get; set; }
    public double PickRate { get; set; }
}

public class ChampionRuneDto
{
    public int PerkId { get; set; }
    public int Games { get; set; }
    public double WinRate { get; set; }
    public double PickRate { get; set; }
}

public class ChampionShardDto
{
    public int OffensePerk { get; set; }
    public int FlexPerk { get; set; }
    public int DefensePerk { get; set; }
    public int Games { get; set; }
    public double WinRate { get; set; }
    public double PickRate { get; set; }
}
public class ChampionSpellsDto
{
    public int Spell1Id {get;set;}
    public int Spell2Id {get;set;}
    public double PickRate {get;set;}
}
public class ChampionItemDto
{
    public int ItemId { get; set; }
    public int Games { get; set; }
    public double WinRate { get; set; }
    public double PickRate { get; set; }
}
public class ChampionPositionDto
{
    public string TeamPosition { get; set; } = null!;
    public int Games { get; set; }
}