using PromptCraft.Api.Models;
using PromptCraft.Api.Services;

namespace PromptCraft.Api.Endpoints;

public static class AiEndpoints
{
    public static void MapAiEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/ai").WithTags("AI");

        // POST chat
        group.MapPost("/chat", async (AiService service, AiRequest request) =>
        {
            try
            {
                var response = await service.ChatAsync(request);
                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        // POST score prompt
        group.MapPost("/score", async (AiService service, PromptService promptService, PromptScoreRequest request) =>
        {
            try
            {
                var result = await service.ScorePromptAsync(request);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        // POST optimize prompt
        group.MapPost("/optimize", async (AiService service, OptimizeRequest request) =>
        {
            try
            {
                var result = await service.OptimizePromptAsync(request);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        // POST compare models
        group.MapPost("/compare", async (AiService service, CompareRequest request) =>
        {
            try
            {
                var result = await service.CompareModelsAsync(request);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        // GET available models
        group.MapGet("/models", (IConfiguration config) =>
        {
            var models = new List<object>
            {
                new { id = "deepseek-chat", name = "DeepSeek Chat", provider = "DeepSeek" },
                new { id = "deepseek-reasoner", name = "DeepSeek Reasoner", provider = "DeepSeek" },
                new { id = "qwen-plus", name = "通义千问 Plus", provider = "Alibaba" },
                new { id = "qwen-turbo", name = "通义千问 Turbo", provider = "Alibaba" },
                new { id = "gpt-4o-mini", name = "GPT-4o Mini", provider = "OpenAI" },
                new { id = "gpt-4o", name = "GPT-4o", provider = "OpenAI" },
            };
            return Results.Ok(models);
        });
    }
}
