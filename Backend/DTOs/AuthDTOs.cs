using System.ComponentModel.DataAnnotations;

namespace Todo.Api.DTOs
{
	public class RegisterRequest
	{
		[Required]
		[EmailAddress]
		[MaxLength(254)]
		public string Email { get; set; } = string.Empty;

		[Required]
		[MinLength(6)]
		[MaxLength(128)]
		public string Password { get; set; } = string.Empty;

		public string? DisplayName { get; set; }
	}

	public class LoginRequest
	{
		[Required]
		[EmailAddress]
		public string Email { get; set; } = string.Empty;

		[Required]
		public string Password { get; set; } = string.Empty;
	}

	public class AuthUserResponse
	{
		public Guid Id { get; set; }
		public string Email { get; set; } = string.Empty;
		public string? DisplayName { get; set; }
	}

	public class LoginResponse
	{
		public string AccessToken { get; set; } = string.Empty;
		public string TokenType { get; set; } = "Bearer";
		public int ExpiresInSeconds { get; set; } = 3600;
		public AuthUserResponse User { get; set; } = new AuthUserResponse();
	}
}