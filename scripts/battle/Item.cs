using System;
using System.Threading.Tasks;

public class Item : BattleAction
{
	public Func<Actor, Actor, Item, Task> Effect;
	public bool IsToy = false;
}
