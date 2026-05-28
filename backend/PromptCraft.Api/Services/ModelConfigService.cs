using Microsoft.EntityFrameworkCore;
using PromptCraft.Api.Data;
using PromptCraft.Api.Models;

namespace PromptCraft.Api.Services;

public class ModelConfigService
{
    private readonly AppDbContext _db;

    public ModelConfigService(AppDbContext db) => _db = db;

    public async Task<List<ModelConfig>> GetAllAsync()
    {
        return await _db.ModelConfigs.OrderBy(m => m.Provider).ThenBy(m => m.Name).ToListAsync();
    }

    public async Task<ModelConfig?> GetByIdAsync(int id)
    {
        return await _db.ModelConfigs.FindAsync(id);
    }

    public async Task<ModelConfig?> GetByModelIdAsync(string modelId)
    {
        return await _db.ModelConfigs.FirstOrDefaultAsync(m => m.ModelId == modelId && m.IsActive);
    }

    public async Task<ModelConfig> CreateAsync(AddModelRequest request)
    {
        var config = new ModelConfig
        {
            ModelId = request.ModelId,
            Name = request.Name,
            Provider = request.Provider,
            ApiEndpoint = request.ApiEndpoint,
            ApiKey = request.ApiKey,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.ModelConfigs.Add(config);
        await _db.SaveChangesAsync();
        return config;
    }

    public async Task<ModelConfig?> UpdateAsync(int id, UpdateModelRequest request)
    {
        var config = await _db.ModelConfigs.FindAsync(id);
        if (config == null) return null;

        if (request.Name != null) config.Name = request.Name;
        if (request.ApiEndpoint != null) config.ApiEndpoint = request.ApiEndpoint;
        if (request.ApiKey != null) config.ApiKey = request.ApiKey;
        if (request.IsActive.HasValue) config.IsActive = request.IsActive.Value;

        await _db.SaveChangesAsync();
        return config;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var config = await _db.ModelConfigs.FindAsync(id);
        if (config == null) return false;

        _db.ModelConfigs.Remove(config);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleActiveAsync(int id)
    {
        var config = await _db.ModelConfigs.FindAsync(id);
        if (config == null) return false;

        config.IsActive = !config.IsActive;
        await _db.SaveChangesAsync();
        return true;
    }

    // Seed default models
    public async Task SeedDefaultModelsAsync()
    {
        if (await _db.ModelConfigs.AnyAsync()) return;

        var defaults = new List<ModelConfig>
        {
            new()
            {
                ModelId = "deepseek-chat",
                Name = "DeepSeek Chat",
                Provider = "DeepSeek",
                ApiEndpoint = "https://api.deepseek.com/v1/chat/completions",
                ApiKey = "",
                IsActive = true
            },
            new()
            {
                ModelId = "deepseek-reasoner",
                Name = "DeepSeek Reasoner",
                Provider = "DeepSeek",
                ApiEndpoint = "https://api.deepseek.com/v1/chat/completions",
                ApiKey = "",
                IsActive = true
            },
            new()
            {
                ModelId = "qwen-plus",
                Name = "通义千问 Plus",
                Provider = "Alibaba",
                ApiEndpoint = "https://dashscope.aliyuncs.com/compatible-mode/v1/chat/completions",
                ApiKey = "",
                IsActive = true
            },
            new()
            {
                ModelId = "qwen-turbo",
                Name = "通义千问 Turbo",
                Provider = "Alibaba",
                ApiEndpoint = "https://dashscope.aliyuncs.com/compatible-mode/v1/chat/completions",
                ApiKey = "",
                IsActive = true
            },
            new()
            {
                ModelId = "gpt-4o-mini",
                Name = "GPT-4o Mini",
                Provider = "OpenAI",
                ApiEndpoint = "https://api.openai.com/v1/chat/completions",
                ApiKey = "",
                IsActive = true
            },
            new()
            {
                ModelId = "gpt-4o",
                Name = "GPT-4o",
                Provider = "OpenAI",
                ApiEndpoint = "https://api.openai.com/v1/chat/completions",
                ApiKey = "",
                IsActive = true
            }
        };

        _db.ModelConfigs.AddRange(defaults);
        await _db.SaveChangesAsync();
    }
}
