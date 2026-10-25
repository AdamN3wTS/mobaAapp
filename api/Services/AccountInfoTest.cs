using System;
using System.Net;
using System.Threading.Tasks;
using api.Client;
using api.IServices;
using api.Models;
using api.Models.Account;

namespace api.Services
{
    public class AccountInfoTest : IAccountInfoTest
    {
        private readonly IRiotClient _riotClient;

        public AccountInfoTest(IRiotClient riotClient)
        {
            _riotClient = riotClient;
        }

        

        public async Task<ResponseDto<FullAccountInfoDto>> GetFullAccountInfoAsync(string gameName, string tagLine)
        {
            // KROK 1: Konto (PUUID)
            Console.WriteLine($"[LOG] Szukam gracza: {gameName}#{tagLine}...");
            var account = await _riotClient.GetAccountByRiotIdAsync(gameName, tagLine);

            if (account?.Data?.Puuid is null)
            {
                Console.WriteLine($"[LOG] ERROR: Brak konta (status: {account?.StatusCode})");
                return new ResponseDto<FullAccountInfoDto>
                {
                    IsSuccess = false,
                    StatusCode = account?.StatusCode ?? HttpStatusCode.BadGateway,
                    Message = $"Nie znaleziono konta Riot dla: {gameName}#{tagLine}"
                };
            }

            var puuid = account.Data.Puuid;
            Console.WriteLine($"[LOG] Sukces! Puuid: {puuid}");
            var regionRes = await _riotClient.GetActiveRegionByPuuidAsync(puuid);
            var platform = regionRes.Data?.Region ?? "eun1";      
            Console.WriteLine($"[LOG] Platforma gracza: {platform}");

            Console.WriteLine("[LOG] Pobieranie poziomu przywoływacza...");
            var summoner = await _riotClient.GetSummonerByPuuidAsync(platform, puuid);
            

            if (summoner?.Data is null)
                Console.WriteLine($"[LOG] WARNING: Brak danych Summoner na eun1/euw1 (status: {summoner?.StatusCode})");
            else
                Console.WriteLine($"[LOG] Sukces! Poziom konta: {summoner.Data.SummonerLevel}");

            // KROK 3: Ostatnie 10 meczów
            Console.WriteLine("[LOG] Pobieranie historii meczów...");
            var matchIds = await _riotClient.GetRecentMatchIdsAsync("europe", puuid, 10);
            var ids = matchIds?.Data ?? new List<string>();
            Console.WriteLine($"[LOG] Pobrano meczów: {ids.Count}");
            // KROK 4: ranga tego typu
            Console.WriteLine($"[LOG] Zaczynam pobierać rangę");
            var rank = await _riotClient.GetRanksByPuuidAsync(platform,account.Data.Puuid);
            if (rank.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine($"[Warning] Błąd podczas pobierania rangi status {rank.StatusCode}");

            }
            var result = new FullAccountInfoDto
            {
                Puuid = puuid,
                GameName = account.Data.GameName,
                TagLine = account.Data.TagLine,
                SummonerLevel = summoner?.Data?.SummonerLevel ?? 0,
                RecentMatchIds = ids,
                Ranks =rank?.Data ?? new List<Models.Rank.RankDto?>()
            };

            return new ResponseDto<FullAccountInfoDto>
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Data = result
            };
        }
    }
}