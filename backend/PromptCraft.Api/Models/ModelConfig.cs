namespace PromptCraft.Api.Models;

public class ModelConfig
{
    public int Id { get; set; }
    public string ModelId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string ApiEndpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class AddModelRequest
{
    public string ModelId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string ApiEndpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}

public class UpdateModelRequest
{
    public string? Name { get; set; }
    public string? ApiEndpoint { get; set; }
    public string? ApiKey { get; set; }
    public bool? IsActive { get; set; }
}
