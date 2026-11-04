using System;

namespace api.Models.Champion;

public class ChampionDto
{
    public int ChampionId {get;set;}
    public string ChampionName {get;set;}
    public int Games {get;set;}
    public double WinRate {get;set;}
    public double PickRate {get;set;}
    public double BanRate {get;set;}
    public string TeamPosition {get;set;}
    public List<ChampionSpellsDto> Spells {get;set;} = new();
    public List<ChampionItemDto> Items {get;set;} = new();


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