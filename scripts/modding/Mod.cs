using Godot;
using OmoriSandbox.Actors;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Modifier;
using System;

namespace OmoriSandbox.Modding;

/// <summary>
/// The base mod class that all mods must inherit from.
/// </summary>
public abstract partial class Mod : Node
{
    /// <summary>
    /// Fired whenever the mod is first loaded. Equivalent to Godot's _Ready function.
    /// </summary>
    public abstract void OnLoad();

    /// <summary>
    /// Fired whenever the game is about to close.
    /// </summary>
    public virtual void OnUnload() { }

    /// <summary>
    /// Fired whenever Godot's _Process function is called.
    /// </summary>
    /// <param name="delta">The delta time between this frame and the previous one.</param>
    public virtual void OnProcess(double delta) { }

    /// <summary>
    /// Registers a new <see cref="PartyMember"/> to the database.
    /// </summary>
    /// <typeparam name="T">The class of your custom party member. Must inherit <see cref="PartyMember"/>.</typeparam>
    /// <param name="id">The ID of the party member. This will be how it appears in editor menus.</param>
    protected static void RegisterPartyMember<T>(string id) where T : PartyMember, new()
    {
        Database.RegisterModdedPartyMember<T>(id);
    }

    /// <summary>
    /// Registers a new <see cref="Enemy"/> to the database.
    /// </summary>
    /// <typeparam name="T">The class of your custom enemy. Must inherit <see cref="Enemy"/></typeparam>
    /// <param name="id">The ID of the enemy. This will be how it appears in editor menus.</param>
    protected static void RegisterEnemy<T>(string id) where T : Enemy, new()
    {
        Database.RegisterModdedEnemy<T>(id);
    }

    /// <summary>
    /// Registers a new <see cref="Skill"/> to the database.
    /// </summary>
    /// <param name="id">The ID of the skill. This will be the ID used to give the skill to actors.</param>
    /// <param name="skill">The <see cref="Skill"/> to add.</param>
    protected static void RegisterSkill(string id, Skill skill)
    {
        Database.RegisterModdedSkill(id, skill);
    }

    /// <summary>
    /// Registers a new <see cref="Item"/> to the database.
    /// </summary>
    /// <param name="id">The ID of the item. This will be how it appears in editor menus.</param>
    /// <param name="item">The <see cref="Item"/> to add.</param>
    protected static void RegisterItem(string id, Item item)
    {
        Database.RegisterModdedItem(id, item);
    }

    /// <summary>
    /// Registers a new <see cref="Weapon"/> to the database.
    /// </summary>
    /// <param name="id">The ID of the weapon. This will be how it appears in editor menus.</param>
    /// <param name="weapon">The <see cref="Weapon"/> to add.</param>
    protected static void RegisterWeapon(string id, Weapon weapon)
    {
        Database.RegisterModdedWeapon(id, weapon);
    }

    /// <summary>
    /// Registers a new <see cref="Charm"/> to the database.
    /// </summary>
    /// <param name="id">The ID of the charm. This will be how it appears in editor menus.</param>
    /// <param name="charm">The <see cref="Charm"/> to add.</param>
    protected static void RegisterCharm(string id, Charm charm)
    {
        Database.RegisterModdedCharm(id, charm);
    }

    /// <summary>
    /// Registers a new <see cref="StatModifier"/> to the database.
    /// </summary>
    /// <param name="id">The ID of the stat modifier. This is the ID used in functions like <see cref="Actor.AddStatModifier(string, bool)"/>.</param>
    /// <param name="func">The function used to construct the stat modifier when called.<br/>
    /// This allows you to easily build new stat modifiers, such as the following:<br/>
    /// <c>() => new StatModifier(new StatBonus(StatType.ATK, 1.3f), new StatBonus(StatType.DEF, 0.5f))</c>
    /// </param>
    protected static void RegisterStatModifier(string id, Func<StatModifier> func)
    {
        Database.RegisterModdedStatModifier(id, func);
    }
}