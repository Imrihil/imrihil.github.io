using KeywordFighting.Model;
using Microsoft.Extensions.Logging;

namespace KeywordFighting;
internal class GameEngine(ILogger<GameEngine> logger) : IGameEngine
{
    private readonly GameInitializationEngine _gameInitializationEngine = new(logger);

    public async Task DoActionAsync(IGameContext context, int index, CancellationToken cancellationToken)
    {
        if (!context.IsInitialized)
        {
            await _gameInitializationEngine.DoActionAsync(context, index, cancellationToken);
            if (!context.IsInitialized)
                return;

            context.LastAction = null;
            context.NextAction = null;
            context.AvailableActions = [];
            index = 0;
        }

        if (context.IsFinished)
            return;

        if (cancellationToken.IsCancellationRequested)
        {
            context.LastAction = "Walka anulowana";
            context.IsFinished = true;
            return;
        }

        FixInitiative(context);

        if (context.LastAction == null)
        {
            context.LastAction = GetIntroduction(context);
        }
        else
        {
            logger.LogDebug("Action index: {index}", index);
            var lastAction = ResolveAction(context, index);
            if (lastAction == null)
            {
                logger.LogWarning("[Błąd] Akcja nieprawidłowa, spróbuj jeszcze raz!");
                return;
            }

            context.LastAction = lastAction;
        }

        DrawCards(context);
        UpdateNextAction(context);
        logger.LogInformation(
            "Last = {last}, Next = {next}, Available = [{available}]",
            context.LastAction, context.NextAction, string.Join(", ", context.AvailableActions.Select((availableAction, i) => $"{i}: {availableAction}")));
    }

    private void FixInitiative(IGameContext context)
    {
        if (context.Character.HasInitiative != context.Enemy.HasInitiative)
            return;

        switch (Random.Shared.Next(2))
        {
            case 0:
                logger.LogDebug("Character has initiative.");
                context.Character.HasInitiative = true;
                context.Enemy.HasInitiative = false;
                break;
            case 1:
                logger.LogDebug("Enemy has initiative.");
                context.Character.HasInitiative = false;
                context.Enemy.HasInitiative = true;
                break;
        }
    }

    private string GetIntroduction(IGameContext context) =>
        "Razem z przeciwnikiem powoli zbliżacie się do siebie, krążąc wokół placu, na którym się znaleźliście. Naprzemiennie wznosicie i opuszczacie Wasze bronie. Mierzycie się czujnym spojrzeniem."
        + (context.Character.HasInitiative
            ? " W końcu zbliżacie się na dostatecznie małą odległość, by przypuścić atak. Wykorzystujesz moment, który daje najwięcej możliwości i nacierasz na wroga."
            : " Nie musisz długo czekać na rozwój sytuacji. Przeciwnik natychmiast do Ciebie doskakuje.");

    private string? ResolveAction(IGameContext context, int actionIndex)
    {
        var characterCard = context.Character.Hand.Fights.Skip(actionIndex).FirstOrDefault();
        if (characterCard == null)
        {
            logger.LogInformation("Chosen action {index}, but it has only {cards} fight cards in hand!", actionIndex, context.Character.Hand.FightsCount);
            return null;
        }

        var enemyCard = context.Enemy.NextFightCard;
        if (enemyCard == null)
        {
            logger.LogError("Enemy has no fight cards in hand, but is not defeated yet!");
            return null;
        }

        context.Character.Discard(characterCard, context.FightDeck);
        context.Enemy.Discard(enemyCard, context.FightDeck);

        var outcome = context.Character.HasInitiative
            ? characterCard.Attack.GetOutcome(context.Character, context.Enemy, enemyCard.Defense)
            : enemyCard.Attack.GetOutcome(context.Enemy, context.Character, characterCard.Defense);

        logger.LogDebug("{outcome}", outcome);

        return outcome.ApplyResults(context.FightDeck);
    }

    private void DrawCards(IGameContext context)
    {
        DrawCards(context.Character, context.FightDeck);
        DrawCards(context.Enemy, context.FightDeck);
    }

    private void DrawCards(Character character, Deck<FightCard> deck)
    {
        logger.LogDebug("{character} draw {count} missing cards (wounds: {wounds}, stun: {stun}).", character.Name, character.MissingCards, character.Hand.WoundsCount - (character.Hand.LightWound != null ? 0.5 : 0), character.Hand.Stun != null ? 1 : 0);
        character.DrawCards(deck);
    }

    private static void UpdateNextAction(IGameContext context)
    {
        if (!context.Character.IsConscious)
        {
            context.IsFinished = true;
            context.NextAction = "Walka skończona. Postać jest nieprzytomna!";
            context.AvailableActions = [];
            return;
        }

        if (!context.Enemy.IsConscious)
        {
            context.IsFinished = true;
            context.IsWon = true;
            context.NextAction = "Walka skończona. Przeciwnik został pokonany!";
            context.AvailableActions = [];
            return;
        }

        if (context.Character.HasInitiative)
        {
            context.NextAction = "Wybierz atak:";
            context.AvailableActions = context.Character.Hand.Fights.Select(card => card.Attack.GetDescription(context.Character, context.Enemy)).ToArray();
        }
        else
        {
            context.NextAction = $"Przeciwnik atakuje: {context.Enemy.NextFightCard?.Attack.Description}"
                                 + Environment.NewLine + Environment.NewLine + "Wybierz obronę:";
            context.AvailableActions = context.Character.Hand.Fights.Select(card => card.Defense.GetDescription()).ToArray();
        }
    }
}