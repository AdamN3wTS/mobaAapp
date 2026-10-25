namespace api.DbModels;

public class MatchParticipant
{
    public Guid Id { get; set; }

    public string MatchId { get; set; } = null!;
    public Match Match { get; set; } = null!;

    public string Puuid { get; set; } = null!;
    public string GameName { get; set; } = null!;
    public string TagLine { get; set; } = null!;

    public int ChampionId { get; set; }
    public string ChampionName { get; set; } = null!;
    public string TeamPosition { get; set; } = null!;
    public int TeamId { get; set; }
    public bool IsWin { get; set; }

    public int Level { get; set; }
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public int Assists { get; set; }
    public int Cs { get; set; }                    

    public int Spell1Id { get; set; }              
    public int Spell2Id { get; set; }             
    public List<int> ItemIds { get; set; } = new(); 
}