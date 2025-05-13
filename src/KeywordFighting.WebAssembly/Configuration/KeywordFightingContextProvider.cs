using KeywordFighting.Model;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace KeywordFighting.Configuration;

public class KeywordFightingContextProvider(IServiceScopeFactory factory, IOptionsMonitor<AppOptions> options) : IKeywordFightingContextProvider
{
    public async Task<KeywordFightingContext?> GetAsync(CancellationToken cancellationToken) =>
        _context ??= await LoadAsync(cancellationToken);

    private KeywordFightingContext? _context;

    private async Task<KeywordFightingContext?> LoadAsync(CancellationToken cancellationToken)
    {
        using var scope = factory.CreateScope();
        using var client = scope.ServiceProvider.GetRequiredService<HttpClient>();

        var context = await client.GetFromJsonAsync<KeywordFightingContext>(options.CurrentValue.ContextPath, cancellationToken);
        if (context == null)
            return null;

        return context with
        {
            Equipments = context.Equipments
                .Select(equipment => equipment.Cast())
                .ToArray()
        };
    }
}
