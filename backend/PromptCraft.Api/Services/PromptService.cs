using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PromptCraft.Api.Data;
using PromptCraft.Api.Models;

namespace PromptCraft.Api.Services;

public class PromptService
{
    private readonly AppDbContext _db;

    public PromptService(AppDbContext db) => _db = db;

    public async Task<List<Prompt>> GetAllAsync(int? categoryId = null, string? search = null, bool? favorite = null)
    {
        var query = _db.Prompts.Include(p => p.Category).AsQueryable();

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (!string.IsNullOrEmpty(search))
            query = query.Where(p => p.Title.Contains(search) || p.Content.Contains(search));

        if (favorite == true)
            query = query.Where(p => p.IsFavorite);

        return await query.OrderByDescending(p => p.UpdatedAt).ToListAsync();
    }

    public async Task<Prompt?> GetByIdAsync(int id)
    {
        return await _db.Prompts
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Prompt> CreateAsync(Prompt prompt)
    {
        prompt.CreatedAt = DateTime.UtcNow;
        prompt.UpdatedAt = DateTime.UtcNow;
        _db.Prompts.Add(prompt);
        await _db.SaveChangesAsync();

        // Save initial version
        await SaveVersionAsync(prompt.Id, prompt.Content, "初始版本");

        return prompt;
    }

    public async Task<Prompt?> UpdateAsync(int id, Prompt updated)
    {
        var prompt = await _db.Prompts.FindAsync(id);
        if (prompt == null) return null;

        var changeNote = prompt.Content != updated.Content ? "内容更新" : "信息更新";

        prompt.Title = updated.Title;
        prompt.Content = updated.Content;
        prompt.Description = updated.Description;
        prompt.CategoryId = updated.CategoryId;
        prompt.Variables = updated.Variables;
        prompt.Tags = updated.Tags;
        prompt.ModelId = updated.ModelId;
        prompt.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        if (changeNote == "内容更新")
            await SaveVersionAsync(id, updated.Content, changeNote);

        return prompt;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var prompt = await _db.Prompts.FindAsync(id);
        if (prompt == null) return false;

        _db.Prompts.Remove(prompt);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleFavoriteAsync(int id)
    {
        var prompt = await _db.Prompts.FindAsync(id);
        if (prompt == null) return false;

        prompt.IsFavorite = !prompt.IsFavorite;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task IncrementUsageAsync(int id)
    {
        var prompt = await _db.Prompts.FindAsync(id);
        if (prompt != null)
        {
            prompt.UsageCount++;
            await _db.SaveChangesAsync();
        }
    }

    public async Task UpdateScoreAsync(int id, double score)
    {
        var prompt = await _db.Prompts.FindAsync(id);
        if (prompt != null)
        {
            prompt.AvgScore = prompt.AvgScore == null
                ? score
                : (prompt.AvgScore.Value + score) / 2;
            await _db.SaveChangesAsync();
        }
    }

    private async Task SaveVersionAsync(int promptId, string content, string changeNote)
    {
        var versionCount = await _db.PromptVersions
            .CountAsync(v => v.PromptId == promptId);

        _db.PromptVersions.Add(new PromptVersion
        {
            PromptId = promptId,
            Content = content,
            ChangeNote = changeNote,
            VersionNumber = versionCount + 1
        });

        await _db.SaveChangesAsync();
    }

    public async Task<List<PromptVersion>> GetVersionsAsync(int promptId)
    {
        return await _db.PromptVersions
            .Where(v => v.PromptId == promptId)
            .OrderByDescending(v => v.VersionNumber)
            .ToListAsync();
    }

    // Category methods
    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await _db.Categories.ToListAsync();
    }

    public async Task<Category> CreateCategoryAsync(Category category)
    {
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return category;
    }

    // Import/Export
    public async Task<string> ExportAsync(int? categoryId = null)
    {
        var prompts = await GetAllAsync(categoryId);
        var export = prompts.Select(p => new
        {
            p.Title,
            p.Content,
            p.Description,
            Category = p.Category.Name,
            p.Tags,
            p.Variables
        });

        return JsonSerializer.Serialize(export, new JsonSerializerOptions { WriteIndented = true });
    }

    public async Task<int> ImportAsync(string json)
    {
        var items = JsonSerializer.Deserialize<List<ImportItem>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (items == null) return 0;

        var categories = await _db.Categories.ToListAsync();
        int count = 0;

        foreach (var item in items)
        {
            var category = categories.FirstOrDefault(c => c.Name == item.Category)
                ?? categories.First();

            _db.Prompts.Add(new Prompt
            {
                Title = item.Title,
                Content = item.Content,
                Description = item.Description,
                CategoryId = category.Id,
                Tags = item.Tags,
                Variables = item.Variables
            });
            count++;
        }

        await _db.SaveChangesAsync();
        return count;
    }

    private record ImportItem(string Title, string Content, string? Description, string? Category, string? Tags, string? Variables);
}
