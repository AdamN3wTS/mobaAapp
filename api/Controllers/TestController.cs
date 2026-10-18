using System.Net;
using api.Helpers;
using api.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/test")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IAccountInfoTest _accTest;
        public TestController(IAccountInfoTest accountInfoTest)
        {
            _accTest=accountInfoTest;
        }
        [HttpGet("account-info/{gameName}/{tagLine}")]
        public async Task<IActionResult> AccountInfo(string gameName,string tagLine,CancellationToken ct)
        {
            var serviceResponse = await _accTest.GetRiotAccountAsync(gameName,tagLine,ct);
            return StatusCodeHelper.SafeResponse(this,serviceResponse);
            
        }
    }
}
