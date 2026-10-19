using System.Collections.Generic;
using System.Threading.Tasks;
using api.Models;
using api.Models.Account;
using api.Models.Rank;
using api.Models.Region;

namespace api.Client
{
    public interface IRiotClient
    {
        Task<ResponseRiotDto<AccountDto?>> GetAccountByRiotIdAsync(string gameName, string tagLine);
        Task<ResponseRiotDto<SummonerDto?>> GetSummonerByPuuidAsync(string region, string puuid);
        Task<ResponseRiotDto<List<string>?>> GetRecentMatchIdsAsync(string region, string puuid, int count = 10);
        Task<ResponseRiotDto<List<RankDto?>>> GetRanksByPuuidAsync(string region, string puuid);
        Task<ResponseRiotDto<RegionDto?>> GetActiveRegionByPuuidAsync(string puuid);

    }
}