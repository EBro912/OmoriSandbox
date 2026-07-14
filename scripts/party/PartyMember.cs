using System;
using Godot;
using System.Linq;
using System.Threading.Tasks;
using OmoriSandbox.Battle;

namespace OmoriSandbox.Actors;

/// <summary>
/// An <see cref="Actor"/> that is considered a party member. Can be inherited to make a new party member.
/// </summary>
public abstract class PartyMember : Actor
{
	internal bool Init(AnimatedSprite2D face, BattlePresetActor actor)
	{
		SpriteFrames animation = Animation;
        if (animation == null)
        {
            GD.PrintErr("Failed to load Face animations for PartyMember: " + Name);
            return false;
        }
        // fall back to neutral on unknown or invalid preset emotions
        if (!animation.HasAnimation(actor.Emotion) || !IsStateValid(actor.Emotion))
        {
            GD.PushWarning($"Invalid emotion '{actor.Emotion}' for PartyMember {Name}, defaulting to neutral.");
            actor.Emotion = "neutral";
        }
        // init animation
        Sprite = face;
		Sprite.SpriteFrames = animation;
		Sprite.Animation = actor.Emotion;
		Sprite.Play();
		SetState(actor.Emotion, true);
		
        // init stats
        Level = actor.Level;
        int idx = Math.Clamp(actor.Level, 1, HPTree.Length) - 1;
		SetBaseStats(new Stats(HPTree[idx], JuiceTree[idx], ATKTree[idx], DEFTree[idx], SPDTree[idx], BaseLuck, 0) + actor.AdjustedStats);
		if (!Database.TryGetEquipment(actor.Weapon, out Equipment w))
		{
			GD.PrintErr("Failed to find Weapon: " + actor.Weapon);
			return false;
		}
		Weapon = w;
		
		if (!actor.Charm.Equals("none", System.StringComparison.CurrentCultureIgnoreCase))
		{
			if (!Database.TryGetEquipment(actor.Charm, out Equipment c))
			{
				GD.PrintErr("Failed to find Charm: " + actor.Charm);
				return false;
			}
			Charm = c;
		}

		if (actor.Emotion == "toast")
			CurrentHP = 0;
		else
			CurrentHP = CurrentStats.MaxHP;
		CurrentJuice = CurrentStats.MaxJuice;

		EquippedSkills = actor.Skills;

		foreach (string s in EquippedSkills)
		{
			if (string.IsNullOrWhiteSpace(s))
				continue;

			if (Database.TryGetSkill(s, out var skill))
			{
				if (!Skills.TryAdd(s, skill)) 
					GD.PushWarning($"Actor {Name} already has skill {s} equipped! Skipping...");
				continue;
			}
			GD.PrintErr("Unknown skill: " + s);
		}

		return true;
	}

	/// <summary>
	/// The party member's base stats, plus any stats given by a <see cref="Battle.Weapon"/> and/or <see cref="Equipment"/>.
	/// </summary>
	/// <returns></returns>
	protected override Stats GetBaseStats()
	{
		Stats stats = BaseStats;
		Weapon.Apply(ref stats);
		Charm?.Apply(ref stats);
		return stats;
	}

	/// <inheritdoc/>
	public override bool IsStateValid(string state)
	{
		if (state is "neutral" or "toast" or "victory")
			return true;
		if (Charm?.Name == "Paper Bag")
			return false;
		return !InvalidStates.Contains(state);
	}

    /// <inheritdoc/>
    public override async Task OnStartOfBattle()
    {
	    await Weapon.StartOfBattle(this);
		if (Charm != null)
			await Charm.StartOfBattle(this);
    }

    /// <inheritdoc/>
    public abstract SpriteFrames Animation { get; }
	/// <summary>
	/// The party member's HP scaling, stat by level.
	/// </summary>
	public abstract int[] HPTree { get; }
    /// <summary>
    /// The party member's Juice stat scaling, by level.
    /// </summary>
    public abstract int[] JuiceTree { get; }
    /// <summary>
    /// The party member's ATK stat scaling, by level.
    /// </summary>
    public abstract int[] ATKTree { get; }
    /// <summary>
    /// The party member's DEF stat scaling, by level.
    /// </summary>
    public abstract int[] DEFTree { get; }
    /// <summary>
    /// The party member's SPD stat scaling, by level.
    /// </summary>
    public abstract int[] SPDTree { get; }
    /// <summary>
    /// The party member's LCK stat.
    /// </summary>
    public abstract int BaseLuck { get; }
	/// <summary>
	/// The party member's equipped charm. Will be null if no charm is equipped.
	/// </summary>
	public Equipment Charm { get; private set; }
	/// <summary>
	/// The party member's equipped weapon.
	/// </summary>
	public Equipment Weapon { get; private set; }
	/// <summary>
	/// A list of skills IDs that this actor has equipped.
	/// </summary>
	public string[] EquippedSkills { get; protected set; }
	/// <summary>
	/// A list of Weapons that this actor can equip in the base game.<br/>
	/// Mainly used for the "Filter Equippable" setting in the editor.
	/// </summary>
	public virtual string[] EquippableWeapons { get; protected set; } = [];
	/// <summary>
	/// A list of invalid states this party member cannot feel. Used in <see cref="IsStateValid(string)"/>
	/// </summary>
	public abstract string[] InvalidStates { get; }
	/// <summary>
	/// If this party member is considered to be a "real world" member. Mainly used to change the UI buttons.
	/// </summary>
	public abstract bool IsRealWorld { get; }
	/// <summary>
	/// Whether this party member has plot armor enabled.
	/// </summary>
	/// <remarks>
	/// This only checks if they have it enabled, not if it is currently active.<br/>
	/// Use <see cref="Actor.HasStatModifier"/>("PlotArmor") for that purpose.
	/// </remarks>
	public virtual bool HasPlotArmor => false;
	/// <summary>
	/// Whether this party member has already used their plot armor this battle.
	/// </summary>
	internal bool HasUsedPlotArmor = false;
}
