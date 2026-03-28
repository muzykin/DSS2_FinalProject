using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Todo.Api.Services;

namespace Todo.Api.Controllers
{
	[Route("api/integrations")]
	[ApiController]
	[AllowAnonymous]
	public class IntegrationsController : ControllerBase
	{
		private readonly IDistributedCache _cache;
		private readonly IRabbitMqService _rabbitMq;

		public IntegrationsController(IDistributedCache cache, IRabbitMqService rabbitMq)
		{
			_cache = cache;
			_rabbitMq = rabbitMq;
		}

		[HttpGet("redis/health")]
		public async Task<IActionResult> RedisHealth()
		{
			try
			{
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

		[HttpGet("rabbitmq/health")]
		public IActionResult RabbitMqHealth()
		{
			string result = _rabbitMq.CheckHealth();

			if (result == "connected")
				return Ok(new { status = "connected" });

			return StatusCode(503, new { status = "error", message = result });
		}
	}
}