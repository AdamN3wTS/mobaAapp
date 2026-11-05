using System;

namespace api.DbModels;

public class ParticipantRunes
{
    public Guid Id {get;set;}
    public Guid MatchParticipantId { get; set; }
    public MatchParticipant MatchParticipant { get; set; } = null!;

    public byte Slot { get; set; }     
    public int StyleId { get; set; }
    public int PerkId { get; set; }

    public int Var1 { get; set; }
    public int Var2 { get; set; }
    public int Var3 { get; set; }   
}
