using CsvHelper;
using CsvHelper.Configuration;
using KeywordFighting.Model;
using System.Globalization;
using System.Text.Json;

var closeCombatPath = args.Length > 0 ? args[0] : "data/Keyword fighting - Close Combat.csv";
var equipmentPath = args.Length > 1 ? args[1] : "data/Keyword fighting - Equipment.csv";
var outputPath = args.Length > 2 ? args[2] : "data/KeywordFightingContext.json";

Console.WriteLine($"Loading fight cards from {closeCombatPath}...");
var fightCards = LoadCards<FightCard, FightCardMap>(closeCombatPath).ToList();
Console.WriteLine($"Loaded {fightCards.Count} fight cards.");

Console.WriteLine($"Loading equipment from {equipmentPath}...");
var equipments = LoadCards<Equipment, EquipmentMap>(equipmentPath).Select(equipment => equipment.Cast()).ToList();
Console.WriteLine($"Loaded {equipments.Count} equipment cards.");

var context = JsonSerializer.Serialize(new KeywordFightingContext(fightCards, equipments), SerializationConstants.Options);
File.WriteAllText(outputPath, context);
Console.WriteLine($"Context saved to {outputPath}.");

return;

T[] LoadCards<T, TMap>(string path)
    where TMap : ClassMap
{
    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
    {
        ShouldSkipRecord = args => args.Row.Parser.Record?.All(string.IsNullOrEmpty) ?? false
    };
    using var reader = new StreamReader(path);
    using var csv = new CsvReader(reader, config);

    csv.Context.RegisterClassMap<TMap>();
    return csv.GetRecords<T>().ToArray();
}

sealed class FightCardMap : ClassMap<FightCard>
{
    public FightCardMap()
    {
        Map(m => m.Attack.Name).Name("atak");
        Map(m => m.Attack.Description).Name("opis ataku");
        Map(m => m.Attack.Effectiveness.Block).Name("atak: blok");
        Map(m => m.Attack.Effectiveness.Repulse).Name("atak: zbicie");
        Map(m => m.Attack.Effectiveness.Dodge).Name("atak: unik");
        Map(m => m.Attack.Effectiveness.Capture).Name("atak: związanie");
        Map(m => m.Attack.Effectiveness.Counterattack).Name("atak: kontra");
        Map(m => m.Attack.HitDescription).Name("efekt trafienia");
        Map(m => m.Attack.TargetRaw).Name("miejsce trafienia");
        Map(m => m.Attack.Damage).Name("rany");
        Map(m => m.Attack.Advantage.Bludgeoning).Name("miażdżone");
        Map(m => m.Attack.Advantage.Piercing).Name("kłute");
        Map(m => m.Attack.Advantage.Slashing).Name("sieczne");
        Map(m => m.Attack.Stun).Name("ogłuszenie");
        Map(m => m.Attack.CounterattackResistance).Name("odporność na kontratak");
        Map(m => m.Attack.Strengthen).Name("wzmocnienie x2");
        Map(m => m.Attack.Healing).Name("leczenie");
        Map(m => m.Defense.Name).Name("obrona");
        Map(m => m.Defense.Description).Name("opis obrony");
        Map(m => m.Defense.Effectiveness.Block).Name("obrona: blok");
        Map(m => m.Defense.Effectiveness.Repulse).Name("obrona: zbicie");
        Map(m => m.Defense.Effectiveness.Dodge).Name("obrona: unik");
        Map(m => m.Defense.Effectiveness.Capture).Name("obrona: związanie");
        Map(m => m.Defense.Effectiveness.Counterattack).Name("obrona: kontra");
    }
}

sealed class EquipmentMap : ClassMap<Equipment>
{
    public EquipmentMap()
    {
        Map(m => m.TypeRaw).Name("typ");
        Map(m => m.Name).Name("nazwa");
        Map(m => m.Hands).Name("ręce");
        Map(m => m.Power.Bludgeoning).Name("miażdżone");
        Map(m => m.Power.Piercing).Name("kłute");
        Map(m => m.Power.Slashing).Name("sieczne");
        Map(m => m.Power.Counterattack).Name("kontra");
        Map(m => m.Cost).Name("koszt");
    }
}