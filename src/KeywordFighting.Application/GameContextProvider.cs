using KeywordFighting.Model;

namespace KeywordFighting;
internal class GameContextProvider(IKeywordFightingContextProvider provider)
    : IGameContextProvider
{
    public async Task<IGameContext> CreateAsync(CancellationToken cancellationToken) =>
        new GameContext(await provider.GetAsync(cancellationToken)
                        ?? new KeywordFightingContext([], []));
}