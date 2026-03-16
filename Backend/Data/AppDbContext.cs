using Microsoft.EntityFrameworkCore;
using Todo.Api.Models;

namespace Todo.Api.Data
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
		{
		}

		public DbSet<User> Users { get; set; }
		public DbSet<TodoItem> Todos { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// Enforce unique email constraint at the database level
			modelBuilder.Entity<User>()
				.HasIndex(u => u.Email)
				.IsUnique();

			// Configure the One-to-Many relationship (User -> TodoItems)
			modelBuilder.Entity<TodoItem>()
				.HasOne(t => t.User)
				.WithMany(u => u.Todos)
				.HasForeignKey(t => t.UserId)
				.OnDelete(DeleteBehavior.Cascade); // Delete user's todos if user is deleted
		}
	}
}