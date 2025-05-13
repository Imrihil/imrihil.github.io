namespace KeywordFighting.Configuration;
public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddKeywordFighting(this IServiceCollection services) => services
        .AddSingleton<IKeywordFightingContextProvider, KeywordFightingContextProvider>();
}