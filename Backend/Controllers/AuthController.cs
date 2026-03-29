using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Todo.Api.Data;
using Todo.Api.DTOs;
using Todo.Api.Models;

namespace Todo.Api.Controllers
{
	[ApiController]
	[Route("api/auth")]
	public class AuthController : ControllerBase
	{
		private readonly AppDbContext _context;
		private readonly IConfiguration _config;

		public AuthController(AppDbContext context, IConfiguration config)
		{
			_context = context;
			_config = config;
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] RegisterRequest request)
		{
			if (await _context.Users.AnyAsync(u => u.Email == request.Email))
			{
				return Conflict(new { message = "Email already in use" });
			}

			string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

			var newUser = new User
			{
				Email = request.Email,
				PasswordHash = passwordHash,
				DisplayName = request.DisplayName
			};

			_context.Users.Add(newUser);
			await _context.SaveChangesAsync();

			var response = new AuthUserResponse
			{
				Id = newUser.Id,
				Email = newUser.Email,
				DisplayName = newUser.DisplayName
			};

			return Created("", response);
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginRequest request)
		{
			Console.WriteLine($"DEBUG LOGIN: email={request.Email}");

			var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == request.Email);

			if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
			{
				Console.WriteLine("DEBUG LOGIN: invalid credentials");
				return Unauthorized(new { message = "Invalid email or password" });
			}

			var token = GenerateJwtToken(user);

			var response = new LoginResponse
			{
				AccessToken = token,
				TokenType = "Bearer",
				ExpiresInSeconds = 3600,
				User = new AuthUserResponse
				{
					Id = user.Id,
					Email = user.Email,
					DisplayName = user.DisplayName
				}
			};

			Console.WriteLine($"DEBUG LOGIN: success userId={user.Id}");

			return Ok(response);
		}

		private string GenerateJwtToken(User user)
		{
			var keyStr = _config["JwtSettings:SecretKey"] ?? "ThisIsAVerySecretKeyForTodoApiProjectDDS2DoNotShare";
			var keyBytes = Encoding.UTF8.GetBytes(keyStr);

			var claims = new[]
			{
				new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
				new Claim(JwtRegisteredClaimNames.Email, user.Email),
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
			};

			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(claims),
				Expires = DateTime.UtcNow.AddSeconds(3600),
				// УБРАЛИ ISSUER И AUDIENCE ОТСЮДА!
				SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
			};

			var tokenHandler = new JwtSecurityTokenHandler();
			var token = tokenHandler.CreateToken(tokenDescriptor);

			return tokenHandler.WriteToken(token);
		}
	}
}