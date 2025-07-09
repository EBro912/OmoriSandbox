using System;
using System.Threading.Tasks;

/// <summary>
/// Represents an item, including both Snacks and Toys.
/// </summary>
public class Item : BattleAction
{
	/// <summary>
	/// Whether or not this item is a Toy.
	/// </summary>
	public bool IsToy { get; private set; }

	/// <summary>
	/// Creates a new item. Must be added to the <see cref="Database"/> in order for it to be usable in-game.
	/// </summary>
	public Item(string name, string description, SkillTarget target, Func<Actor, Actor, Task> effect, bool isToy = false)
		: base(name, description, target, effect)
	{
		IsToy = isToy;
	}
}
