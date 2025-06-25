public struct Stats
{
    public int HP;
    public int MaxHP;
    public int Juice;
    public int MaxJuice;
    public int ATK;
    public int DEF;
    public int SPD;
    public int LCK;
    public int HIT;

    public Stats(int hp = 0, int juice = 0, int atk = 0, int def = 0, int spd = 0, int lck = 0, int hit = 0)
    {
        HP = hp;
        MaxHP = hp;
        Juice = juice;
        MaxJuice = juice;
        ATK = atk;
        DEF = def;
        SPD = spd;
        LCK = lck;
        HIT = hit;
    }

    public static Stats operator +(Stats a, Stats b) {
        Stats result = new(a.HP + b.HP, a.Juice + b.Juice, a.ATK + b.ATK, a.DEF + b.DEF, a.SPD + b.SPD, a.LCK + b.LCK, a.HIT + b.HIT);
        result.MaxHP = a.HP + b.HP;
        result.MaxJuice = a.Juice + b.Juice;
        return result;
    }
}