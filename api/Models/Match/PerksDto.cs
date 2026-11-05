using System;
using System.Text.Json.Serialization;

namespace api.Models.Match;



public class PerksDto
{
    [JsonPropertyName("statPerks")]
    public StatPerksDto StatPerks { get; set; } = new();

    [JsonPropertyName("styles")]
    public List<PerkStyleDto> Styles { get; set; } = new();
}

public class StatPerksDto
{
    [JsonPropertyName("offense")] public int Offense { get; set; }
    [JsonPropertyName("flex")]    public int Flex { get; set; }
    [JsonPropertyName("defense")] public int Defense { get; set; }
}

public class PerkStyleDto
{
    [JsonPropertyName("description")] public string Description { get; set; } = "";
    [JsonPropertyName("style")]       public int Style { get; set; }
    [JsonPropertyName("selections")]  public List<PerkSelectionDto> Selections { get; set; } = new();
}

public class PerkSelectionDto
{
    [JsonPropertyName("perk")] public int Perk { get; set; }
    [JsonPropertyName("var1")] public int Var1 { get; set; }
    [JsonPropertyName("var2")] public int Var2 { get; set; }
    [JsonPropertyName("var3")] public int Var3 { get; set; }
}
