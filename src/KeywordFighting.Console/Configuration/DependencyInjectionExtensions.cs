using Microsoft.Extensions.DependencyInjection;

namespace KeywordFighting.Configuration;
public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddKeywordFighting(this IServiceCollection services) => services
        .AddSingleton<ICancellationTokenSource, RenewableCancellationTokenSource>()
        .AddSingleton<IKeywordFightingContextProvider, KeywordFightingContextProvider>();
}