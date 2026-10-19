using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using api.Models.Account;
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
        }

        public async Task<AccountDto?> GetAccountByRiotIdAsync(string gameName, string tagLine)
        {
            var url = $"https://europe.api.riotgames.com/riot/account/v1/accounts/by-riot-id/{Uri.EscapeDataString(gameName)}/{Uri.EscapeDataString(tagLine)}?api_key={_apiKey}";
            
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadFromJsonAsync<AccountDto>();
        }

        public async Task<SummonerDto?> GetSummonerByPuuidAsync(string region, string puuid)
        {
            var url = $"https://{region}.api.riotgames.com/lol/summoner/v4/summoners/by-puuid/{puuid}?api_key={_apiKey}";
            
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadFromJsonAsync<SummonerDto>();
        }

        public async Task<List<string>> GetRecentMatchIdsAsync(string region, string puuid, int count = 10)
        {
            var url = $"https://{region}.api.riotgames.com/lol/match/v5/matches/by-puuid/{puuid}/ids?start=0&count={count}&api_key={_apiKey}";
            
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return new List<string>();

            return await response.Content.ReadFromJsonAsync<List<string>>() ?? new List<string>();
        }
    }
}