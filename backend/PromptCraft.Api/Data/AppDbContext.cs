using Microsoft.EntityFrameworkCore;
using PromptCraft.Api.Models;

namespace PromptCraft.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Prompt> Prompts => Set<Prompt>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<PromptVersion> PromptVersions => Set<PromptVersion>();
    public DbSet<TestResult> TestResults => Set<TestResult>();
    public DbSet<ABTestResult> ABTestResults => Set<ABTestResult>();
    public DbSet<ModelConfig> ModelConfigs => Set<ModelConfig>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Prompt>()
            .HasOne(p => p.Category)
            .WithMany()
            .HasForeignKey(p => p.CategoryId);

        modelBuilder.Entity<PromptVersion>()
            .HasOne<Prompt>()
            .WithMany()
            .HasForeignKey(v => v.PromptId);

        // Seed default categories
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "写作", Icon = "edit", Color = "#409EFF" },
            new Category { Id = 2, Name = "编程", Icon = "code", Color = "#67C23A" },
            new Category { Id = 3, Name = "分析", Icon = "data-analysis", Color = "#E6A23C" },
            new Category { Id = 4, Name = "翻译", Icon = "translate", Color = "#F56C6C" },
            new Category { Id = 5, Name = "创意", Icon = "magic-stick", Color = "#909399" },
            new Category { Id = 6, Name = "角色扮演", Icon = "user", Color = "#b37feb" },
            new Category { Id = 7, Name = "其他", Icon = "more", Color = "#606266" }
        );
    }
}
