using System.Threading.Tasks;
using api.Helpers;
using api.IServices;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        private readonly IAccountInfoTest _accountInfoService;

        public TestController(IAccountInfoTest accountInfoService)
        {
            _accountInfoService = accountInfoService;
        }

        [HttpGet("account-info/{nick}/{tag}")]
        public async Task<IActionResult> GetAccountInfo(string nick, string tag)
        {
            var result = await _accountInfoService.GetFullAccountInfoAsync(nick, tag);

            return StatusCodeHelper.SafeResponse(this,result);
        }
    }
}