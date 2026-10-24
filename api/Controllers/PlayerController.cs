using Microsoft.AspNetCore.Mvc;
using Api.DTOs;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PlayerController : ControllerBase
    {
        // TODO: Po zaimplementowaniu serwisu odblokuj poniższe wstrzykiwanie zależności:
        // private readonly IPlayerService _playerService;
        //
        // public PlayerController(IPlayerService playerService)
        // {
        //     _playerService = playerService;
        // }

        /// <summary>
        /// Pobiera szczegółowe informacje o profilu gracza (bez historii meczów).
        /// </summary>
        /// <param name="gameName">Riot Game Name / Nick gracza (np. Faker)</param>
        /// <param name="tagLine">Riot Tag Line bez znaku # (np. KR1)</param>
        /// <returns>Obiekt z podstawowymi danymi gracza i rangami we wszystkich trybach</returns>
        [HttpGet("{gameName}/{tagLine}")]
        [ProducesResponseType(typeof(PlayerProfileDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPlayerProfile([FromRoute] string gameName, [FromRoute] string tagLine)
        {
            if (string.IsNullOrWhiteSpace(gameName) || string.IsNullOrWhiteSpace(tagLine))
            {
                return BadRequest("GameName oraz TagLine nie mogą być puste.");
            }

            // TODO: Docelowe wywołanie serwisu pobierającego dane z Riot API / Bazy:
            // var profile = await _playerService.GetProfileAsync(gameName, tagLine);
            // if (profile == null) return NotFound("Gracz nie został znaleziony.");

            // Tymczasowa odpowiedź (Mock) dla testów z frontendem:
            var profile = new PlayerProfileDTO
            {
                Puuid = "sample-puuid-12345-abcde",
                GameName = gameName,
                TagLine = tagLine,
                ProfileIconId = 548,
                SummonerLevel = 154,
                Ranks = new List<RankedInfoDTO>
                {
                    new RankedInfoDTO
                    {
                        QueueType = "RANKED_SOLO_5x5",
                        Tier = "GOLD",
                        Rank = "I",
                        LeaguePoints = 75,
                        Wins = 45,
                        Losses = 35,
                        HotStreak = true
                    },
                    new RankedInfoDTO
                    {
                        QueueType = "RANKED_FLEX_SR",
                        Tier = "SILVER",
                        Rank = "II",
                        LeaguePoints = 20,
                        Wins = 10,
                        Losses = 12,
                        HotStreak = false
                    }
                }
            };

            return Ok(profile);
        }

        /// <summary>
        /// Pobiera stronicowaną historię meczów gracza na podstawie jego PUUID.
        /// </summary>
        /// <param name="puuid">Unikalny identyfikator gracza (PUUID)</param>
        /// <param name="page">Numer strony (od 1)</param>
        /// <param name="pageSize">Liczba meczów na stronę (odpowiednik take)</param>
        /// <returns>Stronicowana lista skróconych informacji o meczach</returns>
        [HttpGet("{puuid}/matches")]
        [ProducesResponseType(typeof(PaginatedMatchHistoryDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMatchHistory(
            [FromRoute] string puuid, 
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 20)
        {
            if (string.IsNullOrWhiteSpace(puuid))
            {
                return BadRequest("Parametr PUUID nie może być pusty.");
            }

            if (page < 1 || pageSize < 1)
            {
                return BadRequest("Parametry 'page' oraz 'pageSize' muszą być większe od 0.");
            }

            // Ograniczenie maksymalnej liczby pobieranych meczów na raz
            if (pageSize > 100)
            {
                pageSize = 100;
            }

            int skip = (page - 1) * pageSize;

            // TODO: Docelowe wywołanie serwisu pobierającego dane z Riot API / Bazy:
            // var result = await _playerService.GetMatchHistoryAsync(puuid, skip, pageSize);

            // Tymczasowa pętla generująca dane testowe o właściwym rozmiarze strony:
            var dummyMatches = new List<MatchSummaryDTO>();
            for (int i = 1; i <= pageSize; i++)
            {
                int matchIndex = skip + i;
                dummyMatches.Add(new MatchSummaryDTO
                {
                    MatchId = $"EUN1_MATCH_{matchIndex}",
                    IsWin = i % 2 == 0,
                    GameDurationInSeconds = 1200 + (i * 30),
                    QueueType = "Ranked Solo",
                    LpChange = i % 2 == 0 ? 18 : -15,
                    GameCreationTimestamp = DateTimeOffset.UtcNow.AddDays(-i),
                    ChampionName = i % 3 == 0 ? "Ahri" : "Lux",
                    ChampionId = 103,
                    Stats = new MatchPlayerStatsDTO
                    {
                        Kills = 5 + (i % 5),
                        Deaths = 2,
                        Assists = 10 + (i % 3),
                        TotalMinionsKilled = 150 + i,
                        DamageDealtToChampions = 15000 + (i * 100),
                        VisionScore = 20 + i
                    }
                });
            }

            var result = new PaginatedMatchHistoryDTO
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = 100, // Całkowita dostępna liczba meczów
                Matches = dummyMatches
            };

            return Ok(result);
        }
    }
}