using System;
using System.Security.Cryptography.X509Certificates;
using api.Models.Rank;

namespace api.Helpers;

public static class RandomRankGenerator
{
    private static readonly string[] Divisions = ["I", "II", "III", "IV"];
    private static readonly string[] Tiers = ["IRON", "BRONZE", "SILVER", "GOLD","PLATINUM","EMERALD","DIAMOND"];

    public static RandomRankDto GenerateRandomRank()
    {
        Random rnd = new Random();
        var diviosnNumber = rnd.Next(0,Divisions.Length);
        var tiersNumber = rnd.Next(0,Tiers.Length);

        var random = new RandomRankDto();
        random.Division=Divisions[diviosnNumber];
        random.Tier=Tiers[tiersNumber];
        return random;
    }
}
