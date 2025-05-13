namespace KeywordFighting;
public interface IGameContextProvider
{
    Task<IGameContext> CreateAsync(CancellationToken cancellationToken);
}