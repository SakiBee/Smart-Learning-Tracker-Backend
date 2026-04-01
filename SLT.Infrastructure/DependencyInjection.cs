using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SLT.Core.Interfaces;
using SLT.Infrastructure.Data;
using SLT.Infrastructure.Repositories;
using SLT.Infrastructure.Services;

namespace SLT.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Repositories
        services.AddScoped<ILearningEntryRepository, LearningEntryRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        // Auth
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // HTTP Clients
        services.AddHttpClient("Anthropic", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddHttpClient("Jina", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/plain");
        });

        services.AddHttpClient("Extractor", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(20);
        });

        services.AddHttpClient("Groq", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // AI Services
        services.AddScoped<IContentExtractorService, ContentExtractorService>();
        services.AddScoped<IAiSummaryService, AiSummaryService>();
        services.AddScoped<IFlashcardRepository, FlashcardRepository>();
        services.AddScoped<IFlashcardGeneratorService, FlashcardGeneratorService>();
        services.AddScoped<ICollectionRepository, CollectionRepository>();
        services.AddScoped<IQuoteRepository, QuoteRepository>();
        return services;
    }
}