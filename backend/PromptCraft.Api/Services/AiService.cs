using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using PromptCraft.Api.Models;

namespace PromptCraft.Api.Services;

public class AiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ModelConfigService _modelConfigService;
    private readonly ILogger<AiService> _logger;

    public AiService(
        HttpClient httpClient,
        IConfiguration config,
        ModelConfigService modelConfigService,
        ILogger<AiService> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _modelConfigService = modelConfigService;
        _logger = logger;
    }

    public async Task<AiResponse> ChatAsync(AiRequest request)
    {
        var sw = Stopwatch.StartNew();

        // Get model config from database
        var modelConfig = await _modelConfigService.GetByModelIdAsync(request.Model);

        string endpoint;
        string apiKey;

        if (modelConfig != null)
        {
            endpoint = modelConfig.ApiEndpoint;
            apiKey = modelConfig.ApiKey;
        }
        else
        {
            // Fallback to appsettings
            endpoint = GetDefaultEndpoint(request.Model);
            apiKey = GetDefaultApiKey(request.Model);
        }

        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException($"未配置模型 {request.Model} 的 API Key，请在模型管理中配置");
        }

        var messages = new List<object>();
        if (!string.IsNullOrEmpty(request.SystemMessage))
            messages.Add(new { role = "system", content = request.SystemMessage });
        messages.Add(new { role = "user", content = request.Prompt });

        var body = new
        {
            model = request.Model,
            messages,
            temperature = request.Temperature,
            max_tokens = request.MaxTokens
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        httpRequest.Content = new StringContent(
            JsonSerializer.Serialize(body),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.SendAsync(httpRequest);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var content = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? "";

        var tokens = 0;
        if (doc.RootElement.TryGetProperty("usage", out var usage) &&
            usage.TryGetProperty("total_tokens", out var totalTokens))
        {
            tokens = totalTokens.GetInt32();
        }

        sw.Stop();

        return new AiResponse
        {
            Content = content,
            TokensUsed = tokens,
            LatencyMs = (int)sw.ElapsedMilliseconds,
            Model = request.Model
        };
    }

    public async Task<PromptScoreResponse> ScorePromptAsync(PromptScoreRequest request)
    {
        var systemPrompt = @"你是一个 Prompt 工程专家。请评估以下 Prompt 的质量。

评分维度（每项 1-25 分，总分 100）：
1. 清晰度 (Clarity)：指令是否明确无歧义
2. 具体性 (Specificity)：是否包含足够的细节和约束
3. 结构性 (Structure)：格式是否清晰，逻辑是否连贯
4. 完整性 (Completeness)：是否覆盖了所有必要信息

请严格按以下 JSON 格式返回：
{
  ""totalScore"": 85,
  ""clarity"": 22,
  ""specificity"": 20,
  ""structure"": 23,
  ""completeness"": 20,
  ""strengths"": [""优点1"", ""优点2""],
  ""improvements"": [""改进建议1"", ""改进建议2""],
  ""optimizedPrompt"": ""优化后的 Prompt（如果需要）""
}";

        var userMsg = string.IsNullOrEmpty(request.Task)
            ? $"请评估这个 Prompt：\n\n{request.Prompt}"
            : $"请评估这个 Prompt（用途：{request.Task}）：\n\n{request.Prompt}";

        var defaultModel = await GetDefaultModelAsync();

        var response = await ChatAsync(new AiRequest
        {
            Model = defaultModel,
            SystemMessage = systemPrompt,
            Prompt = userMsg,
            Temperature = 0.3
        });

        return JsonSerializer.Deserialize<PromptScoreResponse>(response.Content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new PromptScoreResponse();
    }

    public async Task<AiResponse> OptimizePromptAsync(OptimizeRequest request)
    {
        var systemPrompt = @"你是一个 Prompt 优化专家。请根据用户的目标优化 Prompt。

要求：
1. 保持原始意图不变
2. 增加清晰度和具体性
3. 添加必要的约束和格式要求
4. 使用结构化格式（如 Markdown）
5. 如果适用，添加角色设定

直接返回优化后的 Prompt，不要加额外解释。";

        var userMsg = $"请优化以下 Prompt：\n\n{request.Prompt}";
        if (!string.IsNullOrEmpty(request.Goal))
            userMsg += $"\n\n优化目标：{request.Goal}";
        if (!string.IsNullOrEmpty(request.TargetModel))
            userMsg += $"\n\n目标模型：{request.TargetModel}";

        var defaultModel = await GetDefaultModelAsync();

        return await ChatAsync(new AiRequest
        {
            Model = defaultModel,
            SystemMessage = systemPrompt,
            Prompt = userMsg,
            Temperature = 0.5
        });
    }

    public async Task<CompareResponse> CompareModelsAsync(CompareRequest request)
    {
        var results = new List<ModelOutput>();

        foreach (var model in request.Models)
        {
            try
            {
                var response = await ChatAsync(new AiRequest
                {
                    Model = model,
                    Prompt = request.Input,
                    SystemMessage = request.Prompt
                });

                results.Add(new ModelOutput
                {
                    Model = model,
                    Output = response.Content,
                    TokensUsed = response.TokensUsed,
                    LatencyMs = response.LatencyMs
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling model {Model}", model);
                results.Add(new ModelOutput
                {
                    Model = model,
                    Output = $"Error: {ex.Message}",
                    TokensUsed = 0,
                    LatencyMs = 0
                });
            }
        }

        return new CompareResponse { Results = results };
    }

    private async Task<string> GetDefaultModelAsync()
    {
        var models = await _modelConfigService.GetAllAsync();
        var activeModel = models.FirstOrDefault(m => m.IsActive && !string.IsNullOrEmpty(m.ApiKey));
        return activeModel?.ModelId ?? _config.GetValue<string>("AI:DefaultModel") ?? "deepseek-chat";
    }

    private string GetDefaultEndpoint(string model)
    {
        return model switch
        {
            var m when m.StartsWith("deepseek") => "https://api.deepseek.com/v1/chat/completions",
            var m when m.StartsWith("qwen") => "https://dashscope.aliyuncs.com/compatible-mode/v1/chat/completions",
            var m when m.StartsWith("gpt") => "https://api.openai.com/v1/chat/completions",
            _ => "https://api.deepseek.com/v1/chat/completions"
        };
    }

    private string GetDefaultApiKey(string model)
    {
        return model switch
        {
            var m when m.StartsWith("deepseek") => _config.GetValue<string>("AI:DeepSeekApiKey") ?? "",
            var m when m.StartsWith("qwen") => _config.GetValue<string>("AI:QwenApiKey") ?? "",
            var m when m.StartsWith("gpt") => _config.GetValue<string>("AI:OpenAIApiKey") ?? "",
            _ => _config.GetValue<string>("AI:DeepSeekApiKey") ?? ""
        };
    }
}
