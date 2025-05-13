using System.Text.Json.Serialization;

namespace KeywordFighting.Model;
public record Character
{
    private static ulong _id;
    public string Name { get; set; }
    public int FullHandSize { get; set; } = 4;
    public bool HasInitiative { get; set; }
    public int CounterattackResistance { get; set; }
    public int Strengthened { get; set; }
    public double? Gold { get; set; }

    [JsonIgnore]
    public IEnumerable<Equipment> Equipment
    {
        get => new List<Equipment?>
        {
            Weapon != Weapon.Default ? Weapon : null,
            Shield,
            Armor != Armor.Default ? Armor : null,
            Helmet != Helmet.Default ? Helmet : null,
            Bracers != Bracers.Default ? Bracers : null,
            Greaves != Greaves.Default ? Greaves : null,
            Consumable != Consumable.Default ? Consumable : null,
            Trinket != Trinket.Default ? Trinket : null
        }.OfType<Equipment>();

        set
        {
            foreach (var equipment in value)
                switch (equipment)
                {
                    case Weapon weapon:
                        Weapon = weapon;
                        break;
                    case Shield shield:
                        Shield = shield;
                        break;
                    case Armor armor:
                        Armor = armor;
                        break;
                    case Helmet helmet:
                        Helmet = helmet;
                        break;
                    case Bracers bracers:
                        Bracers = bracers;
                        break;
                    case Greaves greaves:
                        Greaves = greaves;
                        break;
                    case Consumable consumable:
                        Consumable = consumable;
                        break;
                    case Trinket trinket:
                        Trinket = trinket;
                        break;
                }
        }
    }

    public int MissingCards => FullHandSize - Hand.Count;
    public bool IsStunned => Hand.Stun != null;
    public bool IsConscious => Hand.WoundsCount < FullHandSize;
    public double Health => Math.Max(0, FullHandSize - Hand.WoundsCount + (Hand.LightWound != null ? 0.5 : 0));
    [JsonIgnore]
    public int HealthPercent => (int)Math.Round(100 * Health / FullHandSize);

    public Weapon Weapon { get; set; } = Weapon.Default;
    public Shield? Shield { get; set; } = Shield.Default;
    public Helmet Helmet { get; set; } = Helmet.Default;
    public Armor Armor { get; set; } = Armor.Default;
    public Bracers Bracers { get; set; } = Bracers.Default;
    public Greaves Greaves { get; set; } = Greaves.Default;
    public Consumable? Consumable { get; set; } = Consumable.Default;
    public Trinket? Trinket { get; set; } = Trinket.Default;

    public Hand Hand { get; set; } = new();

    public string GetFullDescription() =>
        GetDescription() + ", " + Equipment.GetDescription();

    public string GetDescription() =>
        (HasInitiative ? "=> " : "   ")
        + $"{Name}, {Health}"
        + $"/{FullHandSize} PŻ"
        + (IsStunned ? ", ogłuszony" : "")
        + (CounterattackResistance > 0 ? ", odporny na kontratak" : "")
        + (Strengthened > 0 ? ", obrażenia x2" : "");

    public Character() => Name = $"Unknown {++_id}";
    public Character(string name) => Name = name;

    public SuccessfulAttackOutcome ApplySuccessfulAttack(SuccessfulAttackOutcome outcome)
    {
        Strengthened = Math.Max(0, Strengthened + outcome.Strengthening - 1);
        CounterattackResistance = Math.Max(0, CounterattackResistance + outcome.CounterattackResistance - 1);
        return outcome with { Healing = Hand.Heal(outcome) };
    }

    public FailedAttackOutcome ApplyFailedAttack(FailedAttackOutcome outcome, Deck<FightCard> deck)
    {
        if (CounterattackResistance > 0)
            outcome = outcome with
            {
                CounterattackDamage = new Damage(0)
            };

        ApplyDamage(outcome.CounterattackDamage, deck);

        if (outcome.IsInitiativeCaptured)
        {
            HasInitiative = false;
            Strengthened = 0;
            CounterattackResistance = 0;
        }
        else
        {
            Strengthened = Math.Max(0, Strengthened - 1);
            CounterattackResistance = Math.Max(0, CounterattackResistance - 1);
        }

        return outcome;
    }

