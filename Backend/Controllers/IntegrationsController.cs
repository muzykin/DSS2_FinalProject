using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace Todo.Api.Controllers
{
	[Route("api/integrations")]
	[ApiController]
	[AllowAnonymous]
	public class IntegrationsController : ControllerBase
	{
		private readonly IDistributedCache _cache;

		public IntegrationsController(IDistributedCache cache)
		{
			_cache = cache;
		}

		[HttpGet("redis/health")]
		public async Task<IActionResult> RedisHealth()
		{
			try
			{
				// This is a simple test to check if Redis is working. We set a value and then get it back.
				await _cache.SetStringAsync("health_check", "ok");
				var result = await _cache.GetStringAsync("health_check");

				if (result == "ok")
					return Ok(new { status = "connected" });

				return StatusCode(503, new { status = "error", message = "Data mismatch" });
			}
			catch (Exception ex)
			{
				return StatusCode(503, new { status = "error", message = ex.Message });
			}
		}
	}
}