using System;

namespace api.DbModels;

public class MatchBan
{
    public Guid Id {get;set;} = new Guid();
    public string MatchId {get;set;}
    public Match Match {get;set;}
    public int ChampionId {get;set;}
    public int TeamId {get;set;}
}
