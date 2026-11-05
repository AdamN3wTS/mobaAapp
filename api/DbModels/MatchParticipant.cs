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
    public int PrimaryStyleId { get; set; }
    public int SubStyleId { get; set; }
    public int KeystoneId { get; set; }

    // Odłamki
    public int OffensePerk { get; set; }
    public int FlexPerk { get; set; }
    public int DefensePerk { get; set; }

    // Wszystkie 6 run
    public List<ParticipantRunes> Runes { get; set; } = new();

}

