using System.ComponentModel.DataAnnotations;

namespace Todo.Api.Models
{
	public class TodoItem
	{
		public Guid Id { get; set; } = Guid.NewGuid();

		[Required]
		public Guid UserId { get; set; }

		[Required]
		[MinLength(3)]
		[MaxLength(100)]
		public string Title { get; set; } = string.Empty;

		[MaxLength(1000)]
		public string? Details { get; set; }

		[Required]
		public Priority Priority { get; set; }

		public DateTime? DueDate { get; set; }

		public bool IsCompleted { get; set; } = false;

		public bool IsPublic { get; set; } = false;

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

		public User? User { get; set; }
	}
}