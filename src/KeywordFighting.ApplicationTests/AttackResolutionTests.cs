using FluentAssertions;
using KeywordFighting.Model;

namespace KeywordFighting.ApplicationTests;

public class AttackResolutionTests
{
    [Fact]
    public void Should_ResolveAttack()
    {
        var attacker = new Character
        {
            Weapon = new Weapon("Włócznia", 2, 1, 1.5, 1, 2),
            Helmet = new Helmet("Łebka", 2, 2, 3),
            Armor = new Armor("Koszula", 2, 1, 1),
            Bracers = new Bracers("Rękawice", 2, 1, 1),
            Greaves = new Greaves("Kalesony", 2, 1, 1)
        };
        var attackerCard = new FightCard
        {
            Attack = new AttackStats("Cios górny", TargetEnum.Hands, new Power(0.5, 0, 0.5), new Effectiveness(-1, 1, 1, 0, 1), 7)
        };
        var defender = new Character
        {
            Weapon = new Weapon("Miecz treningowy", 1, 0.5, 0, 0.5, 1),
            Armor = new Armor("Koszula", 2, 1, 1),
            Bracers = new Bracers("Skórzane karwasze", 3, 2, 2)
        };
        var defenderCard = new FightCard
        {
            Defense = new DefenseStats("Parada", new Effectiveness(1, 0, 0, 0, 0))
        };
        var outcome = attackerCard.Attack.GetOutcome(attacker, defender, defenderCard.Defense) as SuccessfulAttackOutcome;
        outcome.Should().NotBeNull();
        outcome.Damage.Count.Should().Be(3.5);
    }

    [Fact]
    public void Should_GetWeaponMultiplier()
    {
        var weapon = new Weapon("Włócznia", 2, 1, 1.5, 1, 2);
        var attackAdvantage = new Power(0.5, 0, 0.5);
        var armor = new Bracers("Skórzane karwasze", 3, 2, 2);
        var outcome = weapon.GetMultiplier(attackAdvantage, armor);
        outcome.Should().Be(0.5);
    }

    [Fact]
    public void Should_GetWeaponMultiplier_2()
    {
        var weapon = new Weapon("Nóż", 1, 0, 0.5, 0, 0);
        var attackAdvantage = new Power(0.5, 0, 0.5);
        var armor = new Helmet("Kaptur", 2, 1, 1);
        var outcome = weapon.GetMultiplier(attackAdvantage, armor);
        outcome.Should().Be(0.5);
    }

    [Fact]
    public void Should_ResolveAttack_3()
    {
        var attacker = new Character
        {
            Weapon = new Weapon("Włócznia", 2, 1, 1.5, 1, 2),
            Helmet = new Helmet("Kaptur", 2, 1, 1)
        };
        var attackerCard = new FightCard
        {
            Attack = new AttackStats("Uderzenie rękojeścią", TargetEnum.Hands, new Power(0.5, 0.5, 0), new Effectiveness(1, 0, -1, 1, -1), 0.5, true)
        };
        var defender = new Character
        {
            Weapon = new Weapon("Krótki miecz", 1, 1, 1, 1.5, 2),
            Helmet = new Helmet("Kapelusz", 2, 1, 1),
            Bracers = new Bracers("Rękawice", 2, 1, 1),
            Greaves = new Greaves("Kalesony", 2, 1, 1)
        };
        var defenderCard = new FightCard
        {
            Defense = new DefenseStats("Podążanie", new Effectiveness(0, 0, 1, 0, 1))
        };
        var outcome = attackerCard.Attack.GetOutcome(attacker, defender, defenderCard.Defense) as SuccessfulAttackOutcome;
        outcome.Should().NotBeNull();
        outcome.Damage.Count.Should().Be(2);
        outcome.Stun.Should().Be(4);
    }

    [Fact]
    public void Should_GetWeaponMultiplier_3()
    {
        var weapon = new Weapon("Włócznia", 2, 1, 1.5, 1, 2);
        var attackAdvantage = new Power(0.5, 0.5, 0);
        var armor = new Bracers("Rękawice", 2, 1, 1);
        var outcome = weapon.GetMultiplier(attackAdvantage, armor);
        outcome.Should().Be(2);
    }

    [Fact]
    public void Should_ResolveAttack_4()
    {
        var attacker = new Character
        {
            Weapon = new Weapon("Włócznia", 2, 1, 1.5, 1, 2)
        };
        var attackerCard = new FightCard
        {
            Attack = new AttackStats("Sztych", TargetEnum.Head, new Power(0, 0.5, 0), new Effectiveness(-1, 0, 1, 1, 0), 5)
        };
        var defender = new Character
        {
            Weapon = new Weapon("Krótki miecz", 1, 1, 1, 1.5, 2),
            Armor = new Armor("Kolczuga", 2, 3, 3),
            Helmet = new Helmet("Kapelusz", 2, 1, 1),
            Bracers = new Bracers("Skórzane karwasze", 3, 2, 2),
            Greaves = new Greaves("Spodnie lamelkowe", 3, 3, 4)
        };
        var defenderCard = new FightCard
        {
            Defense = new DefenseStats("Parada", new Effectiveness(1, 0, 0, 0, 0))
        };
        var outcome = attackerCard.Attack.GetOutcome(attacker, defender, defenderCard.Defense) as SuccessfulAttackOutcome;
        outcome.Should().NotBeNull();
        outcome.Damage.Count.Should().Be(10);
    }

    [Fact]
    public void Should_ResolveCounterattack()
    {
        var attacker = new Character
        {
            Weapon = new Weapon("Oszczep", 1, 1, 1.5, 1, 2),
            Armor = new Armor("Brygantyna", 3, 3, 4)
        };
        var attackerCard = new FightCard
        {
            Attack = new AttackStats("Cios górny", TargetEnum.Hands, new Power(0.5, 0, 0.5), new Effectiveness(-1, 1, 1, 0, 1), 7)
        };
        var defender = new Character
        {
            Weapon = new Weapon("Młot", 2, 2.5, 2, 2, 2)
        };
        var defenderCard = new FightCard
        {
            Defense = new DefenseStats("Finta", new Effectiveness(0, 0, 0, 0, 1))
        };
        var outcome = attackerCard.Attack.GetOutcome(attacker, defender, defenderCard.Defense) as FailedAttackOutcome;
        outcome.Should().NotBeNull();
        outcome.CounterattackDamage.Count.Should().Be(0.5);
    }

    [Fact]
    public void Should_GetCounterattackWeaponMultiplier()
    {
        var weapon = new Weapon("Młot", 2, 2.5, 2, 2, 2);
        var armor = new Armor("Brygantyna", 3, 3, 4);
        var outcome = weapon.GetCounterattackMultiplier(armor);
        outcome.Should().Be(0.5);
    }
}
