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
            // KROK 1: Pobieranie danych konta (Puuid)
            Console.WriteLine($"[LOG] Szukam gracza: {gameName}#{tagLine}...");
            var account = await _riotClient.GetAccountByRiotIdAsync(gameName, tagLine);
            
            if (account == null)
            {
                Console.WriteLine("[LOG] ERROR: Riot Account API zwróciło NULL (brak gracza lub błąd API)!");
                return new ResponseDto<FullAccountInfoDto>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.NotFound,
                    Message = $"Nie znaleziono konta Riot dla: {gameName}#{tagLine}"
                };
            }

            Console.WriteLine($"[LOG] Sukces! Puuid: {account.Puuid}");

            // KROK 2: Pobieranie Summoner Level (próbujemy eun1, potem euw1)
            Console.WriteLine("[LOG] Pobieranie poziomu przywoływacza...");
            var summoner = await _riotClient.GetSummonerByPuuidAsync("eun1", account.Puuid) 
                        ?? await _riotClient.GetSummonerByPuuidAsync("euw1", account.Puuid);

            if (summoner == null)
            {
                Console.WriteLine("[LOG] WARNING: Nie znaleziono danych Summoner na eun1/euw1.");
            }
            else
            {
                Console.WriteLine($"[LOG] Sukces! Poziom konta: {summoner.SummonerLevel}");
            }

            // KROK 3: Pobieranie ostatnich 10 meczy
            Console.WriteLine("[LOG] Pobieranie historii meczów...");
            var matchIds = await _riotClient.GetRecentMatchIdsAsync("europe", account.Puuid, 10);
            Console.WriteLine($"[LOG] Pobrano meczów: {matchIds.Count}");

            var result = new FullAccountInfoDto
            {
                Puuid = account.Puuid,
                GameName = account.GameName,
                TagLine = account.TagLine,
                SummonerLevel = summoner?.SummonerLevel ?? 0,
                RecentMatchIds = matchIds
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