using System;

namespace api.DbModels;

public class Match
{
    public string MatchId {get;set;}
    public string Patch {get;set;}
    public int DurationSeconds {get;set;}
    public DateTime PlayedAt {get;set;}
    public List<MatchParticipant> Participants {get;set;} = new();
    public List<MatchBan> Bans {get;set;} = new();
}
