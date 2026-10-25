using api.Helpers;
using api.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/stats")]
    [ApiController]
    public class StatsController : ControllerBase
    {
        private readonly IStatsService _statsService;
        public StatsController(IStatsService statsService)
        {
            _statsService=statsService;
        }
        [HttpGet("get")]
        public async Task<IActionResult> GetStats(CancellationToken ct)
        {
            var serviceResult = await _statsService.GetRecentStatsAsync(ct);
            return StatusCodeHelper.SafeResponse(this,serviceResult);
        }

    }
}
