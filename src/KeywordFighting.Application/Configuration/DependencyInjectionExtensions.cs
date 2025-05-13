using Microsoft.Extensions.DependencyInjection;

namespace KeywordFighting.Configuration;
public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddKeywordFightingApplication(this IServiceCollection services) => services
        .AddScoped<IGameContextProvider, GameContextProvider>()
        .AddScoped<IGameEngine, GameEngine>();
}