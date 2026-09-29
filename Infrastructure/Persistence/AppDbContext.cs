using Domain._Common;
using Domain.ConfigurationAggregate.Models;
using Domain.LlmAggregate.Models;
using Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using ConfigurationModel = Domain.ConfigurationAggregate.Models.Configuration;
using ProjectAnalysisModel = Domain.ProjectAnalysisAggregate.Models.ProjectAnalysis;

namespace Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    private readonly PublishDomainEventInterceptor _publishDomainEventInterceptor;
    private readonly SaveTimeStampsInterceptor _saveTimeStampsInterceptor;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        PublishDomainEventInterceptor publishDomainEventInterceptor,
        SaveTimeStampsInterceptor saveTimeStampsInterceptor) : base(options)
    {
        _publishDomainEventInterceptor = publishDomainEventInterceptor;
        _saveTimeStampsInterceptor = saveTimeStampsInterceptor;
    }

    public DbSet<LlmModel> LlmModels => Set<LlmModel>();
    public DbSet<ConfigurationModel> Configurations => Set<ConfigurationModel>();
    public DbSet<PromptConfiguration> PromptConfigurations => Set<PromptConfiguration>();
    public DbSet<PromptTemplate> PromptTemplates => Set<PromptTemplate>();
    public DbSet<ProjectAnalysisModel> ProjectAnalyses => Set<ProjectAnalysisModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Ignore<List<IDomainEvent>>()
            .ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_publishDomainEventInterceptor, _saveTimeStampsInterceptor);
        base.OnConfiguring(optionsBuilder);
    }
}
