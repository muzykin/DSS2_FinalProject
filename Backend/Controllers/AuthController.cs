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
			// Check if email already exists
			if (await _context.Users.AnyAsync(u => u.Email == request.Email))
			{
				return Conflict(new { message = "Email already in use" }); // 409 Conflict
			}

			// Hash password using BCrypt
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

			return Created("", response); // 201 Created
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginRequest request)
		{
			// Find user by email
			var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == request.Email);

			// Verify user exists and password is correct
			if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
			{
				return Unauthorized(new { message = "Invalid email or password" }); // 401 Unauthorized
			}

			// Generate JWT Token
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

			return Ok(response); // 200 OK
		}

		private string GenerateJwtToken(User user)
		{
			var keyStr = _config["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT Secret missing");
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
				Issuer = _config["JwtSettings:Issuer"],
				Audience = _config["JwtSettings:Audience"],
				SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
			};

			var tokenHandler = new JwtSecurityTokenHandler();
			var token = tokenHandler.CreateToken(tokenDescriptor);

			return tokenHandler.WriteToken(token);
		}
	}
}