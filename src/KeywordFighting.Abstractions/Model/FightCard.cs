using System.Text.Json;
using System.Text.Json.Serialization;

namespace KeywordFighting.Model;
public record FightCard : Card
{
    public AttackStats Attack { get; set; }
    public DefenseStats Defense { get; set; }
}

public record AttackStats
{
    public string Name { get; set; }
    public string Description { get; set; }
    public Effectiveness Effectiveness { get; set; } = new();
    public string HitDescription { get; set; }
    [JsonPropertyName("Target")]
    public string TargetRaw { get; set; }
    [JsonIgnore]
    public TargetEnum Target => TargetRaw switch
    {
        "głowa" => TargetEnum.Head,
        "tułów" => TargetEnum.Torso,
        "ręce" => TargetEnum.Hands,
        "nogi" => TargetEnum.Legs,
        _ => TargetEnum.Torso
    };
    public Power Advantage { get; set; } = new();
    public double? Damage { get; set; }
    public int? Stun { get; set; }
    public int? CounterattackResistance { get; set; }
    public int? Strengthen { get; set; }
    public double? Healing { get; set; }

    public AttackStats() { }
    public AttackStats(string name, TargetEnum target, Power advantage, Effectiveness effectiveness, double damage,
        bool stun = false, bool counterattackResistance = false, bool strengthen = false, double healing = 0)
    {
        Name = name;
        TargetRaw = target.ToString();
        Advantage = advantage;
        Effectiveness = effectiveness;
        Damage = damage;
        Stun = stun ? 1 : 0;
        CounterattackResistance = counterattackResistance ? 1 : 0;
        Strengthen = strengthen ? 1 : 0;
        Healing = healing;
    }

    public AttackOutcome GetOutcome(Character attacker, Character defender, DefenseStats defense)
    {
        var attackMultiplier = attacker.Weapon.GetMultiplier(Advantage, defender.GetArmorByTarget(Target));
        var counterattackMultiplier = defender.Weapon.GetCounterattackMultiplier(attacker.Armor);

        var outcome = Effectiveness * defense.Effectiveness;
        return outcome switch
        {
            > 0 => new FailedAttackOutcome(attacker, defender, this, defense,
                true, defense.Effectiveness.Counterattack ?? 0) * (outcome * counterattackMultiplier),
            < 0 => new SuccessfulAttackOutcome(attacker, defender, this, defense,
                attackMultiplier * Damage ?? 0,
                (int)Math.Ceiling(attackMultiplier * Stun ?? 0 - 0.000001),
                CounterattackResistance ?? 0,
                Strengthen ?? 0,
                Healing ?? 0) * (-outcome * (attacker.Strengthened > 0 ? 2 : 1)),
            _ => new FailedAttackOutcome(attacker, defender, this, defense,
                defender.Shield != null, 0)
        };
    }

    public string GetDescription(Character attacker, Enemy defender) =>
        $"{GetDescription()} [x{attacker.Weapon.GetMultiplier(Advantage, defender.GetArmorByTarget(Target))}]";

    public string GetDescription()
    {
        var advantages = Advantage.Advantages.ToArray();
        return $"{Name}: {Description} ({TargetRaw}"
               + (advantages.Length > 0 ? $", {advantages.GetDescription()}" : "")
               + $" [{Effectiveness.GetDescription()}]: {GetResultsDescription()})";
    }

    private string GetResultsDescription()
    {
        var results = new List<string>();
        if (Damage > 0) results.Add($"{Damage} obrażeń");
        if (Stun > 0) results.Add("ogłuszenie");
        if (CounterattackResistance > 0) results.Add("brak kontrataku w następnej turze");
        if (Strengthen > 0) results.Add("obrażenia x2 w następnej turze");
        if (Healing > 0) results.Add($"+{Healing} PŻ");
        return string.Join(", ", results);
    }
}

public record DefenseStats
{
    public string Name { get; set; }
    public string Description { get; set; }
    public Effectiveness Effectiveness { get; set; } = new();

    public DefenseStats() { }
    public DefenseStats(string name, Effectiveness effectiveness)
    {
        Name = name;
        Effectiveness = effectiveness;
    }

    public string GetDescription() =>
        $"{Name}: {Description} [{Effectiveness.GetDescription(true)}]";
}

public record Effectiveness
{
    public int? Block { get; set; }
    public int? Repulse { get; set; }
    public int? Dodge { get; set; }
    public int? Capture { get; set; }
    public int? Counterattack { get; set; }

