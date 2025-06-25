public struct Weapon(string name, Stats stats)
{
    public string Name { get; private set; } = name;
    public Stats Stats { get; private set; } = stats;
}