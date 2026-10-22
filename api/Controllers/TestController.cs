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
        public TestController(IAccountInfoTest accountInfoService,ICrawlPlayerDbService crawlPlayerDbService)
        {
            _crawlDbService=crawlPlayerDbService;
            _accountInfoService = accountInfoService;
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
    }
}