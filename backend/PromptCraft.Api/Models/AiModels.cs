namespace PromptCraft.Api.Models;

public class AiRequest
{
    public string Model { get; set; } = "deepseek-chat";
    public string Prompt { get; set; } = string.Empty;
    public string? SystemMessage { get; set; }
    public double Temperature { get; set; } = 0.7;
    public int MaxTokens { get; set; } = 2000;
}

public class AiResponse
{
    public string Content { get; set; } = string.Empty;
    public int TokensUsed { get; set; }
    public int LatencyMs { get; set; }
    public string Model { get; set; } = string.Empty;
}

public class PromptScoreRequest
{
    public string Prompt { get; set; } = string.Empty;
    public string? Task { get; set; } // What the prompt is supposed to do
}

public class PromptScoreResponse
{
    public int TotalScore { get; set; } // 1-100
    public int Clarity { get; set; }
    public int Specificity { get; set; }
    public int Structure { get; set; }
    public int Completeness { get; set; }
    public List<string> Strengths { get; set; } = new();
    public List<string> Improvements { get; set; } = new();
    public string? OptimizedPrompt { get; set; }
}

public class OptimizeRequest
{
    public string Prompt { get; set; } = string.Empty;
    public string? Goal { get; set; } // What to improve
    public string? TargetModel { get; set; }
}

public class CompareRequest
{
    public string Prompt { get; set; } = string.Empty;
    public string Input { get; set; } = string.Empty;
    public List<string> Models { get; set; } = new();
}

public class CompareResponse
{
    public List<ModelOutput> Results { get; set; } = new();
}

public class ModelOutput
{
    public string Model { get; set; } = string.Empty;
    public string Output { get; set; } = string.Empty;
    public int TokensUsed { get; set; }
    public int LatencyMs { get; set; }
    public int? Score { get; set; }
}
