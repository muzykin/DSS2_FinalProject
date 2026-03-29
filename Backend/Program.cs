using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using System.Text.Json.Serialization;
using Todo.Api.Data;
using Todo.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Register DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Configure Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
	options.Configuration = builder.Configuration.GetConnectionString("Redis") ?? "redis:6379";
	options.InstanceName = "TodoApp_";
});

// 3. Configure RabbitMQ Service
builder.Services.AddSingleton<IRabbitMqService, RabbitMqService>();

// 4. Configure JWT Authentication
var jwtKey = builder.Configuration["JwtSettings:SecretKey"] ?? "ThisIsAVerySecretKeyForTodoApiProjectDDS2DoNotShare";
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = false, // Отключаем проверку
			ValidateAudience = false, // Отключаем проверку
			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,
			IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
		};
	});

// 5. Configure CORS
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAll", policy =>
	{
		policy.SetIsOriginAllowed(origin => true) // Разрешаем любые порты фронтенда
			  .AllowAnyMethod()
			  .AllowAnyHeader()
			  .AllowCredentials(); // Обязательно для токенов
	});
});

// 6. Configure Controllers and JSON Options
builder.Services.AddControllers().AddJsonOptions(options =>
{
	options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
	options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(System.Text.Json.JsonNamingPolicy.CamelCase));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
	c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
	{
		Description = "JWT Authorization header using the Bearer scheme.",
		Name = "Authorization",
		In = Microsoft.OpenApi.Models.ParameterLocation.Header,
		Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
		Scheme = "Bearer"
	});

	c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement()
	{
		{
			new Microsoft.OpenApi.Models.OpenApiSecurityScheme
			{
				Reference = new Microsoft.OpenApi.Models.OpenApiReference { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" },
				Scheme = "oauth2", Name = "Bearer", In = Microsoft.OpenApi.Models.ParameterLocation.Header,
			},
			new List<string>()
		}
	});
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// ПОРЯДОК ВАЖЕН
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
	var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

	int maxRetries = 5;
	for (int i = 0; i < maxRetries; i++)
	{
		try
		{
			dbContext.Database.EnsureCreated();
			break;
		}
		catch (Exception ex)
		{
			if (i == maxRetries - 1) throw;
			Console.WriteLine($"Database not ready yet, retrying in 2 seconds... (Attempt {i + 1}/{maxRetries})");
			Thread.Sleep(2000);
		}
	}
}

app.Run();