public class StatModifier
{
    public Modifier Modifier { get; private set; }
    public int Tier { get; private set; }
    public int TurnsLeft { get; private set; }

    public StatModifier(Modifier modifier, int tier = 1)
    {
        Modifier = modifier;
        Tier = tier;
        TurnsLeft = 6;
    }

    public bool IncreaseTier()
    {
        if (Tier == 3)
            return false;
        Tier++;
        TurnsLeft = 6;
        return true;
    }

    public void DecreaseTurn()
    {
        TurnsLeft--;
    }
}

public enum Modifier
{
    AttackUp,
    DefenseUp,
    SpeedUp,
    AttackDown,
    DefenseDown,
    SpeedDown
}