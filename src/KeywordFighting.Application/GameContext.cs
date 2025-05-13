using KeywordFighting.Model;

namespace KeywordFighting;
public class GameContext(KeywordFightingContext context) : IGameContext
{
    public bool IsInitialized { get; set; }
    public bool IsFinished { get; set; }
    public bool IsWon { get; set; }
    public string? LastAction { get; set; }
    public string? NextAction { get; set; }
    public string[] AvailableActions { get; set; } = ["Initialize"];

    public List<List<Equipment>> EquipmentPropositions { get; set; } = [];

    public Deck<FightCard> FightDeck { get; } = new(context.FightCards);
    public Character Character { get; } = new("Gracz");
    public Enemy Enemy { get; } = new("Wróg");

    public string GetDescription() => 
        Character.GetFullDescription() 
        + Environment.NewLine + Enemy.GetFullDescription();

    public IEnumerable<T> GetEquipment<T>()
        where T : Equipment =>
        context.GetEquipment<T>();
}