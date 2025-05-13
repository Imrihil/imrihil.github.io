namespace KeywordFighting.Model;
public abstract record WoundCard(string Name, double Value) : Card;
public record LightWoundCard() : WoundCard("Lekka rana", 0.5);
public record SeriousWoundCard() : WoundCard("Poważna rana", 1);
public record StunCard() : WoundCard("Ogłuszenie", 1.46);