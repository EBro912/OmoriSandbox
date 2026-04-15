using OmoriSandbox.Actors;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OmoriSandbox.Battle;

/// <summary>
/// An item. Represents both Snacks and Toys.
/// </summary>
public class Item : BattleAction
{
	/// <summary>
	/// Whether this item is a Toy.
	/// </summary>
	public bool IsToy { get; private set; }
	
	/// <summary>
	/// The path to this item's spritesheet.
	/// </summary>
	public string SpritesheetPath { get; private set; }
	
	/// <summary>
	/// The sprite's atlas index into the spritesheet.
	/// </summary>
	public int SpriteIndex { get; private set; }

    /// <summary>
    /// Creates a new single-target item. Must be registered via <see cref="Modding.Mod.RegisterItem(string, Item)"/> to appear in-game.
    /// </summary>
    /// <param name="name">The name of the item.</param>
    /// <param name="description">The description of the item.<br/>You can use the [actor] tag to place the actor's name in the description.</param>
    /// <param name="target">What this item can target. Mainly used for visual targeting.</param>
    /// <param name="effect">The code that runs when this item is used.</param>
    /// <param name="isToy">Whether this item is a Toy or not.</param>
    /// <param name="priority">The priority of the item during turn order calculation.</param>
    public Item(string name, string description, SkillTarget target, Func<Actor, Actor, Task> effect, bool isToy = false, SkillPriority priority = SkillPriority.Normal)
		: base(name, description, target, priority, effect)
	{
		IsToy = isToy;
		SpritesheetPath = null;
		SpriteIndex = -1;
	}
    
	/// <summary>
	/// Creates a new multi-target item. Must be registered via <see cref="Modding.Mod.RegisterItem(string, Item)"/> to appear in-game.
	/// </summary>
	/// <param name="name">The name of the item.</param>
	/// <param name="description">The description of the item.<br/>You can use the [actor] tag to place the actor's name in the description.</param>
	/// <param name="target">What this item can target. Mainly used for visual targeting.</param>
	/// <param name="effect">The code that runs when this item is used.</param>
	/// <param name="isToy">Whether this item is a Toy or not.</param>
	/// <param name="priority">The priority of the item during turn order calculation.</param>
	public Item(string name, string description, SkillTarget target, Func<Actor, IReadOnlyList<Actor>, Task> effect, bool isToy = false, SkillPriority priority = SkillPriority.Normal)
		: base(name, description, target, priority, effect)
	{
		IsToy = isToy;
		SpritesheetPath = null;
		SpriteIndex = -1;
	}

    /// <summary>
    /// Creates a new single-target item with icon data. Must be registered via <see cref="Modding.Mod.RegisterItem(string, Item)"/> to appear in-game.
    /// </summary>
    /// <param name="name">The name of the item.</param>
    /// <param name="description">The description of the item.<br/>You can use the [actor] tag to place the actor's name in the description.</param>
    /// <param name="target">What this item can target. Mainly used for visual targeting.</param>
    /// <param name="effect">The code that runs when this item is used.</param>
    /// <param name="spritesheetPath">The path to this item's spritesheet.</param>
    /// <param name="spriteIndex">The sprite's atlas index into the spritesheet.</param>
    /// <param name="isToy">Whether this item is a Toy or not.</param>
    /// <param name="priority">The priority of the item during turn order calculation.</param>
	public Item(string name, string description, SkillTarget target, Func<Actor, Actor, Task> effect,
		string spritesheetPath, int spriteIndex, bool isToy = false, SkillPriority priority = SkillPriority.Normal)
		: base(name, description, target, priority, effect)
	{
		IsToy = isToy;
		SpritesheetPath = spritesheetPath;
		SpriteIndex = spriteIndex;
	}
    
	/// <summary>
	/// Creates a new multi-target item with icon data. Must be registered via <see cref="Modding.Mod.RegisterItem(string, Item)"/> to appear in-game.
	/// </summary>
	/// <param name="name">The name of the item.</param>
	/// <param name="description">The description of the item.<br/>You can use the [actor] tag to place the actor's name in the description.</param>
	/// <param name="target">What this item can target. Mainly used for visual targeting.</param>
	/// <param name="effect">The code that runs when this item is used.</param>
	/// <param name="spritesheetPath">The path to this item's spritesheet.</param>
	/// <param name="spriteIndex">The sprite's atlas index into the spritesheet.</param>
	/// <param name="isToy">Whether this item is a Toy or not.</param>
	/// <param name="priority">The priority of the item during turn order calculation.</param>
	public Item(string name, string description, SkillTarget target, Func<Actor, IReadOnlyList<Actor>, Task> effect,
		string spritesheetPath, int spriteIndex, bool isToy = false, SkillPriority priority = SkillPriority.Normal)
		: base(name, description, target, priority, effect)
	{
		IsToy = isToy;
		SpritesheetPath = spritesheetPath;
		SpriteIndex = spriteIndex;
	}
}
