using Infrastructure.Git;
using Infrastructure.Llm;
using Infrastructure.Llm.Providers;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Interceptors;
using Infrastructure.ProjectAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Usecases.Common;
using Usecases.Git;
using Usecases.Llm;
using Usecases.ProjectAnalysis;

namespace Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string sqliteConnectionString)
    {
        services.AddSingleton<PublishDomainEventInterceptor>();
        services.AddSingleton<SaveTimeStampsInterceptor>();

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(sqliteConnectionString));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddSingleton<IGitService, GitService>();
        services.AddSingleton<IProjectAnalysisWriter, ProjectAnalysisFileWriter>();

        services.AddHttpClient(OpenAiLlmProvider.HttpClientName);
        services.AddHttpClient(GeminiLlmProvider.HttpClientName);

        services.AddSingleton<ILlmProvider, OpenAiLlmProvider>();
        services.AddSingleton<ILlmProvider, GeminiLlmProvider>();
        services.AddSingleton<ILlmClient, LlmClient>();

        return services;
    }

    public static async Task EnsureDatabaseAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        // Schema bootstrap for SQLite (no EF migrations package in v1).
        // Supported LLM *providers* are not seeded into the DB — they live in Domain
        // as LlmProviderType + LlmProviderCatalog. User models are added via `llm add`.
        await db.Database.EnsureCreatedAsync();
    }
}
