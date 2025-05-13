namespace KeywordFighting;
public interface IGameEngine
{
    Task DoActionAsync(IGameContext context, int index, CancellationToken cancellationToken);
}