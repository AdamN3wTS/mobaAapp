namespace api.Models.Champion;

public class ChampionSearchDto
{
    public int ChampionId { get; set; }
    public string ChampionName { get; set; } = null!;
    public string MainPosition { get; set; } = null!;
}
