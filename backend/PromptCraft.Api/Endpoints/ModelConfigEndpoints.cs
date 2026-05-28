using PromptCraft.Api.Models;
using PromptCraft.Api.Services;

namespace PromptCraft.Api.Endpoints;

public static class ModelConfigEndpoints
{
    public static void MapModelConfigEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/models").WithTags("Models");

        // GET all models
        group.MapGet("/", async (ModelConfigService service) =>
        {
            var models = await service.GetAllAsync();
            // Mask API keys in response
            return models.Select(m => new
            {
                m.Id,
                m.ModelId,
                m.Name,
                m.Provider,
                m.ApiEndpoint,
                HasApiKey = !string.IsNullOrEmpty(m.ApiKey),
                m.IsActive,
                m.CreatedAt
            });
        });

        // GET model by id
        group.MapGet("/{id:int}", async (ModelConfigService service, int id) =>
        {
            var model = await service.GetByIdAsync(id);
            if (model == null) return Results.NotFound();

            return Results.Ok(new
            {
                model.Id,
                model.ModelId,
                model.Name,
                model.Provider,
                model.ApiEndpoint,
                HasApiKey = !string.IsNullOrEmpty(model.ApiKey),
                model.IsActive,
                model.CreatedAt
            });
        });

        // POST add model
        group.MapPost("/", async (ModelConfigService service, AddModelRequest request) =>
        {
            var existing = await service.GetByModelIdAsync(request.ModelId);
            if (existing != null)
                return Results.BadRequest(new { error = "模型 ID 已存在" });

            var created = await service.CreateAsync(request);
            return Results.Created($"/api/models/{created.Id}", new
            {
                created.Id,
                created.ModelId,
                created.Name,
                created.Provider,
                created.ApiEndpoint,
                HasApiKey = !string.IsNullOrEmpty(created.ApiKey),
                created.IsActive,
                created.CreatedAt
            });
        });

        // PUT update model
        group.MapPut("/{id:int}", async (ModelConfigService service, int id, UpdateModelRequest request) =>
        {
            var updated = await service.UpdateAsync(id, request);
            return updated != null ? Results.Ok(updated) : Results.NotFound();
        });

        // DELETE model
        group.MapDelete("/{id:int}", async (ModelConfigService service, int id) =>
        {
            var deleted = await service.DeleteAsync(id);
            return deleted ? Results.NoContent() : Results.NotFound();
        });

        // POST toggle active
        group.MapPost("/{id:int}/toggle", async (ModelConfigService service, int id) =>
        {
            var result = await service.ToggleActiveAsync(id);
            return result ? Results.Ok() : Results.NotFound();
        });

        // GET available providers
        group.MapGet("/providers", () =>
        {
            return Results.Ok(new[]
            {
                new { Id = "deepseek", Name = "DeepSeek", DefaultEndpoint = "https://api.deepseek.com/v1/chat/completions" },
                new { Id = "alibaba", Name = "阿里巴巴 (通义千问)", DefaultEndpoint = "https://dashscope.aliyuncs.com/compatible-mode/v1/chat/completions" },
                new { Id = "openai", Name = "OpenAI", DefaultEndpoint = "https://api.openai.com/v1/chat/completions" },
                new { Id = "custom", Name = "自定义", DefaultEndpoint = "" }
            });
        });

        // POST test connection
        group.MapPost("/{id:int}/test", async (ModelConfigService service, AiService aiService, int id) =>
        {
            var model = await service.GetByIdAsync(id);
            if (model == null) return Results.NotFound();

            if (string.IsNullOrEmpty(model.ApiKey))
                return Results.BadRequest(new { error = "未配置 API Key" });

            try
            {
                var response = await aiService.ChatAsync(new AiRequest
                {
                    Model = model.ModelId,
                    Prompt = "Hello, respond with 'OK' only.",
                    MaxTokens = 10
                });

                return Results.Ok(new
                {
                    success = true,
                    latencyMs = response.LatencyMs,
                    message = "连接成功"
                });
            }
            catch (Exception ex)
            {
                return Results.Ok(new
                {
                    success = false,
                    message = $"连接失败: {ex.Message}"
                });
            }
        });
    }
}
