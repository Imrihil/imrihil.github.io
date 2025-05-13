namespace KeywordFighting.Model;
public record KeywordFightingContext(
    IReadOnlyCollection<FightCard> FightCards,
    IReadOnlyCollection<Equipment> Equipments)
{
    public IEnumerable<T> GetEquipment<T>() where T : Equipment => Equipments.OfType<T>();
}