using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MatchController : ControllerBase
    {

        [HttpGet("match/{matchId}")]
        public async Task<IActionResult> GetMatch(string matchId,CancellationToken ct)
        {
            return Ok();
        }
    }
}
