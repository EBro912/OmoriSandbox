using Godot;
using OmoriSandbox.Actors;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;
using OmoriSandbox.Battle.Modifier;
using System;
using System.Threading.Tasks;
using OmoriSandbox.Extensions;

namespace OmoriSandbox.Modding;

/// <summary>
/// The base mod class that all mods must inherit from.
/// </summary>
public abstract partial class Mod : Node
{
    /// <summary>
    /// Called after the mod has been loaded and added to the scene tree.
    /// </summary>
    public virtual void OnLoad() { }
    
    /// <summary>
    /// Called each frame within Godot's _Process method.
    /// </summary>
    /// <param name="delta">The time difference between the current and previous frame.</param>
    public virtual void OnProcess(double delta) {}
    
    /// <summary>
    /// Called before the mod is removed from the scene tree.
    /// </summary>
    public virtual void OnUnload() {}

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
    /// Registers a new <see cref="Equipment"/> to the database.
    /// </summary>
    /// <param name="id">The ID of the equipment. This will be how it appears in editor menus.</param>
    /// <param name="equipment">The <see cref="Equipment"/> to add.</param>
    protected static void RegisterEquipment(string id, Equipment equipment)
    {
        Database.RegisterModdedEquipment(id, equipment);
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

    /// <summary>
    /// Registers a new <see cref="EmotionGroup"/> to the database.<br/>
    /// Register families before the emotions that belong to them.
    /// </summary>
    /// <param name="group">The group to add. Its id doubles as the group's attack element.</param>
    protected static void RegisterEmotionFamily(EmotionGroup group)
    {
        Database.RegisterModdedEmotionFamily(group);
    }

    /// <summary>
    /// Registers a new <see cref="Emotion"/> to the database.<br/>
    /// Actors opt in to a custom emotion by having an animation matching its id (or <see cref="Emotion.AnimationName"/>)
    /// in their SpriteFrames and by not listing the id in their invalid states.
    /// Example:
    /// <code>
    /// RegisterEmotion(new Emotion("smug")
    ///     .WithGroup("happy", tier: 1)
    ///     .WithStatBonuses(new StatBonus(StatType.LCK, 2.5f), new StatBonus(StatType.HIT, -15))
    ///     .WithAsset(EmotionAsset.FromModTextures("MyMod/sprites/smug_label.png", "MyMod/sprites/smug_face.png")));
    /// </code>
    /// </summary>
    /// <param name="emotion">The emotion to add.</param>
    protected static void RegisterEmotion(Emotion emotion)
    {
        Database.RegisterModdedEmotion(emotion);
    }
}