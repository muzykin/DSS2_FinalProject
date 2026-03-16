using System.ComponentModel.DataAnnotations;

namespace Todo.Api.Models
{
	public class User
	{
		public Guid Id { get; set; } = Guid.NewGuid();

		[Required]
		[MaxLength(254)]
		public string Email { get; set; } = string.Empty;

		[Required]
		public string PasswordHash { get; set; } = string.Empty;

		public string? DisplayName { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		public ICollection<TodoItem> Todos { get; set; } = new List<TodoItem>();
	}
}