    public Effectiveness() { }
    public Effectiveness(int? block, int? repulse, int? dodge, int? capture, int? counterattack)
    {
        Block = block;
        Repulse = repulse;
        Dodge = dodge;
        Capture = capture;
        Counterattack = counterattack;
    }

    public static int operator *(Effectiveness attack, Effectiveness defense) =>
        (attack.Block ?? 0) * (defense.Block ?? 0)
        + (attack.Repulse ?? 0) * (defense.Repulse ?? 0)
        + (attack.Dodge ?? 0) * (defense.Dodge ?? 0)
        + (attack.Capture ?? 0) * (defense.Capture ?? 0)
        + (attack.Counterattack ?? 0) * (defense.Counterattack ?? 0);

    public string GetDescription(bool isDefense = false) =>
        $"{GetChance(Block)}{GetChance(Repulse)}{GetChance(Dodge)}{GetChance(Capture)}{GetChance((isDefense ? -1 : 1) * Counterattack ?? 0)}";

    private static string GetChance(int? chance) => chance switch
    {
        < 0 => "!",
        > 0 => "D",
        _ => "_"
    };
}

public abstract record AttackOutcome(Character Attacker, Character Defender, AttackStats Attack, DefenseStats Defense)
{
    public abstract string ApplyResults(Deck<FightCard> deck);
}

public record SuccessfulAttackOutcome(
    Character Attacker,
    Character Defender,
    AttackStats Attack,
    DefenseStats Defense,
    Damage Damage,
    int Stun,
    int CounterattackResistance,
    int Strengthening,
    Damage Healing) : AttackOutcome(Attacker, Defender, Attack, Defense)
{
    public SuccessfulAttackOutcome(Character attacker, Character defender, AttackStats attack, DefenseStats defense,
        double damage, int stun, int counterattackResistance, int strengthening, double healing)
        : this(attacker, defender, attack, defense,
            new Damage(damage), stun, counterattackResistance, strengthening, new Damage(healing))
    { }

    public override string ApplyResults(Deck<FightCard> deck)
    {
        var outcome = Defender.ApplyFailedDefense(this, deck);
        outcome = Attacker.ApplySuccessfulAttack(outcome);
        return outcome.GetDescription();
    }

    private string GetDescription()
    {
        List<string> descriptions = [];
        if (Damage > 0)
            descriptions.Add($"Atak dosięgnął celu zadając {Damage.SeriousWounds} poważnych obrażeń{(Damage.LightWounds > 0 ? $" i {Damage.LightWounds} lekkie" : "")}{(Stun > 0 ? " oraz ogłuszając przeciwnika" : "")}.");
        else if (Stun > 0)
            descriptions.Add("Atak ogłuszył przeciwnika.");

        if (Strengthening > 0 && CounterattackResistance > 0)
            descriptions.Add($"Następne {Strengthening} ataków zada podwójne obrażenia i będzie odpornych na obrażenia z kontrataku.");
        else if (Strengthening > 0)
            descriptions.Add($"Następne {Strengthening} ataków zada podwójne obrażenia.");
        else if (CounterattackResistance > 0)
            descriptions.Add($"Następne {CounterattackResistance} ataków będzie odpornych na obrażenia z kontrataków.");

        if (Healing.Count > 0)
            descriptions.Add($"Chwila oddechu pozwoliła na zignorowanie {Healing.SeriousWounds} poważnych ran{(Healing.LightWounds > 0 ? $" i {Healing.LightWounds} lekkiej" : "")} do końca walki.");

        return string.Join(" ", descriptions);
    }

    public override string ToString() =>
        $"SuccessfulAttackOutcome {{ "
        + $"Attacker = {Attacker.Name}, Strengthened = {Attacker.Strengthened > 0}, Weapon = {JsonSerializer.Serialize(Attacker.Weapon, SerializationConstants.Options)}, "
        + $"Defender = {Defender.Name}, Armor = {JsonSerializer.Serialize(Defender.GetArmorByTarget(Attack.Target), SerializationConstants.Options)}, "
        + $"Attack = {JsonSerializer.Serialize(Attack, SerializationConstants.Options)}, "
        + $"Defense = {JsonSerializer.Serialize(Defense, SerializationConstants.Options)}, "
        + $"Damage = {Damage.Count}, "
        + $"Stun = {Stun}, "
        + $"CounterattackResistance = {CounterattackResistance}, "
        + $"Strengthening = {Strengthening}, "
        + $"Healing = {Healing.Count} }}";

    public static SuccessfulAttackOutcome operator *(SuccessfulAttackOutcome outcome, double multiplier) => outcome with
    {
        Damage = outcome.Damage * multiplier,
        Stun = (int)Math.Ceiling(outcome.Stun * multiplier - 0.000001),
        CounterattackResistance = (int)Math.Ceiling(outcome.CounterattackResistance * multiplier - 0.000001),
        Strengthening = (int)Math.Ceiling(outcome.Strengthening * multiplier - 0.000001),
        Healing = outcome.Healing * multiplier
    };
}

