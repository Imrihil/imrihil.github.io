using KeywordFighting.Model;

namespace KeywordFighting;
public interface IGameContext
{
    bool IsInitialized { get; set; }
    bool IsFinished { get; set; }
    bool IsWon { get; set; }
    string? LastAction { get; set; }
    string? NextAction { get; set; }
    string[] AvailableActions { get; set; }

    List<List<Equipment>> EquipmentPropositions { get; set; }

    Deck<FightCard> FightDeck { get; }
    Character Character { get; }
    Enemy Enemy { get; }
    string GetDescription();
    IEnumerable<T> GetEquipment<T>() 
        where T : Equipment;
}