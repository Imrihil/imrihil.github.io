using KeywordFighting.Model;

namespace KeywordFighting;
public interface IKeywordFightingContextProvider
{
    Task<KeywordFightingContext?> GetAsync(CancellationToken cancellationToken);
}
