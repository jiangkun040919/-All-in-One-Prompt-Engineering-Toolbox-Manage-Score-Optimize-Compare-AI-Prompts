using PromptCraft.Api.Models;
using PromptCraft.Api.Services;

namespace PromptCraft.Api.Endpoints;

public static class PromptEndpoints
{
    public static void MapPromptEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/prompts").WithTags("Prompts");

        // GET all prompts
        group.MapGet("/", async (PromptService service, int? categoryId, string? search, bool? favorite) =>
        {
            return await service.GetAllAsync(categoryId, search, favorite);
        });

        // GET prompt by id
        group.MapGet("/{id:int}", async (PromptService service, int id) =>
        {
            var prompt = await service.GetByIdAsync(id);
            return prompt is not null ? Results.Ok(prompt) : Results.NotFound();
        });

        // POST create prompt
        group.MapPost("/", async (PromptService service, Prompt prompt) =>
        {
            var created = await service.CreateAsync(prompt);
            return Results.Created($"/api/prompts/{created.Id}", created);
        });

        // PUT update prompt
        group.MapPut("/{id:int}", async (PromptService service, int id, Prompt prompt) =>
        {
            var updated = await service.UpdateAsync(id, prompt);
            return updated is not null ? Results.Ok(updated) : Results.NotFound();
        });

        // DELETE prompt
        group.MapDelete("/{id:int}", async (PromptService service, int id) =>
        {
            var deleted = await service.DeleteAsync(id);
            return deleted ? Results.NoContent() : Results.NotFound();
        });

        // POST toggle favorite
        group.MapPost("/{id:int}/favorite", async (PromptService service, int id) =>
        {
            var result = await service.ToggleFavoriteAsync(id);
            return result ? Results.Ok() : Results.NotFound();
        });

        // GET versions
        group.MapGet("/{id:int}/versions", async (PromptService service, int id) =>
        {
            return await service.GetVersionsAsync(id);
        });

        // GET export
        group.MapGet("/export", async (PromptService service, int? categoryId) =>
        {
            var json = await service.ExportAsync(categoryId);
            return Results.Content(json, "application/json");
        });

        // POST import
        group.MapPost("/import", async (PromptService service, HttpRequest request) =>
        {
            using var reader = new StreamReader(request.Body);
            var json = await reader.ReadToEndAsync();
            var count = await service.ImportAsync(json);
            return Results.Ok(new { imported = count });
        });
    }
}
