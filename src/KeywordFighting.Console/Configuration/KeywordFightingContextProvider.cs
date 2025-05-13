using KeywordFighting.Model;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace KeywordFighting.Configuration;
internal class KeywordFightingContextProvider(IOptionsMonitor<AppOptions> options) : IKeywordFightingContextProvider
{
    public async Task<KeywordFightingContext?> GetAsync(CancellationToken cancellationToken)
    {
        var text = await File.ReadAllTextAsync(options.CurrentValue.ContextPath, cancellationToken);
        var context = JsonSerializer.Deserialize<KeywordFightingContext>(text) ?? new KeywordFightingContext([], []);
        return context with
        {
            Equipments = context.Equipments
                .Select(equipment => equipment.Cast())
                .ToArray()
        };
    }
}