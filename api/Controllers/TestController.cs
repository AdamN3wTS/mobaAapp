using System.Threading.Tasks;
using api.Helpers;
using api.IDbServices;
using api.IServices;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        private readonly IAccountInfoTest _accountInfoService;
        private readonly ICrawlPlayerDbService _crawlDbService;
        private readonly IChampionService _championService;
        public TestController(IAccountInfoTest accountInfoService,ICrawlPlayerDbService crawlPlayerDbService,IChampionService championService)
        {
            _crawlDbService=crawlPlayerDbService;
            _accountInfoService = accountInfoService;
            _championService=championService;
        }

        [HttpGet("account-info/{nick}/{tag}")]
        public async Task<IActionResult> GetAccountInfo(string nick, string tag)
        {
            var result = await _accountInfoService.GetFullAccountInfoAsync(nick, tag);

            return StatusCodeHelper.SafeResponse(this,result);
        }
        [HttpGet("playersDb")]
        public async Task<IActionResult> GetPlayers(CancellationToken cancellationToken)
        {
            var result = await _crawlDbService.GetPlayersAsync(cancellationToken);
            return Ok(result);
        }
        [HttpGet("champion/{championName}/{teamPosition}")]
        public async Task<IActionResult> GetChampion(string championName,string teamPosition,CancellationToken cancellationToken)
        {
            var result = await _championService.GetChampionStatsAsync(championName,teamPosition,cancellationToken);
            return Ok(result);
        }
        [HttpGet("champion/{championName}")]
        public async Task<IActionResult> GetChampionPositions(string championName, CancellationToken ct)
        {
            var result = await _championService.GetChampionPositionsAsync(championName, ct);
            return StatusCode((int)result.StatusCode, result);
        }
    }
}