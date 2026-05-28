using Microsoft.EntityFrameworkCore;
using PromptCraft.Api.Data;
using PromptCraft.Api.Endpoints;
using PromptCraft.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=promptcraft.db"));

// Services
builder.Services.AddHttpClient<AiService>();
builder.Services.AddScoped<PromptService>();
builder.Services.AddScoped<ModelConfigService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

// Auto-migrate database and seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    // Seed default models
    var modelConfigService = scope.ServiceProvider.GetRequiredService<ModelConfigService>();
    await modelConfigService.SeedDefaultModelsAsync();
}

// Middleware
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();

// Map endpoints
app.MapPromptEndpoints();
app.MapAiEndpoints();
app.MapCategoryEndpoints();
app.MapModelConfigEndpoints();

// Health check
app.MapGet("/api/health", () => Results.Ok(new { status = "healthy", time = DateTime.UtcNow }));

app.Run();
