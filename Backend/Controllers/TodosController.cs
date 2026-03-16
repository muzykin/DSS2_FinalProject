using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Todo.Api.Data;
using Todo.Api.DTOs;
using Todo.Api.Models;

namespace Todo.Api.Controllers
{
	[Route("api/todos")]
	[ApiController]
	[Authorize]
	public class TodosController : ControllerBase
	{
		private readonly AppDbContext _context;

		public TodosController(AppDbContext context)
		{
			_context = context;
		}

		private Guid GetCurrentUserId()
		{
			var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (Guid.TryParse(userIdString, out Guid userId))
			{
				return userId;
			}
			throw new UnauthorizedAccessException("User ID not found in token");
		}

		// GET: api/todos/public
		[HttpGet("public")]
		[AllowAnonymous]
		public async Task<IActionResult> GetPublicTodos([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
		{
			if (page < 1) page = 1;
			if (pageSize < 1 || pageSize > 50) pageSize = 10;

			var query = _context.Todos.Where(t => t.IsPublic);

			var totalItems = await query.CountAsync();
			var todos = await query
				.OrderByDescending(t => t.CreatedAt)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			var response = new PagedResponse<TodoResponse>
			{
				Items = todos.Select(MapToResponse),
				Page = page,
				PageSize = pageSize,
				TotalItems = totalItems,
				TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
			};

			return Ok(response);
		}

		// GET: api/todos
		[HttpGet]
		public async Task<IActionResult> GetUserTodos(
			[FromQuery] int page = 1,
			[FromQuery] int pageSize = 10,
			[FromQuery] string status = "all",
			[FromQuery] string? priority = null,
			[FromQuery] string? dueFrom = null,
			[FromQuery] string? dueTo = null,
			[FromQuery] string sortBy = "createdAt",
			[FromQuery] string sortDir = "desc",
			[FromQuery] string? search = null)
		{
			if (page < 1) page = 1;
			if (pageSize < 1 || pageSize > 50) pageSize = 10;

			var userId = GetCurrentUserId();
			var query = _context.Todos.Where(t => t.UserId == userId);

			// 1. Filtering
			if (status.Equals("active", StringComparison.OrdinalIgnoreCase))
				query = query.Where(t => !t.IsCompleted);
			else if (status.Equals("completed", StringComparison.OrdinalIgnoreCase))
				query = query.Where(t => t.IsCompleted);

			if (!string.IsNullOrEmpty(priority) && Enum.TryParse(typeof(Priority), priority, true, out var parsedPriority))
				query = query.Where(t => t.Priority == (Priority)parsedPriority);

			if (!string.IsNullOrEmpty(dueFrom) && DateTime.TryParse(dueFrom, out var fromDate))
				query = query.Where(t => t.DueDate >= fromDate.ToUniversalTime());

			if (!string.IsNullOrEmpty(dueTo) && DateTime.TryParse(dueTo, out var toDate))
				query = query.Where(t => t.DueDate <= toDate.ToUniversalTime());

			// 2. Searching
			if (!string.IsNullOrEmpty(search))
			{
				search = search.ToLower();
				query = query.Where(t => t.Title.ToLower().Contains(search) ||
										(t.Details != null && t.Details.ToLower().Contains(search)));
			}

			// 3. Sorting
			bool isDesc = sortDir.Equals("desc", StringComparison.OrdinalIgnoreCase);
			query = sortBy.ToLower() switch
			{
				"title" => isDesc ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title),
				"duedate" => isDesc ? query.OrderByDescending(t => t.DueDate) : query.OrderBy(t => t.DueDate),
				"priority" => isDesc ? query.OrderByDescending(t => t.Priority) : query.OrderBy(t => t.Priority),
				_ => isDesc ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt), // Default: createdAt
			};

			// 4. Pagination
			var totalItems = await query.CountAsync();
			var todos = await query
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			var response = new PagedResponse<TodoResponse>
			{
				Items = todos.Select(MapToResponse),
				Page = page,
				PageSize = pageSize,
				TotalItems = totalItems,
				TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
			};

			return Ok(response);
		}

