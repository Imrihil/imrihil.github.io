namespace KeywordFighting.Model;
public class Deck<T>
{
    public int Size => _cards.Count;
    public int Discarded => _discarded.Count;

    private readonly Queue<T> _cards;

    private readonly List<T> _discarded = [];

    public Deck(IEnumerable<T> enumerable)
    {
        var array = enumerable.ToArray();
        Random.Shared.Shuffle(array);
        _cards = new Queue<T>(array);
    }

    public IEnumerable<T> Draw(int count)
    {
        while (--count >= 0)
        {
            var item = DrawNext();
            if (item != null)
                yield return item;
        }
    }

    public void Discard(T card) =>
        _discarded.Add(card);

    private T? DrawNext()
    {
        if (!_cards.Any())
        {
            var array = _discarded.ToArray();
            Random.Shared.Shuffle(array);
            EnqueueMany(array);
            _discarded.Clear();
        }

        _cards.TryDequeue(out var item);
        return item;
    }

    private void EnqueueMany(T[] array)
    {
        foreach (var item in array)
            _cards.Enqueue(item);
    }
}