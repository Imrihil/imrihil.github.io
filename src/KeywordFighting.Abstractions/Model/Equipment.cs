using System.Text.Json.Serialization;

namespace KeywordFighting.Model;
public record Equipment
{
    [JsonPropertyName("Type")]
    public string TypeRaw { get; set; }
    [JsonIgnore]
    public EquipmentTypeEnum Type => Enum.TryParse(TypeRaw, true, out EquipmentTypeEnum @enum) ? @enum : EquipmentTypeEnum.None;
    public string Name { get; set; }
    public int? Hands { get; set; }
    public Power Power { get; set; } = new();
    public double Cost { get; set; }

    public Equipment() { }
    public Equipment(string typeRaw, string name, int? hands, Power power, double cost)
    {
        TypeRaw = typeRaw;
        Name = name;
        Hands = hands;
        Power = power;
        Cost = cost;
    }

    public string GetDescription()
    {
        var advantages = Power.Advantages.Select(type => type.GetDescription()).ToArray();
        return $"{Name} ({Power.Min}"
               + (advantages.Length > 0 ? $", {string.Join(", ", advantages)}" : "")
               + ")";
    }

    public Equipment Cast() => TypeRaw.ToLower() switch
    {
        Weapon.TypeName => new Weapon(Name, Hands, Power, Cost),
        Shield.TypeName => new Shield(Name, Power, Cost),
        Helmet.TypeName => new Helmet(Name, Power, Cost),
        Armor.TypeName => new Armor(Name, Power, Cost),
        Bracers.TypeName => new Bracers(Name, Power, Cost),
        Greaves.TypeName => new Greaves(Name, Power, Cost),
        Consumable.TypeName => new Consumable(Name, Power, Cost),
        Trinket.TypeName => new Trinket(Name, Power, Cost),
        _ => this
    };

    public static Equipment? Default(Equipment equipment) => equipment switch
    {
        Weapon => Weapon.Default,
        Shield => Shield.Default,
        Helmet => Helmet.Default,
        Armor => Armor.Default,
        Bracers => Bracers.Default,
        Greaves => Greaves.Default,
        Consumable => Consumable.Default,
        Trinket => Trinket.Default,
        _ => null
    };
}

public record Weapon : Equipment
{
    public const string TypeName = "weapon";
    public static readonly Weapon Default = new("Ręce", 1, new Power(0.5, 0, 0, 0), 0);

    [JsonIgnore]
    public bool IsTwoHanded => Hands == 2;

    public Weapon() { }
    public Weapon(string name, int? hands, Power power, double cost)
        : base(TypeName, name, hands ?? 1, power, cost)
    { }

    public Weapon(string name, int? hands, double bludgeoning, double piercing, double slashing, int counterattack, double cost = 0)
        : base(TypeName, name, hands ?? 1, new Power(bludgeoning, piercing, slashing, counterattack), cost)
    { }

    public double GetMultiplier(Power advantage, Equipment armor) =>
        Math.Pow(2, (int)Math.Floor((Power + advantage - armor.Power).Max + 0.000001));

    public double GetCounterattackMultiplier(Armor armor) =>
        Math.Pow(2, (int)Math.Floor((Power.Counterattack ?? Power.Min) - armor.Power.Min + 0.000001));
}

public record Shield : Equipment
{
    public const string TypeName = "shield";
    public static readonly Shield? Default = null;

    public Shield() { }
    public Shield(string name, Power power, double cost)
        : base(TypeName, name, 1, power, cost)
    { }
    public Shield(string name, double cost = 0)
        : base(TypeName, name, 1, Power.Zero, cost)
    { }
}

public record Armor : Equipment
{
    public const string TypeName = "armor";
    public static readonly Armor Default = new("Nic", Power.Zero, 0);

    public Armor() { }
    public Armor(string name, Power power, double cost)
        : base(TypeName, name, null, power, cost)
    { }
    public Armor(string name, int bludgeoning, int piercing, int slashing, double cost = 0)
        : base(TypeName, name, null, new Power(bludgeoning, piercing, slashing), cost)
    { }
}

public record Helmet : Equipment
{
    public const string TypeName = "helmet";
    public static readonly Helmet Default = new("Nic", Power.Zero, 0);

    public Helmet() { }
    public Helmet(string name, Power power, double cost)
        : base(TypeName, name, null, power, cost)
    { }
    public Helmet(string name, int bludgeoning, int piercing, int slashing, double cost = 0)
        : base(TypeName, name, null, new Power(bludgeoning, piercing, slashing), cost)
    { }
}

public record Bracers : Equipment
{
    public const string TypeName = "bracers";
    public static readonly Bracers Default = new("Nic", Power.Zero, 0);

    public Bracers() { }
    public Bracers(string name, Power power, double cost)
        : base(TypeName, name, null, power, cost)
    { }
    public Bracers(string name, int bludgeoning, int piercing, int slashing, double cost = 0)
        : base(TypeName, name, null, new Power(bludgeoning, piercing, slashing), cost)
    { }
}