		// GET: api/todos/{id}
		[HttpGet("{id}")]
		public async Task<IActionResult> GetTodoById(Guid id)
		{
			var userId = GetCurrentUserId();
			var todo = await _context.Todos.FirstOrDefaultAsync(t => t.Id == id);

			if (todo == null) return NotFound();
			if (todo.UserId != userId) return StatusCode(403, new { message = "Forbidden" });

			return Ok(MapToResponse(todo));
		}

		// POST: api/todos
		[HttpPost]
		public async Task<IActionResult> CreateTodo([FromBody] CreateTodoRequest request)
		{
			DateTime? parsedDueDate = null;
			if (!string.IsNullOrEmpty(request.DueDate) && DateTime.TryParse(request.DueDate, out DateTime tempDate))
			{
				parsedDueDate = tempDate.ToUniversalTime();
			}

			var todo = new TodoItem
			{
				UserId = GetCurrentUserId(),
				Title = request.Title,
				Details = request.Details,
				Priority = request.Priority,
				DueDate = parsedDueDate,
				IsPublic = request.IsPublic,
				IsCompleted = false,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			};

			_context.Todos.Add(todo);
			await _context.SaveChangesAsync();

			return CreatedAtAction(nameof(GetTodoById), new { id = todo.Id }, MapToResponse(todo));
		}

		// PUT: api/todos/{id}
		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateTodo(Guid id, [FromBody] UpdateTodoRequest request)
		{
			var userId = GetCurrentUserId();
			var todo = await _context.Todos.FirstOrDefaultAsync(t => t.Id == id);

			if (todo == null) return NotFound();
			if (todo.UserId != userId) return StatusCode(403, new { message = "Forbidden" });

			DateTime? parsedDueDate = null;
			if (!string.IsNullOrEmpty(request.DueDate) && DateTime.TryParse(request.DueDate, out DateTime tempDate))
			{
				parsedDueDate = tempDate.ToUniversalTime();
			}

			todo.Title = request.Title;
			todo.Details = request.Details;
			todo.Priority = request.Priority;
			todo.DueDate = parsedDueDate;
			todo.IsPublic = request.IsPublic;
			todo.IsCompleted = request.IsCompleted;
			todo.UpdatedAt = DateTime.UtcNow;

			await _context.SaveChangesAsync();

			return Ok(MapToResponse(todo));
		}

		// PATCH: api/todos/{id}/completion
		[HttpPatch("{id}/completion")]
		public async Task<IActionResult> SetCompletion(Guid id, [FromBody] SetCompletionRequest request)
		{
			var userId = GetCurrentUserId();
			var todo = await _context.Todos.FirstOrDefaultAsync(t => t.Id == id);

			if (todo == null) return NotFound();
			if (todo.UserId != userId) return StatusCode(403, new { message = "Forbidden" });

			todo.IsCompleted = request.IsCompleted;
			todo.UpdatedAt = DateTime.UtcNow;

			await _context.SaveChangesAsync();

			return Ok(MapToResponse(todo));
		}

		// DELETE: api/todos/{id}
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteTodo(Guid id)
		{
			var userId = GetCurrentUserId();
			var todo = await _context.Todos.FirstOrDefaultAsync(t => t.Id == id);

			if (todo == null) return NotFound();
			if (todo.UserId != userId) return StatusCode(403, new { message = "Forbidden" });

			_context.Todos.Remove(todo);
			await _context.SaveChangesAsync();

			return NoContent(); // 204 No Content
		}

		private static TodoResponse MapToResponse(TodoItem todo)
		{
			return new TodoResponse
			{
				Id = todo.Id,
				Title = todo.Title,
				Details = todo.Details,
				Priority = todo.Priority.ToString(),
				DueDate = todo.DueDate?.ToString("yyyy-MM-dd"),
				IsCompleted = todo.IsCompleted,
				IsPublic = todo.IsPublic,
				CreatedAt = todo.CreatedAt,
				UpdatedAt = todo.UpdatedAt
			};
		}
	}
}