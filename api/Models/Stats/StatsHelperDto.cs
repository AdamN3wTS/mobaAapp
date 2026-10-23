using System;
using Microsoft.Net.Http.Headers;

namespace api.Models.Stats;

public class StatsHelperDto
{
    public List<StatsPositionHelperDto> StatsPositions {get;set;} = new();
    public List<StatsBansHelperDto> StatsBans {get;set;} = new();
}
public class StatsBansHelperDto
{
    public string ChampionName {get;set;}
    public int ChampionId {get;set;}
    public double BanRate {get;set;}


}
public class StatsPositionHelperDto
{
    public string TeamPosition {get;set;}
    public List<StatsChampionHelperDto> Stats {get;set;} = new();


}
public class StatsChampionHelperDto
{
    public int Games{get;set;}
    public string ChampionName {get;set;}
    public int ChampionId {get;set;}
    public double WinRate {get;set;}
    public double PickRate {get;set;}

}