public enum TargetEnum
{
    Torso, Hands, Legs, Head, Counterattack
}

public static class TargetExtensions
{
    public static EquipmentTypeEnum GetEquipmentType(this TargetEnum target) => target switch
    {
        TargetEnum.Torso => EquipmentTypeEnum.Armor,
        TargetEnum.Hands => EquipmentTypeEnum.Bracers,
        TargetEnum.Legs => EquipmentTypeEnum.Greaves,
        TargetEnum.Head => EquipmentTypeEnum.Helmet,
        TargetEnum.Counterattack => EquipmentTypeEnum.Armor,
        _ => EquipmentTypeEnum.None
    };
}

public record FailedAttackOutcome(
    Character Attacker,
    Character Defender,
    AttackStats Attack,
    DefenseStats Defense,
    bool IsInitiativeCaptured,
    Damage CounterattackDamage) : AttackOutcome(Attacker, Defender, Attack, Defense)
{
    public FailedAttackOutcome(Character attacker, Character defender, AttackStats attack, DefenseStats defense, bool isInitiativeCaptured, double counterattackDamage)
        : this(attacker, defender, attack, defense, isInitiativeCaptured, new Damage(counterattackDamage)) { }

    public override string ApplyResults(Deck<FightCard> deck)
    {
        if (!IsInitiativeCaptured)
            return "Nie udało się przełamać obrony przeciwnika, jednak inicjatywa nadal jest po stronie atakującego.";

        var outcome = Defender.ApplySuccessfulDefense(this);
        outcome = Attacker.ApplyFailedAttack(outcome, deck);

        if (!outcome.IsInitiativeCaptured)
            return "Zaatakowany był w stanie uniknąć ciosów i otrząsnąć się z ogłuszenia.";

        return "Zaatakowany był w stanie uniknąć wszelkich obrażeń i przejąć inicjatywę."
               + (outcome.CounterattackDamage > 0
                   ? $" Ponadto wyprowadził skuteczny kontratak zadając {outcome.CounterattackDamage.SeriousWounds} poważnych obrażeń{(outcome.CounterattackDamage.LightWounds > 0 ? $" i {outcome.CounterattackDamage.LightWounds} lekkie." : ".")}"
                   : "");
    }

    public override string ToString() =>
        $"SuccessfulAttackOutcome {{ "
        + $"Attacker = {Attacker.Name}, Strengthened = {Attacker.Strengthened > 0}, Armor = {JsonSerializer.Serialize(Attacker.Armor, SerializationConstants.Options)}, "
        + $"Defender = {Defender.Name}, Weapon = {JsonSerializer.Serialize(Defender.Weapon, SerializationConstants.Options)}, "
        + $"Attack = {JsonSerializer.Serialize(Attack, SerializationConstants.Options)}, "
        + $"Defense = {JsonSerializer.Serialize(Defense, SerializationConstants.Options)}, "
        + $"IsInitiativeCaptured = {IsInitiativeCaptured}, "
        + $"CounterattackDamage = {CounterattackDamage.Count} }}";

    public static FailedAttackOutcome operator *(FailedAttackOutcome outcome, double multiplier) => outcome with
    {
        CounterattackDamage = outcome.CounterattackDamage * multiplier
    };
}

public record Damage(double Count)
{
    public int SeriousWounds => (int)Count;
    public int LightWounds => Count - SeriousWounds > 0 ? 1 : 0;

    public static bool operator >(Damage damage, double count) => damage.Count > count;
    public static bool operator <(Damage damage, double count) => damage.Count < count;
    public static bool operator ==(Damage damage, double count) => Math.Abs(damage.Count - count) < 0.000001;
    public static bool operator !=(Damage damage, double count) => Math.Abs(damage.Count - count) > 0.000001;

    public static Damage operator *(Damage damage, double multiplier) => new(damage.Count * multiplier);
    public static Damage operator /(Damage damage, double divisor) => new(damage.Count / divisor);
    public static Damage operator +(Damage damage, double addend) => new(damage.Count + addend);
    public static Damage operator -(Damage damage, double subtrahend) => new(damage.Count - subtrahend);
}