    public FailedAttackOutcome ApplySuccessfulDefense(FailedAttackOutcome outcome)
    {
        if (!outcome.IsInitiativeCaptured)
            return outcome;

        if (Hand.TryRemoveStun())
            return outcome with
            {
                IsInitiativeCaptured = false,
                CounterattackDamage = new Damage(0)
            };

        HasInitiative = true;
        return outcome;
    }

    public SuccessfulAttackOutcome ApplyFailedDefense(SuccessfulAttackOutcome outcome, Deck<FightCard> deck)
    {
        outcome = outcome with
        {
            Damage = outcome.Damage + Math.Max(outcome.Stun - (IsStunned ? 0 : 1), 0),
            Stun = outcome.Stun > 0 && !IsStunned ? 1 : 0
        };
        if (outcome.Stun > 0)
            Hand.AddStun();
        ApplyDamage(outcome.Damage, deck);

        return outcome;
    }

    private void ApplyDamage(Damage damage, Deck<FightCard> deck)
    {
        for (var i = 0; i < damage.SeriousWounds; i++)
            Hand.AddSeriousWound();

        for (var i = 0; i < damage.LightWounds; i++)
            Hand.AddLightWound();

        Hand.DiscardRandom(Hand.Count - FullHandSize, deck);
    }

    public Equipment GetArmorByTarget(TargetEnum target) => target switch
    {
        TargetEnum.Head => Helmet,
        TargetEnum.Torso => Armor,
        TargetEnum.Hands => Bracers,
        TargetEnum.Legs => Greaves,
        _ => Armor
    };

    public void DrawCards(Deck<FightCard> deck) =>
        Hand.Draw(deck, MissingCards);

    public void Discard(FightCard card, Deck<FightCard> deck) =>
        Hand.Discard(card, deck);
}

public record Enemy : Character
{
    public FightCard? NextFightCard => Hand.Fights.FirstOrDefault();

    public Enemy() { }
    public Enemy(string name) : base(name) { }
}

public class Hand
{
    public int Count => _cards.Count;
    public int FightsCount => Fights.Count();
    public int WoundsCount => Wounds.Count();
    [JsonIgnore]
    public LightWoundCard? LightWound => _cards.OfType<LightWoundCard>().FirstOrDefault();
    [JsonIgnore]
    public StunCard? Stun => _cards.OfType<StunCard>().FirstOrDefault();

    public IEnumerable<FightCard> Fights => _cards.OfType<FightCard>();
    public IEnumerable<WoundCard> Wounds => _cards.OfType<WoundCard>().OrderByDescending(card => card.Value);

    public Damage Heal(SuccessfulAttackOutcome outcome)
    {
        var healedSeriousWounds = Wounds.Take(outcome.Healing.SeriousWounds).ToArray();
        foreach (var seriousWound in healedSeriousWounds)
            _cards.Remove(seriousWound);

        var lightWoundCard = LightWound;
        if (lightWoundCard == null || outcome.Healing.LightWounds == 0)
            return new Damage(healedSeriousWounds.Length);

        _cards.Remove(lightWoundCard);
        return new Damage(healedSeriousWounds.Length + 0.5);
    }

    private readonly List<Card> _cards = [];

    public void Draw(Deck<FightCard> deck, int count) =>
        _cards.AddRange(deck.Draw(count));

    public void Discard(FightCard card, Deck<FightCard> deck)
    {
        if (_cards.Remove(card))
            deck.Discard(card);
    }

    public void DiscardRandom(int count, Deck<FightCard> deck)
    {
        var remainingFightCard = FightsCount;
        for (var i = 0; i < count && remainingFightCard > 0; ++i)
        {
            var idx = Random.Shared.Next(0, remainingFightCard);
            var card = Fights.Skip(idx).First();
            Discard(card, deck);
            --remainingFightCard;
        }
    }

    public bool TryRemoveStun()
    {
        var stun = Stun;
        if (stun == null)
            return false;

        _cards.Remove(stun);
        return true;
    }

    public void AddStun() =>
        _cards.Add(new StunCard());

    public void AddSeriousWound() =>
        _cards.Add(new SeriousWoundCard());

    public void AddLightWound()
    {
        var lightWound = LightWound;
        if (lightWound == null)
            _cards.Add(new LightWoundCard());
        else
        {
            _cards.Remove(lightWound);
            AddSeriousWound();
        }
    }
}