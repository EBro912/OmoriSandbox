public struct EnemyStats
{
    public int HP;
    public int Juice;
    public int ATK;
    public int DEF;
    public int SPD;
    public int LCK;

    public EnemyStats(int hp, int juice, int atk, int def, int spd, int lck)
    {
        HP = hp;
        Juice = juice;
        ATK = atk;
        DEF = def;
        SPD = spd;
        LCK = lck;
    }
}