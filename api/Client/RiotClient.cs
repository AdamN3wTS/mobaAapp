using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using api.Models;
using api.Models.Account;
using api.Models.Rank;
using api.Models.Region;
using Microsoft.Extensions.Configuration;

namespace api.Client
{
    public class RiotClient : IRiotClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public RiotClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            // Pobieranie ze zmiennych środowiskowych DotNetEnv / Systemu lub appsettings
            _apiKey = Environment.GetEnvironmentVariable("RIOT_API_KEY") 
                      ?? configuration["RIOT_API_KEY"] 
                      ?? string.Empty;
            _httpClient.DefaultRequestHeaders.Add("X-Riot-Token",_apiKey);

            
        }

        private async Task<ResponseRiotDto<T?>> GetAsync<T>(string url)
        {
            using var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[RIOT] {(int)response.StatusCode} {url.Split('?')[0]} {await response.Content.ReadAsStringAsync()}");
                return new ResponseRiotDto<T?> { StatusCode = response.StatusCode };   
            }

            return new ResponseRiotDto<T?>
            {
                StatusCode = response.StatusCode,
                Data = await response.Content.ReadFromJsonAsync<T>()
            };
        }

        public Task<ResponseRiotDto<AccountDto?>> GetAccountByRiotIdAsync(string gameName, string tagLine) =>
            GetAsync<AccountDto>($"https://europe.api.riotgames.com/riot/account/v1/accounts/by-riot-id/{Uri.EscapeDataString(gameName)}/{Uri.EscapeDataString(tagLine)}");

        public Task<ResponseRiotDto<SummonerDto?>> GetSummonerByPuuidAsync(string platform, string puuid) =>
            GetAsync<SummonerDto>($"https://{platform}.api.riotgames.com/lol/summoner/v4/summoners/by-puuid/{puuid}");

        public Task<ResponseRiotDto<List<string>?>> GetRecentMatchIdsAsync(string region, string puuid, int count = 10) =>
            GetAsync<List<string>>($"https://{region}.api.riotgames.com/lol/match/v5/matches/by-puuid/{puuid}/ids?start=0&count={count}");
        public Task<ResponseRiotDto<List<RankDto?>>> GetRanksByPuuidAsync(string region, string puuid) =>
            GetAsync<List<RankDto?>>($"https://{region}.api.riotgames.com/lol/league/v4/entries/by-puuid/{puuid}");
        public Task<ResponseRiotDto<RegionDto?>> GetActiveRegionByPuuidAsync(string puuid) =>
            GetAsync<RegionDto?>($"https://europe.api.riotgames.com/riot/account/v1/region/by-game/lol/by-puuid/{puuid}");
    }
}