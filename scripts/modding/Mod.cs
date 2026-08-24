using Godot;
using HarmonyLib;
using OmoriSandbox.Actors;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;
using OmoriSandbox.Battle.Modifier;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OmoriSandbox.Extensions;

namespace OmoriSandbox.Modding;

/// <summary>
/// The base mod class that all mods must inherit from.
/// </summary>
public abstract partial class Mod : Node
{
    /// <summary>
    /// The mod's ID, as declared in its mod.json.
    /// </summary>
    public string Id { get; internal set; }

    private Harmony _harmony;

    /// <summary>
    /// This mod's <see cref="HarmonyLib.Harmony"/> instance, created on first use.<br/>
    /// Harmony patches are applied to the whole game, not just this mod's content. A patch on a
    /// shared method like <see cref="BattleManager.Damage"/> runs for every actor in every battle.
    /// Always guard patches so they only affect the actors or battles they are meant for,
    /// e.g. <c>if (__instance is not MyEnemy) return;</c>
    /// </summary>
    protected Harmony Harmony
    {
        get
        {
            if (_harmony == null)
            {
                if (OS.HasFeature("editor"))
                    GD.PushWarning($"Mod {Id} is creating a Harmony instance in an editor build. " +
                                   "Patches can prevent the editor from reloading assemblies until it is restarted.");
                _harmony = new Harmony($"omorisandbox.mod.{Id}");
            }
            return _harmony;
        }
    }

    /// <summary>
    /// Removes every Harmony patch this mod has applied.
    /// Called automatically by the <see cref="ModManager"/> when the game exits.
    /// </summary>
    internal void UnpatchAll() => _harmony?.UnpatchAll(_harmony.Id);

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
    /// <param name="id">The ID of the stat modifier. This is the ID used in functions like <see cref="Actor.AddStatModifier(string, int, bool)"/>.</param>
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
    /// Register groups before the emotions that belong to them.
    /// </summary>
    /// <param name="group">The group to add. Its id doubles as the group's attack element.</param>
    protected static void RegisterEmotionGroup(EmotionGroup group)
    {
        Database.RegisterModdedEmotionGroup(group);
    }

    /// <summary>
    /// Registers a new <see cref="Emotion"/> to the database.<br/>
    /// Requires the actor to have an animation matching its id (or <see cref="Emotion.AnimationName"/>)
    /// in their SpriteFrames and are able to feel the emotion.
    /// Example:
    /// <code>
    /// RegisterEmotion(new Emotion("smug")
    ///     .WithGroup("happy", tier: 4) // tiers are 1-based, tier 4 extends above MANIC
    ///     .WithStatBonuses(new StatBonus(StatType.LCK, 2.5f), new StatBonus(StatType.HIT, -15))
    ///     .WithAsset(EmotionAsset.FromModTextures("MyMod/sprites/smug_label.png", "MyMod/sprites/smug_face.png")));
    /// </code>
    /// </summary>
    /// <param name="emotion">The emotion to add.</param>
    protected static void RegisterEmotion(Emotion emotion)
    {
        Database.RegisterModdedEmotion(emotion);
    }

    /// <summary>
    /// Registers a new followup set. It appears in the editor's followup dropdown after the
    /// vanilla sets and can be assigned to any party member slot.<br/>
    /// A set maps up to three <see cref="FollowupInput"/> directions to <see cref="FollowupEntry"/>
    /// bubbles, omitted directions are hidden in battle.
    /// </summary>
    /// <remarks>
    /// Entries whose skill name starts with <c>ReleaseEnergy</c> are considered Release Energy skills and will
    /// cost 10 energy to use, as well as requiring the entire party being alive.
    /// </remarks>
    /// <param name="id">The ID of the set. This is how it appears in the editor dropdown and in presets.</param>
    /// <param name="entries">The bubbles by role. <see cref="FollowupInput.Horizontal"/> faces the enemies from the member's slot.</param>
    /// <param name="tiered">Whether the battle's followup tier (1-3) is appended to each skill name,
    /// requiring skills like <c>MySkill1</c>/<c>MySkill2</c>/<c>MySkill3</c> to be registered.</param>
    protected static void RegisterFollowupSet(string id, IReadOnlyDictionary<FollowupInput, FollowupEntry> entries, bool tiered = false)
    {
        Database.RegisterModdedFollowupSet(id, entries, tiered);
    }
}