using System.Collections.Generic;
using System.Threading.Tasks;
using api.Models;
using api.Models.Account;
using api.Models.Match;
using api.Models.Rank;
using api.Models.Region;

namespace api.Client
{
    public interface IRiotClient
    {
        Task<ResponseRiotDto<AccountDto?>> GetAccountByRiotIdAsync(string gameName, string tagLine,bool IsBcg=false);
        Task<ResponseRiotDto<SummonerDto?>> GetSummonerByPuuidAsync(string region, string puuid,bool IsBcg=false);
        Task<ResponseRiotDto<List<string>?>> GetRecentMatchIdsAsync(string region, string puuid, int count = 10,bool IsBcg=false);
        Task<ResponseRiotDto<List<RankDto?>>> GetRanksByPuuidAsync(string region, string puuid,bool IsBcg=false);
        Task<ResponseRiotDto<RegionDto?>> GetActiveRegionByPuuidAsync(string puuid,bool IsBcg=false);
        Task<ResponseRiotDto<MatchDto>> GetMatchAsync(string matchId,bool IsBcg=false);
        Task<ResponseRiotDto<List<string>?>> GetMatchIdsForCrawlerAsync(string region, string puuid, int count = 20, int daysBack = 14,bool IsBcg=false);


        Task<ResponseRiotDto<List<RankDto>?>> GetSummonersByRegionQueueTierDivisionAsync(string region,string queue,string tier,string division,int page,bool IsBcg=false);


    }
}