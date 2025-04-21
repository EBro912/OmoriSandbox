public abstract class Actor
{
    public abstract string Name { get; }
    public string CurrentState;
    public int Level = 1;
    public int HP = 0;
    public int MaxHP = 0;
    public int Juice = 0;
    public int MaxJuice = 0;
    public int ATK = 0;
    public int DEF = 0;
    public int SPD = 0;
    public int LCK = 0;
    public int HIT = 0;

    public void Damage(int damage)
    {
        HP -= damage;
        if (HP < 0) 
            HP = 0;
    }

}