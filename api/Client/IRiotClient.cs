using System.Collections.Generic;
using System.Threading.Tasks;
using api.Models.Account;

namespace api.Client
{
    public interface IRiotClient
    {
        Task<AccountDto?> GetAccountByRiotIdAsync(string gameName, string tagLine);
        Task<SummonerDto?> GetSummonerByPuuidAsync(string region, string puuid);
        Task<List<string>> GetRecentMatchIdsAsync(string region, string puuid, int count = 10);
    }
}