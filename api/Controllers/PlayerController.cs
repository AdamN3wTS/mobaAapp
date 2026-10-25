using Microsoft.AspNetCore.Mvc;
using Api.DTOs;
using api.IServices;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/player")]
    [Produces("application/json")]
    public class PlayerController : ControllerBase
    {
        private readonly IPlayerService _playerService;
        
        public PlayerController(IPlayerService playerService)
        {
            _playerService = playerService;
        }

        /// <summary>
        /// Pobiera szczegółowe informacje o profilu gracza (bez historii meczów).
        /// </summary>
        /// <param name="gameName">Riot Game Name / Nick gracza (np. Faker)</param>
        /// <param name="tagLine">Riot Tag Line bez znaku # (np. KR1)</param>
        /// <returns>Obiekt z podstawowymi danymi gracza i rangami we wszystkich trybach</returns>
        [HttpGet("get/{gameName}/{tagLine}")]
        public async Task<IActionResult> GetPlayerProfile([FromRoute] string gameName, [FromRoute] string tagLine)
        {
            if (string.IsNullOrWhiteSpace(gameName) || string.IsNullOrWhiteSpace(tagLine))
            {
                return BadRequest("GameName oraz TagLine nie mogą być puste.");
            }

            // TODO: Docelowe wywołanie serwisu pobierającego dane z Riot API / Bazy:
            var profile = await _playerService.GetProfileAsync(gameName, tagLine);
            if (profile == null) return NotFound("Gracz nie został znaleziony.");

            

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
            [FromQuery] int pageSize = 5)
        {
            var result = await _playerService.GetMatchHistoryAsync(puuid, page, pageSize);

            if (!result.IsSuccess)
                return StatusCode((int)result.StatusCode, result.Message);

            return Ok(result.Data);
        }
    }
}