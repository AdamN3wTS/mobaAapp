using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using api.Models;
using api.Models.Account;
using api.Models.Match;
using api.Models.Rank;
using api.Models.Region;
using Microsoft.Extensions.Configuration;

namespace api.Client
{
    public class RiotClient : IRiotClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly RiotRateLimiter _rateLimiter;
        public RiotClient(HttpClient httpClient, IConfiguration configuration,RiotRateLimiter riotRateLimiter)
        {
            _httpClient = httpClient;
            // Pobieranie ze zmiennych środowiskowych DotNetEnv / Systemu lub appsettings
            _apiKey = Environment.GetEnvironmentVariable("RIOT_API_KEY") 
                      ?? configuration["RIOT_API_KEY"] 
                      ?? string.Empty;
            _httpClient.DefaultRequestHeaders.Add("X-Riot-Token",_apiKey);
            _rateLimiter=riotRateLimiter;

            
        }

        private async Task<ResponseRiotDto<T?>> GetAsync<T>(string url, bool IsBcg = false)
        {
            const int maxAttempts = 3;

            for (int attempt = 1; ; attempt++)
            {
                if (IsBcg)
                {
                    await _rateLimiter.WaitAsync();
                }

                using var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return new ResponseRiotDto<T?>
                    {
                        StatusCode = response.StatusCode,
                        Data = await response.Content.ReadFromJsonAsync<T>()
                    };
                }

                var code = (int)response.StatusCode;
                var wait = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(attempt * 2);

                bool retryable = code == 429 || code >= 500;
                bool tooLongForUser = !IsBcg && wait > TimeSpan.FromSeconds(2);

                if (!retryable || attempt >= maxAttempts || tooLongForUser)
                {
                    Console.WriteLine($"[RIOT] {code} {url.Split('?')[0]} (próba {attempt}) {await response.Content.ReadAsStringAsync()}");
                    return new ResponseRiotDto<T?> { StatusCode = response.StatusCode };
                }

                Console.WriteLine($"[RIOT] {code} {url.Split('?')[0]} – ponawiam za {wait.TotalSeconds}s (próba {attempt}/{maxAttempts})");
                await Task.Delay(wait);
            }
        }

        public Task<ResponseRiotDto<AccountDto?>> GetAccountByRiotIdAsync(string gameName, string tagLine,bool IsBcg=false) =>
            GetAsync<AccountDto>($"https://europe.api.riotgames.com/riot/account/v1/accounts/by-riot-id/{Uri.EscapeDataString(gameName)}/{Uri.EscapeDataString(tagLine)}");

        public Task<ResponseRiotDto<SummonerDto?>> GetSummonerByPuuidAsync(string platform, string puuid,bool IsBcg=false) =>
            GetAsync<SummonerDto>($"https://{platform}.api.riotgames.com/lol/summoner/v4/summoners/by-puuid/{puuid}");

        public Task<ResponseRiotDto<List<string>?>> GetRecentMatchIdsAsync(string region, string puuid, int count = 10,bool IsBcg=false) =>
            GetAsync<List<string>>($"https://{region}.api.riotgames.com/lol/match/v5/matches/by-puuid/{puuid}/ids?start=0&count={count}");
        public Task<ResponseRiotDto<List<string>?>> GetMatchIdsForCrawlerAsync(string region, string puuid, int count = 20, int daysBack = 14,bool IsBcg=false)
        {
            var startTime = DateTimeOffset.UtcNow.AddDays(-daysBack).ToUnixTimeSeconds();

            return GetAsync<List<string>>(
                $"https://{region}.api.riotgames.com/lol/match/v5/matches/by-puuid/{puuid}/ids" +
                $"?queue=420&startTime={startTime}&start=0&count={count}");
        }
        public Task<ResponseRiotDto<List<RankDto?>>> GetRanksByPuuidAsync(string region, string puuid,bool IsBcg=false) =>
            GetAsync<List<RankDto?>>($"https://{region}.api.riotgames.com/lol/league/v4/entries/by-puuid/{puuid}");
        public Task<ResponseRiotDto<RegionDto?>> GetActiveRegionByPuuidAsync(string puuid,bool IsBcg=false) =>
            GetAsync<RegionDto?>($"https://europe.api.riotgames.com/riot/account/v1/region/by-game/lol/by-puuid/{puuid}");
         public Task<ResponseRiotDto<List<RankDto>?>> GetSummonersByRegionQueueTierDivisionAsync(string region,string queue,string tier,string division,int page,bool IsBcg=false) =>
            GetAsync<List<RankDto>?>($"https://{region}.api.riotgames.com/lol/league/v4/entries/{queue}/{tier}/{division}?page={page}");

        public Task<ResponseRiotDto<MatchDto>> GetMatchAsync(string matchId,bool IsBcg=false)
        => GetAsync<MatchDto>($"https://europe.api.riotgames.com/lol/match/v5/matches/{matchId}");
            
    }
}