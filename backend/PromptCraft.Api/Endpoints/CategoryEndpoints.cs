using PromptCraft.Api.Models;
using PromptCraft.Api.Services;

namespace PromptCraft.Api.Endpoints;

public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/categories").WithTags("Categories");

        group.MapGet("/", async (PromptService service) =>
        {
            return await service.GetCategoriesAsync();
        });

        group.MapPost("/", async (PromptService service, Category category) =>
        {
            var created = await service.CreateCategoryAsync(category);
            return Results.Created($"/api/categories/{created.Id}", created);
        });
    }
}
