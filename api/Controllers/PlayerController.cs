using Microsoft.AspNetCore.Mvc;
using Api.DTOs;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PlayerController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public PlayerController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Pobiera prawdziwy profil gracza z Riot API na podstawie Nicku i Tagu.
        /// </summary>
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

            string apiKey = _configuration["RIOT_API_KEY"] ?? _configuration["RiotApi:ApiKey"] ?? "";
            string regionalRoute = _configuration["RIOT_REGION"] ?? "europe"; // europe, americas, asia
            string platformRoute = _configuration["RIOT_PLATFORM"] ?? "eun1"; // eun1, euw1, kr

            using var client = new HttpClient();

            try
            {
                // 1. Pobranie PUUID z Riot Account-V1 API
                var accountUrl = $"https://{regionalRoute}.api.riotgames.com/riot/account/v1/accounts/by-riot-id/{gameName}/{tagLine}?api_key={apiKey}";
                var accountResponse = await client.GetAsync(accountUrl);

                if (accountResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return NotFound($"Nie znaleziono gracza o Riot ID: {gameName}#{tagLine}");
                }

                if (!accountResponse.IsSuccessStatusCode)
                {
                    return StatusCode((int)accountResponse.StatusCode, "Błąd podczas pobierania danych konta z Riot API.");
                }

                using var accountDoc = await JsonDocument.ParseAsync(await accountResponse.Content.ReadAsStreamAsync());
                var accountRoot = accountDoc.RootElement;

                string realPuuid = accountRoot.TryGetProperty("puuid", out var puuidElem) ? puuidElem.GetString() ?? "" : "";
                string realGameName = accountRoot.TryGetProperty("gameName", out var nameElem) ? nameElem.GetString() ?? gameName : gameName;
                string realTagLine = accountRoot.TryGetProperty("tagLine", out var tagElem) ? tagElem.GetString() ?? tagLine : tagLine;

                // 2. Pobranie danych przywoływacza (Level i ProfileIcon) z Summoner-V4 API
                var summonerUrl = $"https://{platformRoute}.api.riotgames.com/lol/summoner/v4/summoners/by-puuid/{realPuuid}?api_key={apiKey}";
                var summonerResponse = await client.GetAsync(summonerUrl);

                int profileIconId = 0;
                long summonerLevel = 0;
                string summonerId = "";

                if (summonerResponse.IsSuccessStatusCode)
                {
                    using var summonerDoc = await JsonDocument.ParseAsync(await summonerResponse.Content.ReadAsStreamAsync());
                    var summonerRoot = summonerDoc.RootElement;

                    if (summonerRoot.TryGetProperty("profileIconId", out var iconElem)) profileIconId = iconElem.GetInt32();
                    if (summonerRoot.TryGetProperty("summonerLevel", out var lvlElem)) summonerLevel = lvlElem.GetInt64();
                    if (summonerRoot.TryGetProperty("id", out var idElem)) summonerId = idElem.GetString() ?? "";
                }

                // 3. Pobranie rang gracza z League-V4 API
                var ranksList = new List<RankedInfoDTO>();

                if (!string.IsNullOrEmpty(summonerId))
                {
                    var leagueUrl = $"https://{platformRoute}.api.riotgames.com/lol/league/v4/entries/by-summoner/{summonerId}?api_key={apiKey}";
                    var leagueResponse = await client.GetAsync(leagueUrl);

                    if (leagueResponse.IsSuccessStatusCode)
                    {
                        using var leagueDoc = await JsonDocument.ParseAsync(await leagueResponse.Content.ReadAsStreamAsync());
                        var leagueRoot = leagueDoc.RootElement;

                        foreach (var entry in leagueRoot.EnumerateArray())
                        {
                            ranksList.Add(new RankedInfoDTO
                            {
                                QueueType = entry.TryGetProperty("queueType", out var qElem) ? qElem.GetString() ?? "" : "",
                                Tier = entry.TryGetProperty("tier", out var tElem) ? tElem.GetString() ?? "" : "",
                                Rank = entry.TryGetProperty("rank", out var rElem) ? rElem.GetString() ?? "" : "",
                                LeaguePoints = entry.TryGetProperty("leaguePoints", out var lpElem) ? lpElem.GetInt32() : 0,
                                Wins = entry.TryGetProperty("wins", out var wElem) ? wElem.GetInt32() : 0,
                                Losses = entry.TryGetProperty("losses", out var lElem) ? lElem.GetInt32() : 0,
                                HotStreak = entry.TryGetProperty("hotStreak", out var hsElem) && hsElem.GetBoolean()
                            });
                        }
                    }
                }

                var profile = new PlayerProfileDTO
                {
                    Puuid = realPuuid,
                    GameName = realGameName,
                    TagLine = realTagLine,
                    ProfileIconId = profileIconId,
                    SummonerLevel = summonerLevel,
                    Ranks = ranksList
                };

                return Ok(profile);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Błąd serwera: {ex.Message}");
            }
        }

        /// <summary>
        /// Pobiera stronicowaną historię meczów gracza na podstawie jego PUUID bezpośrednio z Riot API.
        /// </summary>
        [HttpGet("{puuid}/matches")]
        [ProducesResponseType(typeof(PaginatedMatchHistoryDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

            string apiKey = _configuration["RIOT_API_KEY"] ?? _configuration["RiotApi:ApiKey"] ?? "";
            string region = _configuration["RIOT_REGION"] ?? "europe";

            int start = (page - 1) * pageSize;

            using var client = new HttpClient();

            try
            {
                var idsUrl = $"https://{region}.api.riotgames.com/lol/match/v5/matches/by-puuid/{puuid}/ids?start={start}&count={pageSize}&api_key={apiKey}";
                var idsResponse = await client.GetAsync(idsUrl);

                if (!idsResponse.IsSuccessStatusCode)
                {
                    return StatusCode((int)idsResponse.StatusCode, "Błąd podczas pobierania listy meczów z Riot API.");
                }

                var matchIdsJson = await idsResponse.Content.ReadAsStringAsync();
                var matchIds = JsonSerializer.Deserialize<List<string>>(matchIdsJson) ?? new List<string>();

                var matchesList = new List<MatchSummaryDTO>();

                foreach (var matchId in matchIds)
                {
                    var matchUrl = $"https://{region}.api.riotgames.com/lol/match/v5/matches/{matchId}?api_key={apiKey}";
                    var matchResponse = await client.GetAsync(matchUrl);

                    if (!matchResponse.IsSuccessStatusCode) continue;

                    using var doc = await JsonDocument.ParseAsync(await matchResponse.Content.ReadAsStreamAsync());
                    var root = doc.RootElement;
                    if (!root.TryGetProperty("info", out var info)) continue;
                    if (!info.TryGetProperty("participants", out var participants)) continue;

                    JsonElement playerStats = default;
                    bool found = false;

                    foreach (var participant in participants.EnumerateArray())
                    {
                        if (participant.TryGetProperty("puuid", out var pElem) && pElem.GetString() == puuid)
                        {
                            playerStats = participant;
                            found = true;
                            break;
                        }
                    }

                    if (!found) continue;

                    int kills = playerStats.TryGetProperty("kills", out var k) ? k.GetInt32() : 0;
                    int deaths = playerStats.TryGetProperty("deaths", out var d) ? d.GetInt32() : 0;
                    int assists = playerStats.TryGetProperty("assists", out var a) ? a.GetInt32() : 0;
                    int totalCs = (playerStats.TryGetProperty("totalMinionsKilled", out var tmk) ? tmk.GetInt32() : 0) + 
                                 (playerStats.TryGetProperty("neutralMinionsKilled", out var nmk) ? nmk.GetInt32() : 0);
                    int dmg = playerStats.TryGetProperty("totalDamageDealtToChampions", out var dmgElem) ? dmgElem.GetInt32() : 0;
                    int vision = playerStats.TryGetProperty("visionScore", out var vElem) ? vElem.GetInt32() : 0;

                    matchesList.Add(new MatchSummaryDTO
                    {
                        MatchId = matchId,
                        IsWin = playerStats.TryGetProperty("win", out var w) && w.GetBoolean(),
                        GameDurationInSeconds = info.TryGetProperty("gameDuration", out var gd) ? gd.GetInt64() : 0,
                        QueueType = GetQueueName(info.TryGetProperty("queueId", out var q) ? q.GetInt32() : 0),
                        LpChange = 0,
                        GameCreationTimestamp = DateTimeOffset.FromUnixTimeMilliseconds(info.TryGetProperty("gameStartTimestamp", out var gst) ? gst.GetInt64() : 0),
                        ChampionName = playerStats.TryGetProperty("championName", out var cn) ? cn.GetString() ?? "" : "",
                        ChampionId = playerStats.TryGetProperty("championId", out var cid) ? cid.GetInt32() : 0,
                        Stats = new MatchPlayerStatsDTO
                        {
                            Kills = kills,
                            Deaths = deaths,
                            Assists = assists,
                            TotalMinionsKilled = totalCs,
                            DamageDealtToChampions = dmg,
                            VisionScore = vision
                        }
                    });
                }

                var result = new PaginatedMatchHistoryDTO
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = 100,
                    Matches = matchesList
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Błąd serwera: {ex.Message}");
            }
        }

        private static string GetQueueName(int queueId) => queueId switch
        {
            420 => "Ranked Solo/Duo",
            440 => "Ranked Flex",
            450 => "ARAM",
            400 => "Normal Draft",
            430 => "Normal Blind",
            _ => $"Tryb {queueId}"
        };
    }
}