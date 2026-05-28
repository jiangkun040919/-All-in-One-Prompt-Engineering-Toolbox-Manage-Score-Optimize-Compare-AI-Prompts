namespace PromptCraft.Api.Models;

public class TestResult
{
    public int Id { get; set; }
    public int PromptId { get; set; }
    public string ModelId { get; set; } = string.Empty;
    public string Input { get; set; } = string.Empty;
    public string Output { get; set; } = string.Empty;
    public int? Score { get; set; } // 1-10
    public string? ScoreDetails { get; set; } // JSON: clarity, specificity, relevance
    public int TokensUsed { get; set; }
    public int LatencyMs { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ABTestResult
{
    public int Id { get; set; }
    public int PromptId { get; set; }
    public string ModelA { get; set; } = string.Empty;
    public string ModelB { get; set; } = string.Empty;
    public string Input { get; set; } = string.Empty;
    public string OutputA { get; set; } = string.Empty;
    public string OutputB { get; set; } = string.Empty;
    public int? ScoreA { get; set; }
    public int? ScoreB { get; set; }
    public string? Winner { get; set; } // "A" or "B"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
