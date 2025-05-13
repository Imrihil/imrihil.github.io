using KeywordFighting.Model;
using Microsoft.Extensions.Logging;

namespace KeywordFighting;
internal class GameInitializationEngine(ILogger logger) : IGameEngine
{
    public async Task DoActionAsync(IGameContext context, int index, CancellationToken cancellationToken)
    {
        if (context.IsInitialized)
            return;

        if (cancellationToken.IsCancellationRequested)
        {
            context.LastAction = "Przygotowanie anulowane.";
            context.IsFinished = true;
            return;
        }

        if (context.LastAction == null)
        {
            context.LastAction = "Witaj wojowniku!";
            context.NextAction = "Zanim rozpoczniesz swoją niezapomnianą przygodę, wybierz swój ekwipunek! Jaka jest Twoja zamożność?";
            context.AvailableActions = [
                "Walka bez ekwipunku",
                "Chłop: 6 sztuk złota",
                "Mieszczanin: 12 sztuk złota",
                "Doświadczony wojownik: 25 sztuk złota",
                "Bogaty rycerz: 50 sztuk złota",
                "Magnata: 100 sztuk złota"
            ];
        }
        else
        {
            if (context.Character.Gold == null)
            {
                context.Character.Gold = index == 0 ? 0 : 25d * Math.Pow(2, index - 3);
                if (context.Character.Gold > 0)
                {
                    context.LastAction = $"Posiadasz {context.Character.Gold:0.##} sztuk złota.";
                    context.NextAction = "Jak trudna ma być dla Ciebie walka?";
                    context.AvailableActions = [
                        "Trening ze słabszym przeciwnikiem o klasę",
                        "Łatwa",
                        "Wyrównana",
                        "Wymagająca",
                        "Wyzwanie z przeciwnikiem silniejszym o klasę"];
                }
                else
                {
                    context.LastAction = "Nie posiadasz złota. Walka odbędzie się bez broni.";
                    context.NextAction = null;
                    context.AvailableActions = [];

                    context.IsInitialized = true;
                }
            }
            else
            {
                if (context.Enemy.Gold == null || context.EquipmentPropositions.Count == 0)
                {
                    context.Enemy.Gold = context.Character.Gold.Value * Math.Pow(2, (double)index / 2 - 1);
                    context.Enemy.Equipment = GenerateEquipment(context, context.Enemy.Gold.Value);
                    for (var i = 0; i < 4; i++)
                        context.EquipmentPropositions.Add(GenerateEquipment(context, context.Character.Gold.Value).OrderBy(equipment => equipment.Type).ToList());

                    var enemyEquipment = context.Enemy.Equipment.ToList();
                    context.LastAction = $"Przeciwnik posiada {enemyEquipment.GetDescription()} warte {enemyEquipment.Cost():0.##} sztuk złota.";
                    context.NextAction = "W który zestaw ekwipunku chcesz się wyposażyć?";
                    context.AvailableActions = context.EquipmentPropositions.Select(list => $"{list.GetDescription()} warte {list.Cost():0.##} sztuk złota.").ToArray();
                }
                else
                {

                    context.Character.Equipment = context.EquipmentPropositions[index];
                    var characterEquipment = context.Character.Equipment.ToList();
                    var enemyEquipment = context.Enemy.Equipment.ToList();
                    context.LastAction = $"Gracz posiada {characterEquipment.GetDescription()} warte {characterEquipment.Cost():0.##} sztuk złota."
                                         + Environment.NewLine + $"Przeciwnik posiada {enemyEquipment.GetDescription()} warte {enemyEquipment.Cost():0.##} sztuk złota.";
                    context.NextAction = null;
                    context.AvailableActions = [];

                    context.IsInitialized = true;
                }
            }
        }

        logger.LogInformation(
            "Last = {last}, Next = {next}, Available = [{available}]",
            context.LastAction, context.NextAction, string.Join(", ", context.AvailableActions.Select((availableAction, i) => $"{i}: {availableAction}")));
    }

    private IEnumerable<Equipment> GenerateEquipment(IGameContext context, double gold)
    {
        Equipment? equipment = GetRandom<Weapon>(context, gold);
        if (equipment != null)
        {
            gold -= equipment.Cost;
            yield return equipment;
        }

        var array = Enumerable.Range(0, 4 + 2 - (equipment?.Hands ?? 1)).ToArray();
        Random.Shared.Shuffle(array);

        foreach (var number in array[..^1])
        {
            equipment = GetRandom(context, gold, number);
            if (equipment != null)
            {
                gold -= equipment.Cost;
                yield return equipment;
            }
        }

        equipment = GetBest(context, gold, array[^1]);
        if (equipment != null)
            yield return equipment;
    }

    private Equipment? GetRandom(IGameContext context, double gold, int number) => number switch
    {
        0 => GetRandom<Armor>(context, gold),
        1 => GetRandom<Helmet>(context, gold),
        2 => GetRandom<Bracers>(context, gold),
        3 => GetRandom<Greaves>(context, gold),
        4 => GetRandom<Shield>(context, gold),
        _ => null
    };

    private Equipment? GetBest(IGameContext context, double gold, int number) => number switch
    {
        0 => GetBest<Armor>(context, gold),
        1 => GetBest<Helmet>(context, gold),
        2 => GetBest<Bracers>(context, gold),
        3 => GetBest<Greaves>(context, gold),
        4 => GetBest<Shield>(context, gold),
        _ => null
    };

    private T? GetRandom<T>(IGameContext context, double maxCost)
        where T : Equipment
    {
        var affordable = context.GetEquipment<T>().Where(equipment => equipment.Cost > 0 && equipment.Cost <= maxCost).ToArray();
        return affordable.Skip(Random.Shared.Next(0, affordable.Length)).FirstOrDefault();
    }

    private T? GetBest<T>(IGameContext context, double maxCost)
        where T : Equipment =>
        context.GetEquipment<T>().Where(equipment => equipment.Cost > 0 && equipment.Cost <= maxCost)
            .MaxBy(equipment => equipment.Cost);
}