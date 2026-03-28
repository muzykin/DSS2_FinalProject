using System.ComponentModel.DataAnnotations;
using Todo.Api.Models;

namespace Todo.Api.DTOs
{
	public class TodoResponse
	{
		public Guid Id { get; set; }
		public string Title { get; set; } = string.Empty;
		public string? Details { get; set; }
		public Priority Priority { get; set; } // The JSON converter will make this lowercase string
		public string? DueDate { get; set; }
		public bool IsCompleted { get; set; }
		public bool IsPublic { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
	}

	public class CreateTodoRequest
	{
		[Required(ErrorMessage = "Title is required")]
		[MinLength(3, ErrorMessage = "Title must be at least 3 characters")]
		[MaxLength(100, ErrorMessage = "Title must not exceed 100 characters")]
		public string Title { get; set; } = string.Empty;

		[MaxLength(1000, ErrorMessage = "Details must not exceed 1000 characters")]
		public string? Details { get; set; }

		[Required(ErrorMessage = "Priority is required")]
		[EnumDataType(typeof(Priority), ErrorMessage = "Invalid priority value")]
		public Priority Priority { get; set; }

		[RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "DueDate must be in YYYY-MM-DD format")]
		public string? DueDate { get; set; }

		public bool IsPublic { get; set; } = false;
	}

	public class UpdateTodoRequest
	{
		[Required(ErrorMessage = "Title is required")]
		[MinLength(3, ErrorMessage = "Title must be at least 3 characters")]
		[MaxLength(100, ErrorMessage = "Title must not exceed 100 characters")]
		public string Title { get; set; } = string.Empty;

		[MaxLength(1000, ErrorMessage = "Details must not exceed 1000 characters")]
		public string? Details { get; set; }

		[Required(ErrorMessage = "Priority is required")]
		[EnumDataType(typeof(Priority), ErrorMessage = "Invalid priority value")]
		public Priority Priority { get; set; }

		[RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "DueDate must be in YYYY-MM-DD format")]
		public string? DueDate { get; set; }

		public bool IsPublic { get; set; }
		public bool IsCompleted { get; set; }
	}

	public class SetCompletionRequest
	{
		public bool IsCompleted { get; set; }
	}

	public class PagedResponse<T>
	{
		public IEnumerable<T> Items { get; set; } = new List<T>();
		public int Page { get; set; }
		public int PageSize { get; set; }
		public int TotalItems { get; set; }
		public int TotalPages { get; set; }
	}
}