public record Greaves : Equipment
{
    public const string TypeName = "greaves";
    public static readonly Greaves Default = new("Nic", Power.Zero, 0);

    public Greaves() { }
    public Greaves(string name, Power power, double cost)
        : base(TypeName, name, null, power, cost)
    { }
    public Greaves(string name, int bludgeoning, int piercing, int slashing, double cost = 0)
        : base(TypeName, name, null, new Power(bludgeoning, piercing, slashing), cost)
    { }
}

public record Consumable : Equipment
{
    public const string TypeName = "consumable";
    public static readonly Consumable? Default = null;

    public Consumable() { }
    public Consumable(string name, Power power, double cost)
        : base(TypeName, name, null, power, cost)
    { }
    public Consumable(string name, int bludgeoning, int piercing, int slashing, double cost = 0)
        : base(TypeName, name, null, new Power(bludgeoning, piercing, slashing), cost)
    { }
}

public record Trinket : Equipment
{
    public const string TypeName = "trinket";
    public static readonly Trinket? Default = null;

    public Trinket() { }
    public Trinket(string name, Power power, double cost)
        : base(TypeName, name, null, power, cost)
    { }
    public Trinket(string name, int bludgeoning, int piercing, int slashing, double cost = 0)
        : base(TypeName, name, null, new Power(bludgeoning, piercing, slashing), cost)
    { }
}

public record Power
{
    public static readonly Power Zero = new(0, 0, 0);

    public double? Bludgeoning { get; set; }
    public double? Piercing { get; set; }
    public double? Slashing { get; set; }
    public double? Counterattack { get; set; }

    private double[] AllPowers => [Bludgeoning ?? 0, Piercing ?? 0, Slashing ?? 0];
    [JsonIgnore]
    public double Max => AllPowers.Max();
    [JsonIgnore]
    public double Min => AllPowers.Min();

    [JsonIgnore]
    public IEnumerable<DamageType> Advantages => !(Math.Abs(Min - Max) < 0.0000001)
        ? new List<DamageType?>([
            (Bludgeoning ?? 0) > Min + 0.0000001 ? DamageType.Bludgeoning : null,
            (Piercing ?? 0) > Min + 0.000000 ? DamageType.Piercing : null,
            (Slashing ?? 0) > Min + 0.000000 ? DamageType.Slashing : null,
            (Counterattack ?? 0) > Min + 0.000000 ? DamageType.Counterattack: null]).OfType<DamageType>()
        : Counterattack > Min + 0.000001
            ? [DamageType.Counterattack]
            : [];

    public Power() { }

    public Power(double? bludgeoning, double? piercing, double? slashing, double? counterattack = null)
    {
        Bludgeoning = bludgeoning;
        Piercing = piercing;
        Slashing = slashing;
        Counterattack = counterattack;
    }

    public static Power operator +(Power power1, Power power2) => new(
        power1.Bludgeoning + power2.Bludgeoning,
        power1.Piercing + power2.Piercing,
        power1.Slashing + power2.Slashing,
        power1.Counterattack + power2.Counterattack);

    public static Power operator -(Power power1, Power power2) => new(
        power1.Bludgeoning - power2.Bludgeoning,
        power1.Piercing - power2.Piercing,
        power1.Slashing - power2.Slashing,
        power1.Counterattack - power2.Counterattack);
}

public enum DamageType
{
    Any,
    Bludgeoning,
    Piercing,
    Slashing,
    Counterattack
}

public enum EquipmentTypeEnum
{
    None,
    Weapon,
    Shield,
    Armor,
    Helmet,
    Bracers,
    Greaves
}

public static class EquipmentExtensions
{
    public static string GetDescription(this IEnumerable<Equipment> equipments) =>
        string.Join(", ", equipments.Select(equipment => equipment.GetDescription()));

    public static double Cost(this IEnumerable<Equipment> equipments) =>
        equipments.Sum(equipment => equipment.Cost);
}

public static class DamageTypeExtensions
{
    public static string GetDescription(this IEnumerable<DamageType> types) =>
        string.Join("/", types.Select(type => type.GetDescription()));

    public static string GetDescription(this DamageType type) => type switch
    {
        DamageType.Bludgeoning => "miażdżone",
        DamageType.Piercing => "kłute",
        DamageType.Slashing => "cięte",
        DamageType.Counterattack => "kontratak",
        _ => "*"
    };
}

public static class CurrencyExtensions
{
    public static (int Gold, int Silver, int Copper) GetCurrency(this double cost)
    {
        var gold = (int)cost;
        var silver = (int)(10 * cost) - 10 * gold;
        var copper = (int)(100 * cost) - 100 * gold - 10 * silver;
        return (gold, silver, copper);
    }
}