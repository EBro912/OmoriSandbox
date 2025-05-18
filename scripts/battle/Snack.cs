using System;
using System.Threading.Tasks;

public class Snack : BattleAction
{
    public Func<Actor, Actor, Snack, Task> Effect;
}