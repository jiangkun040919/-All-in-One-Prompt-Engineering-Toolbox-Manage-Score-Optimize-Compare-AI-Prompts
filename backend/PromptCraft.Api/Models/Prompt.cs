namespace PromptCraft.Api.Models;

public class Prompt
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public string? Variables { get; set; } // JSON array of variable names
    public string? Tags { get; set; } // JSON array of tags
    public int UsageCount { get; set; }
    public double? AvgScore { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsFavorite { get; set; }
    public string? ModelId { get; set; } // Preferred model
}

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Color { get; set; }
}

public class PromptVersion
{
    public int Id { get; set; }
    public int PromptId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? ChangeNote { get; set; }
    public int VersionNumber { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
