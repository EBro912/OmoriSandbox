using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;
using OmoriSandbox.Battle.Modifier;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OmoriSandbox.Editor;

namespace OmoriSandbox.Actors;

/// <summary>
/// A generic actor. See <see cref="PartyMember"/> and <see cref="Enemy"/>.
/// </summary>
public abstract class Actor
{
	/// <summary>
	/// Fired whenever the actor's emotion changes.
	/// </summary>
	public event EventHandler OnEmotionChanged;
	/// <summary>
	/// Fired whenever the actor's HP changes.
	/// </summary>
	public event EventHandler OnHPChanged;
	/// <summary>
	/// Fired whenever the actor's Juice changes.
	/// </summary>
	public event EventHandler OnJuiceChanged;
	/// <summary>
	/// Fired whenever the actor takes damage.
	/// </summary>
	public event EventHandler OnDamaged;
	/// <summary>
	/// Fired whenever the actor's animation override changes. See <see cref="PlayAnimation"/> and <see cref="ClearAnimation"/>.
	/// </summary>
	public event EventHandler OnAnimationChanged;
	/// <summary>
	/// Fired after a <see cref="StatModifier"/> is added to this actor.<br/>
	/// Not fired for tier or duration changes to a modifier the actor already has.
	/// </summary>
	public event EventHandler<StatModifierEventArgs> OnStatModifierAdded;
	/// <summary>
	/// Fired after a <see cref="StatModifier"/> is removed from this actor for any reason.
	/// </summary>
	public event EventHandler<StatModifierRemovedEventArgs> OnStatModifierRemoved;
	/// <summary>
	/// Fired when this actor is revived. See <see cref="Revive"/>.
	/// </summary>
	public event EventHandler OnRevived;
	
	/// <summary>
	/// The name of the actor.
	/// </summary>
	public abstract string Name { get; }
	/// <summary>
	/// The actor's sprite.
	/// </summary>
	public AnimatedSprite2D Sprite { get; protected set; }
	/// <summary>
	/// The center point of the actor, calculated by the center of its sprite.
	/// </summary>
	/// <remarks>
	/// For a party member, it's (57, 82) pixels from the top-left corner of their battle card.
	/// </remarks>
	public Vector2 CenterPoint = Vector2.Zero;

	private ActorComponent _Component;
	/// <summary>
	/// The actor's node in the current battle, or null when it isn't in one.
	/// </summary>
	/// <remarks>
	/// This gets set when the actor spawns into a battle, it will be null again as soon as that node starts being removed, including when the
	/// battle is exited or restarted, when a Boss Rush stage changes, when the enemy is replaced with <see cref="BattleManager.TransformEnemy"/> or <see cref="BattleManager.ReplaceEnemy"/>,
	/// or when spawning fails.<br/>
	/// <see cref="Enemy.Component"/> and <see cref="PartyMember.Component"/> return their own component types.
	/// </remarks>
	public virtual ActorComponent Component =>
		GodotObject.IsInstanceValid(_Component) && !_Component.IsQueuedForDeletion()
		&& _Component.GetParent()?.IsQueuedForDeletion() != true ? _Component : null;

	internal void Bind(ActorComponent component)
	{
		_Component = component;
	}

	internal void Unbind(ActorComponent component)
	{
		if (_Component == component)
			_Component = null;
	}

	/// <summary>
	/// The actor this actor's last action ended up targeting, or null before its first action.
	/// </summary>
	/// <remarks>
	/// Recorded when an action starts, after retargeting and taunts. For an action with several targets, the last one will be set.<br/>
	/// Actions that never start, such as ones refused for JUICE, don't change it. Some skills don't update it, or record their user; see <see cref="Skill.WithLastTarget"/>.
	/// </remarks>
	public Actor LastTarget { get; internal set; }

	/// <summary>
	/// The index of <see cref="LastTarget"/> on the actor's own side, or 0 before this actor's first action.
	/// </summary>
	/// <remarks>
	/// Party members are counted like <see cref="BattleManager.GetPartyMember"/> and enemies like <see cref="BattleManager.GetEnemy"/>.<br/>
	/// This will be -1 if the target wasn't in the battle when the action started.
	/// </remarks>
	public int LastTargetIndex { get; internal set; }
	/// <summary>
	/// The actor's current <see cref="Emotion"/>. Defaults to neutral.
	/// </summary>
	public Emotion CurrentEmotion { get; private set; } = Database.NeutralEmotion;
	/// <summary>
	/// The emotion used in damage and advantage calculations.<br/>
	/// Usually the same as <see cref="CurrentEmotion"/>, unless an emotion lock with an advantage override is active. See <see cref="LockEmotion"/>.
	/// </summary>
	public Emotion EffectiveEmotion => IsEmotionLocked && LockedAdvantageEmotion != null ? LockedAdvantageEmotion : CurrentEmotion;
	/// <summary>
	/// The name of the animation currently overriding this actor's sprite, or null if no override is active.<br/>
	/// While an override is active, emotion changes will not change the animation override until cleared. See <see cref="PlayAnimation"/>.
	/// </summary>
	public string CurrentAnimation { get; private set; }
	/// <summary>
	/// The skills the actor has equipped.
	/// </summary>
	public readonly Dictionary<string, Skill> Skills = [];

	/// <summary>
	/// The actor's absolute base stats with no modifications.
	/// </summary>
	/// <remarks>
	/// <see cref="GetBaseStats"/> should usually be used instead, as it takes things like Weapons and Charms into account for PartyMembers.
	/// </remarks>
	protected Stats BaseStats { get; private set; }

    /// <summary>
    /// The actor's level. Mainly only used for <see cref="PartyMember"/>s.
    /// </summary>
    public int Level { get; protected set; } = 1;

	/// <summary>
	/// The <see cref="StatModifier"/>s the actor currently has.
	/// </summary>
	public readonly Dictionary<string, StatModifier> StatModifiers = [];

	private static long LastModifierSequence;

	private int _CurrentHP = 0;
	/// <summary>
	/// The actor's current HP. Updating this value will fire <see cref="OnHPChanged"/>.
	/// </summary>
	public int CurrentHP
	{
		get => _CurrentHP;
		set
		{
			_CurrentHP = value;
			BattleManager.Instance.RunAsReaction(() => OnHPChanged?.Invoke(this, EventArgs.Empty));
		}
	} 

	private int _CurrentJuice = 0;

	/// <summary>
    /// The actor's current Juice. Updating this value will fire <see cref="OnJuiceChanged"/>.
    /// </summary>
    public int CurrentJuice
	{
		get => _CurrentJuice;
		set
		{
			_CurrentJuice = value;
			BattleManager.Instance.RunAsReaction(() => OnJuiceChanged?.Invoke(this, EventArgs.Empty));
		}
	}

	/// <summary>
	/// Whether the actor is currently stunned. Setting this value to true will cause their actions to be skipped. Forced commands will still run.
	/// </summary>
	// TODO: think of a better solution for this, as different states can fight for this variable
	public bool Stunned = false;

	/// <summary>
	/// Whether the actor is toast.
	/// </summary>
	public bool IsToast { get; private set; }
	
	/// <summary>
	/// The actor's base stats without any modifiers.
	/// </summary>
	/// <returns></returns>
	public virtual Stats GetBaseStats() { return BaseStats; }

	/// <summary>
	/// Sets the base stats for this actor.
	/// </summary>
	/// <param name="stats">The Stats to set.</param>
	protected void SetBaseStats(Stats stats)
	{
		BaseStats = stats;
	}

	/// <summary>
	/// The Actor's base stats, any adjusted stats from equips or modifiers, and emotion stats.
	/// Stats are capped at the very end of the calculation, and HIT/EVA are not capped at all.
	/// </summary>
	public Stats CurrentStats
	{
		get
		{
			Stats current = GetBaseStats();
			foreach (StatType stat in ParamTypes)
				current.ApplyParam(stat, GetParamPlus(stat), GetExactParamRate(stat));
			// single round + clamp at the very end, matching the base game
			current.ApplyStatLimits();
			return current;
		}
	}

	private static readonly StatType[] ParamTypes = Enum.GetValues<StatType>();

	/// <summary>
	/// The actor's total stat multiplier from their current emotion and all stat modifiers,
	/// like RPGMaker's <c>battler.paramRate</c>.
	/// </summary>
	/// <remarks>
	/// Flat bonuses and the stat limit are not included.
	/// Equipment is included in the base stats.
	/// </remarks>
	/// <param name="stat">The <see cref="StatType"/> to get the total multiplier for.</param>
	public float GetParamRate(StatType stat)
	{
		return (float)GetExactParamRate(stat);
	}

	private double GetExactParamRate(StatType stat)
	{
		double rate = 1d;
		foreach (StatBonus bonus in GetEmotionStatBonuses(CurrentEmotion))
		{
			if (bonus.Type == stat)
				rate *= StatBonus.ExactRate(bonus.Multiplier);
		}
		foreach (StatModifier mod in StatModifiers.Values)
			rate *= StatBonus.ExactRate(mod.GetParamRate(stat));
		return rate;
	}

	/// <summary>
	/// The actor's total flat stat bonus from their current emotion and all stat modifiers,
	/// like RPGMaker's <c>battler.paramPlus</c>.
	/// </summary>
	/// <remarks>
	/// Multipliers and the stat limit are not included. 
	/// Equipment is included in the base stats.
	/// </remarks>
	/// <param name="stat">The <see cref="StatType"/> to get the total flat bonus for.</param>
	public int GetParamPlus(StatType stat)
	{
		int plus = 0;
		foreach (StatBonus bonus in GetEmotionStatBonuses(CurrentEmotion))
		{
			if (bonus.Type == stat)
				plus += bonus.FlatBonus;
		}
		foreach (StatModifier mod in StatModifiers.Values)
			plus += mod.GetParamPlus(stat);
		return plus;
	}

	/// <summary>
	/// Adds a new <see cref="StatModifier"/> to this actor.
	/// </summary>
	/// <param name="modifier">The name of the modifier to add.</param>
	/// <param name="turns">Overrides the default number of turns to give this modifier for. If unchanged, will use the default turn count for the modifier.</param>
	/// <param name="silent">If true, success/failure messages will not be logged.</param>
	public void AddStatModifier(string modifier, int turns = -1, bool silent = false)
	{
		if (StatModifiers.TryGetValue(modifier, out StatModifier m) && m is not TierStatModifier)
		{
			if (IsReapplyBlocked(modifier))
				return;
			m.RefreshTurns();
			GrantFreeActionTick(m);
			GD.Print("Refreshed modifier " + modifier + " on " + Name);
			return;
		}

		StatModifier mod = m ?? Database.CreateModifier(modifier);
		if (mod == null)
		{
			GD.PrintErr("Unknown stat modifier: " + modifier);
			return;
		}

		if (mod is TierStatModifier)
		{
			// tiered modifiers all use AddTierStatModifier so stacking and combining behave the same everywhere
			AddTierStatModifier(modifier, 1, turns, silent);
			return;
		}

		if (IsReapplyBlocked(modifier))
			return;

		if (turns > -1)
		{
			mod.SetTurnsLeft(turns);
		}

		mod.AddSequence = ++LastModifierSequence;
		StatModifiers.Add(modifier, mod);
		GrantFreeActionTick(mod);
		BattleManager.Instance.RunAsReaction(() => mod.OnAdd(this));
		GD.Print("Added modifier " + modifier + " to " + Name);
		NotifyModifierAdded(modifier, mod);
	}

    /// <summary>
    /// Adds a new <see cref="TierStatModifier"/> to this actor.
    /// </summary>
    /// <param name="modifier">The name of the tier modifier to add.</param>
    /// <param name="tier">The tier that this modifier will start at.</param>
    /// <param name="turns">The number of turns left the modifier will start at. If unchanged (-1), the modifier's registered duration is kept.</param>
    /// <param name="silent">If true, success/failure messages will not be logged.</param>
    public void AddTierStatModifier(string modifier, int tier = 1, int turns = -1, bool silent = false)
	{
		if (tier <= 1 && BattleManager.Instance != null && BattleManager.Instance.TryGetModifierLanding(this, modifier, out int landing))
		{
			int held = GetStatModifierTier(modifier);
			if (held >= landing)
				RefreshTierStatModifier(modifier, held == landing, silent);
			else
				AddTierStatModifierCore(modifier, landing, turns, silent);
			return;
		}
		AddTierStatModifierCore(modifier, tier, turns, silent);
	}

	private void AddTierStatModifierCore(string modifier, int tier, int turns, bool silent)
	{
		StatModifier mod = Database.CreateModifier(modifier);
		if (mod is not TierStatModifier t)
		{
			if (mod == null)
			{
				GD.PrintErr("Unknown stat modifier: " + modifier);
				return;
			}

			GD.PushWarning("Tried to add a non-tiered stat modifier with tier and turns: " + modifier);
			if (IsReapplyBlocked(modifier))
				return;
			if (StatModifiers.TryGetValue(modifier, out StatModifier held))
			{
				held.RefreshTurns();
				GD.Print("Refreshed modifier " + modifier + " on " + Name);
				return;
			}
			if (turns > -1)
				mod.SetTurnsLeft(turns);
			mod.AddSequence = ++LastModifierSequence;
			StatModifiers.Add(modifier, mod);
			BattleManager.Instance.RunAsReaction(() => mod.OnAdd(this));
			GD.Print("Added modifier " + modifier + " to " + Name);
			NotifyModifierAdded(modifier, mod);
			return;
		}

		if (CombineEnabled && t.CounterpartId != null
			&& StatModifiers.TryGetValue(t.CounterpartId, out StatModifier c)
			&& c is TierStatModifier counterpart)
		{
			int net = counterpart.CurrentTier - Math.Clamp(tier, 1, t.MaxTier);
			if (net > 0)
			{
				counterpart.ReduceTier(net);
				GD.Print("Reduced tier of " + t.CounterpartId + " on " + Name + " to " + net + " via " + modifier);
				if (!silent && t.SuccessMessage != null)
					ShowStatMessage(t.SuccessMessage);
				ClampHealthJuiceToMax();
				return;
			}
			RemoveStatModifierInternal(t.CounterpartId, StatModifierRemovalReason.Combined);
			if (net == 0)
			{
				GD.Print("Removed " + t.CounterpartId + " on " + Name + " via " + modifier + " (neutralized)");
				if (!silent && t.SuccessMessage != null)
					ShowStatMessage(t.SuccessMessage);
				return;
			}

			tier = -net;
		}
		if (StatModifiers.TryGetValue(modifier, out StatModifier m))
		{
			TierStatModifier existing = m as TierStatModifier;
			// vanilla adds the state of the tier this lands on, so that's the tier the re-apply rule checks
			int landing = Math.Min(tier, existing.MaxTier);
			if (landing <= existing.CurrentTier)
				landing = existing.CurrentTier == existing.MaxTier ? existing.MaxTier : existing.CurrentTier + 1;
			if (IsReapplyBlocked(modifier, landing))
				return;
			bool success = existing.ApplyTier(tier);
			if (success)
			{
                GD.Print("Increased tier of " + modifier + " on " + Name + " to " + existing.CurrentTier);
			}
			if (!silent && existing.SuccessMessage != null)
			{
				ShowStatMessage(success ? existing.SuccessMessage : existing.FailureMessage);
			}
			ClampHealthJuiceToMax();
			return;
		}
		if (IsReapplyBlocked(modifier, Math.Min(Math.Max(tier, 1), t.MaxTier)))
			return;
		t.WithTier(tier);
		if (turns > -1)
			t.SetTurnsLeft(turns);
		t.AddSequence = ++LastModifierSequence;
		StatModifiers.Add(modifier, t);
		BattleManager.Instance.RunAsReaction(() => t.OnAdd(this));
		GD.Print("Added modifier " + modifier + " to " + Name);
		if (!silent && t.SuccessMessage != null)
			ShowStatMessage(t.SuccessMessage);
		NotifyModifierAdded(modifier, t);
	}

	internal void RefreshTierStatModifier(string modifier, bool refreshTurns, bool silent)
	{
		if (!StatModifiers.TryGetValue(modifier, out StatModifier mod) || mod is not TierStatModifier tier)
			return;
		if (IsReapplyBlocked(modifier, tier.CurrentTier))
			return;
		if (refreshTurns)
			tier.RefreshTurns();
		if (!silent && tier.SuccessMessage != null)
			ShowStatMessage(tier.FailureMessage);
	}

	/// <summary>
	/// Stuns this actor for the given number of turns, so its actions are skipped. Forced commands still run.
	/// </summary>
	/// <remarks>
	/// The stun is the <c>"Stunned"</c> stat modifier, which counts down at the end of each turn. A stun of 1 ends after the current turn,
	/// so it only skips an action the actor hasn't taken yet. Stunning a stunned actor again keeps the longer of the two durations.
	/// Remove the modifier to end the stun early. A stun can be added again right after one ends, even while "Allow State Reapply on Removal" is off.
	/// </remarks>
	/// <param name="turns">The number of turns, at least 1.</param>
	public void Stun(int turns)
	{
		if (turns < 1)
			return;
		if (StatModifiers.TryGetValue("Stunned", out StatModifier stun))
		{
			stun.SetTurnsLeft(Math.Max(stun.TurnsLeft, turns));
			return;
		}
		AddStatModifier("Stunned", turns, silent: true);
	}

	/// <summary>
	/// Removes a <see cref="StatModifier"/> of the given name from this actor.
	/// </summary>
	/// <param name="modifier">The name of the modifier to remove.</param>
	/// <param name="force">If true, removes the modifier even if it bypasses removal. See <see cref="StatModifier.WithBypassRecoverAllRemoval"/>.</param>
	/// <param name="blockReapply">If false, the modifier can be added again right away, even while "Allow State Reapply on Removal" is off.</param>
	public void RemoveStatModifier(string modifier, bool force = false, bool blockReapply = true)
	{
		RemoveStatModifierInternal(modifier, StatModifierRemovalReason.Manual, force, blockReapply);
	}

	/// <summary>
	/// Removes all <see cref="StatModifier"/>s from this actor.
	/// </summary>
	/// <param name="force">If true, also removes modifiers that bypass removal. See <see cref="StatModifier.WithBypassRecoverAllRemoval"/>.</param>
	public void RemoveAllStatModifiers(bool force = false)
	{
		foreach (string modifier in StatModifiers.Keys.ToList())
			RemoveStatModifierInternal(modifier, StatModifierRemovalReason.Cleared, force, false);
	}

	/// <summary>
	/// Whether this actor has a <see cref="StatModifier"/> in the given category. See <see cref="StatModifier.WithCategories"/>.
	/// </summary>
	/// <param name="category">The category name, ignoring case.</param>
	public bool HasStatModifierInCategory(string category)
	{
		return StatModifiers.Values.Any(x => x.HasCategory(category));
	}

	/// <summary>
	/// Removes every <see cref="StatModifier"/> in the given category.
	/// </summary>
	/// <param name="category">The category name, ignoring case.</param>
	/// <param name="force">If true, also removes modifiers that bypass removal. See <see cref="StatModifier.WithBypassRecoverAllRemoval"/>.</param>
	public void RemoveStatModifiersInCategory(string category, bool force = false)
	{
		foreach (KeyValuePair<string, StatModifier> entry in StatModifiers.ToList())
		{
			if (entry.Value.HasCategory(category))
				RemoveStatModifierInternal(entry.Key, StatModifierRemovalReason.Manual, force);
		}
	}

	/// <summary>
	/// Whether this actor can use its skill <paramref name="skill"/> right now.
	/// </summary>
	/// <remarks>
	/// False when the actor doesn't have the skill, its requirement fails (while AFRAID or STRESSED OUT by default), the skill is cooling down (see <see cref="GetSkillCooldown(Skill)"/>), or the actor can't pay its JUICE, HP or Energy cost.
	/// In <see cref="Enemy.ProcessAI"/>, use it to skip a choice the enemy couldn't carry out, and return null when no choice is left, so the enemy takes no action this turn.
	/// </remarks>
	/// <param name="skill">The skill's key in <see cref="Skills"/>.</param>
	/// <returns>True if the skill can be used now.</returns>
	public bool CanUse(string skill)
	{
		return skill != null && Skills.TryGetValue(skill, out Skill value) && CanUse(value);
	}

	/// <summary>
	/// Whether this actor can use <paramref name="skill"/> right now. See <see cref="CanUse(string)"/>.
	/// </summary>
	/// <param name="skill">The skill to check. It doesn't have to be in <see cref="Skills"/>.</param>
	/// <returns>True if the skill can be used now.</returns>
	public bool CanUse(Skill skill)
	{
		return skill != null && GetUseFailure(skill) == null;
	}

	internal SkillUseFailure? GetUseFailure(Skill skill) => GetUseFailure(skill, true);

	internal SkillUseFailure? GetFollowupUseFailure(Skill skill) => GetUseFailure(skill, false);

	private SkillUseFailure? GetUseFailure(Skill skill, bool includeRequirementAndJuice)
	{
		if (includeRequirementAndJuice && !skill.MeetsRequirements(this))
			return SkillUseFailure.Requirement;
		if (skill.UseDisallowed)
			return SkillUseFailure.UseDisallowed;
		if (GetSkillCooldown(skill) > 0)
			return SkillUseFailure.Cooldown;
		int juice = skill.Cost(this);
		if (includeRequirementAndJuice && juice > 0 && CurrentJuice < juice)
			return SkillUseFailure.Juice;
		int hp = skill.HPCost(this);
		if (hp > 0 && CurrentHP <= hp)
			return SkillUseFailure.HP;
		if (this is PartyMember && BattleManager.Instance != null && BattleManager.Instance.Energy < skill.EnergyCost(this))
			return SkillUseFailure.Energy;
		return null;
	}

	private readonly Dictionary<Skill, int> SkillCooldowns = [];

	/// <summary>
	/// How many more turns this actor can't use <paramref name="skill"/>, counting the current turn. 0 means it isn't cooling down.
	/// </summary>
	/// <param name="skill">The skill to check.</param>
	public int GetSkillCooldown(Skill skill)
	{
		return skill != null && SkillCooldowns.TryGetValue(skill, out int turns) ? turns : 0;
	}

	/// <summary>
	/// How many more turns this actor can't use its skill <paramref name="skill"/>, counting the current turn. See <see cref="GetSkillCooldown(Skill)"/>.
	/// </summary>
	/// <param name="skill">The skill's key in <see cref="Skills"/>. An unknown key reads 0.</param>
	public int GetSkillCooldown(string skill)
	{
		return skill != null && Skills.TryGetValue(skill, out Skill value) ? GetSkillCooldown(value) : 0;
	}

	/// <summary>
	/// Sets how many more turns this actor can't use <paramref name="skill"/>, counting the current turn.
	/// </summary>
	/// <remarks>
	/// For example, 2 set during a turn blocks the skill for the rest of that turn and the whole next one. 0 makes it usable again.
	/// </remarks>
	/// <param name="skill">The skill to set.</param>
	/// <param name="turns">The number of turns, at least 0.</param>
	public void SetSkillCooldown(Skill skill, int turns)
	{
		if (skill == null)
			return;
		if (turns > 0)
			SkillCooldowns[skill] = turns;
		else
			SkillCooldowns.Remove(skill);
	}

	/// <summary>
	/// Sets how many more turns this actor can't use its skill <paramref name="skill"/>, counting the current turn. See <see cref="SetSkillCooldown(Skill, int)"/>.
	/// </summary>
	/// <param name="skill">The skill's key in <see cref="Skills"/>. An unknown key is ignored, with an error.</param>
	/// <param name="turns">The number of turns, at least 0.</param>
	public void SetSkillCooldown(string skill, int turns)
	{
		if (skill != null && Skills.TryGetValue(skill, out Skill value))
		{
			SetSkillCooldown(value, turns);
			return;
		}
		GD.PrintErr($"{Name} has no skill \"{skill}\" to set a cooldown on.");
	}

	internal void StartSkillCooldown(Skill skill)
	{
		if (skill.Cooldown > 0)
			SkillCooldowns[skill] = skill.Cooldown + 1;
	}

	internal void TickSkillCooldowns()
	{
		foreach (Skill skill in SkillCooldowns.Keys.ToList())
		{
			if (--SkillCooldowns[skill] <= 0)
				SkillCooldowns.Remove(skill);
		}
	}

	internal void ClearSkillCooldowns()
	{
		SkillCooldowns.Clear();
	}

	/// <summary>
	/// How many of this actor's <see cref="StatModifier"/>s are in the given category.
	/// </summary>
	/// <param name="category">The category name, ignoring case.</param>
	public int CountStatModifiersInCategory(string category)
	{
		return StatModifiers.Values.Count(x => x.HasCategory(category));
	}

	/// <summary>
	/// The ids of this actor's <see cref="StatModifier"/>s in the given category, oldest first.
	/// </summary>
	/// <param name="category">The category name, ignoring case.</param>
	public IReadOnlyList<string> GetStatModifiersInCategory(string category)
	{
		return StatModifiers.Where(x => x.Value.HasCategory(category))
			.OrderBy(x => x.Value.AddSequence)
			.Select(x => x.Key)
			.ToList();
	}

	/// <summary>
	/// Removes up to <paramref name="count"/> of this actor's <see cref="StatModifier"/>s in the given category, oldest first.
	/// </summary>
	/// <remarks>
	/// Modifiers that bypass removal are skipped without using up the count, unless <paramref name="force"/> is true. A count of 0 or less removes nothing.
	/// </remarks>
	/// <param name="category">The category name, ignoring case.</param>
	/// <param name="count">The most modifiers to remove.</param>
	/// <param name="force">If true, also removes modifiers that bypass removal. See <see cref="StatModifier.WithBypassRecoverAllRemoval"/>.</param>
	public void RemoveStatModifiersInCategory(string category, int count, bool force = false)
	{
		if (count <= 0)
			return;
		List<string> removing = GetStatModifiersInCategory(category)
			.Where(x => force || !StatModifiers[x].BypassesRecoverAllRemoval)
			.Take(count)
			.ToList();
		foreach (string id in removing)
			RemoveStatModifierInternal(id, StatModifierRemovalReason.Manual, force);
	}

	/// <summary>
	/// Fully restores this actor like RPG Maker's Recover All: modifiers are cleared without blocking their re-application,
	/// the emotion returns to neutral (unless an emotion lock keeps the current emotion, since <see cref="SetEmotion"/> fails silently while locked) and HP and Juice are restored, reviving a toast actor.
	/// Modifiers marked <see cref="StatModifier.WithBypassRecoverAllRemoval"/> stay.
	/// </summary>
	public void RecoverAll()
	{
		ClearStatModifiersOnRecoverAll();
		SetEmotionUnrecorded("neutral");
		if (IsToast)
			Revive(CurrentStats.MaxHP);
		else
			CurrentHP = CurrentStats.MaxHP;
		CurrentJuice = CurrentStats.MaxJuice;
	}

	internal void ClearStatModifiersOnRecoverAll()
	{
		foreach (KeyValuePair<string, StatModifier> entry in StatModifiers.ToList())
		{
			if (!entry.Value.BypassesRecoverAllRemoval)
				RemoveStatModifierInternal(entry.Key, StatModifierRemovalReason.Cleared, true, false);
		}
	}

	internal void ClearStatModifiersOnDeath()
	{
		foreach (KeyValuePair<string, StatModifier> entry in StatModifiers.ToList())
		{
			if (!entry.Value.BypassesDeathRemoval)
				RemoveStatModifierInternal(entry.Key, StatModifierRemovalReason.Cleared, true, false);
		}
	}

	private void RemoveStatModifierInternal(string modifier, StatModifierRemovalReason reason, bool force = false, bool record = true)
	{
		if (modifier == null || !StatModifiers.TryGetValue(modifier, out StatModifier mod))
			return;
		// modifiers that bypass Recover All removal only get removed via force or their own turn counter
		if (!force && mod.BypassesRecoverAllRemoval && reason != StatModifierRemovalReason.Expired)
		{
			GD.Print("Removal of modifier " + modifier + " on " + Name + " blocked by BypassesRecoverAllRemoval (" + reason + ")");
			return;
		}
		StatModifiers.Remove(modifier);
		if (record && !mod.ReapplyAllowed)
			RemovedSinceResultClear.Add(ResultKey(modifier, (mod as TierStatModifier)?.CurrentTier ?? 0));
		BattleManager.Instance.RunAsReaction(() => mod.OnRemove(this));
		GD.Print("Removed modifier " + modifier + " from " + Name + " (" + reason + ")");
		ClampHealthJuiceToMax();
		BattleManager.Instance.RunAsReaction(() => OnStatModifierRemoved?.Invoke(this, new StatModifierRemovedEventArgs(modifier, mod, reason)));
	}

	// RPGMaker keeps track of removed states whenever they expire
	// if any state is in this list before the actor is cleared and tries to be reapplied, it will be refused
	private readonly HashSet<string> RemovedSinceResultClear = [];

	/// <summary>
	/// Clears which modifiers and emotions were removed from this actor, mirroring RPG Maker's <c>clearResult</c>.<br/>
	/// Until this runs, a removed modifier can't be added back, and some lost emotions can't either, unless "Allow State Reapply on Removal" is enabled.
	/// The battle calls this when an action applies to or from this actor, after its action ends and at the end of the turn.
	/// </summary>
	public void ClearResult()
	{
		RemovedSinceResultClear.Clear();
	}

	internal void CopyBattlerState(Actor original)
	{
		RemovedSinceResultClear.UnionWith(original.RemovedSinceResultClear);
		DamageTakenThisAction = original.DamageTakenThisAction;
		IsEmotionLocked = original.IsEmotionLocked;
		LockedAdvantageEmotion = original.LockedAdvantageEmotion;
		foreach (KeyValuePair<Skill, int> entry in original.SkillCooldowns)
			SkillCooldowns[entry.Key] = entry.Value;
	}

	private static string ResultKey(string modifier, int tier)
	{
		return tier > 0 ? modifier + "#" + tier : modifier;
	}
	
	private bool IsReapplyBlocked(string modifier, int tier = 0)
	{
		if (BattleManager.Instance == null || SettingsMenuManager.Instance.AllowStateReapplyOnRemoval)
			return false;
		if (!RemovedSinceResultClear.Contains(ResultKey(modifier, tier)))
			return false;
		GD.Print("Blocked re-add of modifier " + ResultKey(modifier, tier) + " on " + Name + " (removed since the last result clear)");
		return true;
	}

	// in vanilla, an action-end state applied or refreshed by its holder's own action skips its
	// first tick, so the state doesn't expire by its own action ending
	private void GrantFreeActionTick(StatModifier mod)
	{
		if (mod.TicksOnActionEnd && BattleManager.ResolveKeptBattler(BattleManager.Instance?.GetCurrentCommand()?.Actor) == this)
			mod.SkipNextActionTick = true;
	}

	private void NotifyModifierAdded(string modifier, StatModifier mod)
	{
		if (CurrentEmotion.Id != "neutral" && mod.BlocksEmotion(CurrentEmotion))
			SetEmotionForced("neutral");
		ClampHealthJuiceToMax();
		BattleManager.Instance.RunAsReaction(() => OnStatModifierAdded?.Invoke(this, new StatModifierEventArgs(modifier, mod)));
	}

	private static bool CombineEnabled => BattleManager.Instance != null && BattleManager.Instance.CombinedBuffsDebuffs;

	// keeps current HP/Juice within the (possibly modified) max stats
	private void ClampHealthJuiceToMax()
	{
		if (IsToast || CurrentHP <= 0)
			return;
		Stats current = CurrentStats;
		// never let a max reduction kill the actor
		if (CurrentHP > current.MaxHP)
			CurrentHP = Math.Max(1, current.MaxHP);
		if (CurrentJuice > current.MaxJuice)
			CurrentJuice = Math.Max(0, current.MaxJuice);
	}

	private void ShowStatMessage(string message)
	{
		BattleLogManager.Instance.QueueMessage($"{Name.ToUpper()}'s {message}");
	}

	internal void ApplyPassiveStatModifiers()
	{
		foreach (Skill skill in Skills.Values.ToList())
		{
			foreach (string id in skill.PassiveStatModifiers)
			{
				if (StatModifiers.TryGetValue(id, out StatModifier held))
				{
					held.RefreshTurns();
					continue;
				}
				StatModifier mod = Database.CreateModifier(id);
				if (mod == null)
				{
					GD.PrintErr("Unknown stat modifier: " + id);
					continue;
				}
				if (mod is TierStatModifier tier)
					tier.WithTier(1);
				mod.AddSequence = ++LastModifierSequence;
				StatModifiers.Add(id, mod);
				BattleManager.Instance.RunAsReaction(() => mod.OnAdd(this));
				GD.Print("Added passive modifier " + id + " to " + Name);
				NotifyModifierAdded(id, mod);
			}
		}
	}

	internal void DecreaseStatTurnCounter()
	{
		foreach (var mod in StatModifiers.ToList())
		{
			if (mod.Value.TurnsLeft != -1 && !mod.Value.TicksOnActionEnd)
			{
				mod.Value.DecreaseTurns();
				if (mod.Value.TurnsLeft <= 0)
					RemoveStatModifierInternal(mod.Key, StatModifierRemovalReason.Expired);
			}
		}
	}
	
	internal void DecreaseActionStatTurnCounter()
	{
		foreach (var mod in StatModifiers.ToList())
		{
			if (mod.Value.TurnsLeft == -1 || !mod.Value.TicksOnActionEnd)
				continue;
			if (mod.Value.SkipNextActionTick)
			{
				mod.Value.SkipNextActionTick = false;
				continue;
			}
			mod.Value.DecreaseTurns();
			if (mod.Value.TurnsLeft <= 0)
				RemoveStatModifierInternal(mod.Key, StatModifierRemovalReason.Expired);
		}
	}

	/// <summary>
	/// Checks if this actor has a certain <see cref="StatModifier"/>.
	/// </summary>
	/// <param name="modifier">The name of the modifier to check for.</param>
	/// <returns>True if the actor has the given <paramref name="modifier"/>.</returns>
	public bool HasStatModifier(string modifier)
	{
		return StatModifiers.ContainsKey(modifier);
	}

	/// <summary>
	/// Returns the current tier of a stat modifier.
	/// </summary>
	/// <remarks>
	/// If the actor does not have the requested modifier or if it is not a tiered stat modifier, 0 is returned.
	/// </remarks>
	/// <param name="modifier">The modifier to get the current tier of.</param>
	/// <returns>The current tier if the actor has the tiered modifier, otherwise 0.</returns>
	public int GetStatModifierTier(string modifier)
	{
		if (!StatModifiers.TryGetValue(modifier, out StatModifier mod))
			return 0;
		if (mod is TierStatModifier tier)
			return tier.CurrentTier;
		return 0;
	}

	/// <summary>
	/// Returns the current turns left of a stat modifier.
	/// </summary>
	/// <remarks>
	/// If the actor does not have the requested modifier or is an infinite modifier, -1 is returned.
	/// </remarks>
	/// <param name="modifier">The modifier to get the turns left of.</param>
	/// <returns>The current number of turns left, otherwise -1.</returns>
	public int GetStatModifierTurnsLeft(string modifier)
	{
		if (!StatModifiers.TryGetValue(modifier, out StatModifier mod))
			return -1;
		return mod.TurnsLeft;
	}

	/// <summary>
	/// Whether this actor's emotion is currently locked. See <see cref="LockEmotion"/>.
	/// </summary>
	public bool IsEmotionLocked { get; private set; }

	private Emotion LockedAdvantageEmotion;

	/// <summary>
	/// Locks this actor's emotion, preventing any changes through <see cref="SetEmotion"/> until <see cref="UnlockEmotion"/> is called.<br/>
	/// Mainly used by bosses. <see cref="SetEmotionForced"/> bypasses the lock.
	/// </summary>
	/// <param name="advantageAsId">If set, damage calculations treat this actor as feeling that emotion instead of the displayed one.<br/>
	/// Vanilla lock bosses use their group's base emotion, so emotion advantage against them never scales past the base tier.</param>
	public void LockEmotion(string advantageAsId = null)
	{
		IsEmotionLocked = true;
		LockedAdvantageEmotion = null;
		if (advantageAsId == null)
			return;

		if (Database.TryGetEmotion(advantageAsId, out Emotion emotion))
			LockedAdvantageEmotion = emotion;
		else
			GD.PushWarning("Unknown emotion for emotion lock: " + advantageAsId);
	}

	/// <summary>
	/// Unlocks this actor's emotion. See <see cref="LockEmotion"/>.
	/// </summary>
	public void UnlockEmotion()
	{
		IsEmotionLocked = false;
		LockedAdvantageEmotion = null;
	}

	/// <summary>
	/// Returns the 1-based tier of the actor's current emotion within the given group,
	/// or 0 if the actor is not feeling an emotion of that group at all.
	/// </summary>
	/// <param name="groupId">The id of the emotion group to check.</param>
	public int GetEmotionTier(string groupId)
	{
		return CurrentEmotion.Group?.Id == groupId ? CurrentEmotion.Tier : 0;
	}

	/// <summary>
	/// Whether the actor is feeling an emotion of the given group, at or above the given tier.
	/// </summary>
	/// <param name="groupId">The id of the emotion group to check.</param>
	/// <param name="minTier">The minimum 1-based tier to check for. Values below 1 are treated as 1.</param>
	public bool IsFeeling(string groupId, int minTier = 1)
	{
		return GetEmotionTier(groupId) >= Math.Max(minTier, 1);
	}

	/// <summary>
	/// Whether the actor is feeling the exact emotion with the given id.
	/// </summary>
	/// <param name="emotionId">The id of the emotion to check.</param>
	public bool HasEmotion(string emotionId)
	{
		return CurrentEmotion.Id == emotionId;
	}

	/// <summary>
	/// Like <see cref="GetEmotionTier"/>, but reads <see cref="EffectiveEmotion"/>, respecting
	/// the advantage override of <see cref="LockEmotion"/>. Use for logic that must match damage calculations.
	/// </summary>
	/// <param name="groupId">The id of the emotion group to check.</param>
	public int GetEffectiveEmotionTier(string groupId)
	{
		return EffectiveEmotion.Group?.Id == groupId ? EffectiveEmotion.Tier : 0;
	}

	/// <summary>
	/// Like <see cref="IsFeeling"/>, but reads <see cref="EffectiveEmotion"/>, respecting
	/// the advantage override of <see cref="LockEmotion"/>. Use for logic that must match damage calculations.
	/// </summary>
	/// <param name="groupId">The id of the emotion group to check.</param>
	/// <param name="minTier">The minimum 1-based tier to check for. Values below 1 are treated as 1.</param>
	public bool IsEffectivelyFeeling(string groupId, int minTier = 1)
	{
		return GetEffectiveEmotionTier(groupId) >= Math.Max(minTier, 1);
	}

	/// <summary>
	/// The stat bonuses granted by the given emotion. Bosses whose locked emotions use alternate stats can override this.
	/// </summary>
	/// <param name="emotion">The emotion to get the stat bonuses of.</param>
	protected virtual StatBonus[] GetEmotionStatBonuses(Emotion emotion)
	{
		return emotion.StatBonuses;
	}

	/// <summary>
	/// Damages this actor by the given amount.
	/// </summary>
	/// <remarks>
	/// Negative values should not be used. See <see cref="Heal(int, bool)"/> for healing actors.
	/// </remarks>
	/// <param name="damage">The amount of damage to deal to this actor.</param>
	/// <param name="applyDebugScale">Whether Debug Damage should multiply the amount. Pass false for amounts that already went through the damage pipeline or were derived from its output.</param>
	/// <returns>The amount actually applied, including any potential Debug Damage scaling.</returns>
	public int Damage(int damage, bool applyDebugScale = true)
	{
		damage = DebugScaled(damage, applyDebugScale);
		if (damage <= 0)
			return 0;

		DamageTakenThisAction += damage;
		CurrentHP -= damage;
		if (CurrentHP < 0)
			CurrentHP = 0;

		if (this is PartyMember member && member.HasPlotArmor && CurrentHP == 0 && !member.HasUsedPlotArmor)
		{
			CurrentHP = 1;
			member.HasUsedPlotArmor = true;
			member.PlayAnimation("plotarmor", EmotionAsset.PlotArmor);
			AddStatModifier("PlotArmor");
			return damage;
		}

		BattleManager.Instance.RunAsReaction(() => OnDamaged?.Invoke(this, EventArgs.Empty));
		return damage;
	}

    /// <summary>
    /// Damages this actor's juice by the given amount.
    /// </summary>
	/// <remarks>
	/// Negative values should not be used. See <see cref="HealJuice(int, bool)"/> for healing juice.<br/>
	/// This will also not cause the actor to show the hurt animation.
	/// </remarks>
    /// <param name="damage">The amount of juice damage to deal to this actor.</param>
    /// <param name="applyDebugScale">Whether Debug Damage should multiply the amount. Pass false for amounts that already went through the damage pipeline or were derived from its output.</param>
    /// <returns>The amount actually applied, including any potential Debug Damage scaling.</returns>
    public int DamageJuice(int damage, bool applyDebugScale = true)
	{
		damage = DebugScaled(damage, applyDebugScale);
		if (damage <= 0)
			return 0;

		CurrentJuice -= damage;
		if (CurrentJuice < 0)
			CurrentJuice = 0;
		return damage;
    }

    /// <summary>
    /// Heals this actor by the given amount.
    /// </summary>
    /// <param name="health">The amount of health to heal.</param>
    /// <param name="applyDebugScale">Whether Debug Damage should multiply the amount. Pass false for amounts that already went through the damage pipeline or were derived from its output.</param>
    /// <returns>The amount actually applied, including any potential Debug Damage scaling.</returns>
    public int Heal(int health, bool applyDebugScale = true)
	{
		health = DebugScaled(health, applyDebugScale);
		CurrentHP += health;
		if (CurrentHP > CurrentStats.MaxHP)
			CurrentHP = CurrentStats.MaxHP;
		return health;
	}

	/// <summary>
	/// Heals this actor's juice by the given amount.
	/// </summary>
	/// <param name="juice">The amount of juice to heal.</param>
	/// <param name="applyDebugScale">Whether Debug Damage should multiply the amount. Pass false for amounts that already went through the damage pipeline or were derived from its output.</param>
	/// <returns>The amount actually applied, including any potential Debug Damage scaling.</returns>
	public int HealJuice(int juice, bool applyDebugScale = true)
	{
		juice = DebugScaled(juice, applyDebugScale);
		CurrentJuice += juice;
		if (CurrentJuice > CurrentStats.MaxJuice)
			CurrentJuice = CurrentStats.MaxJuice;
		return juice;
	}
	
	private static int DebugScaled(int amount, bool apply)
	{
		return apply && BattleManager.Instance != null ? BattleManager.Instance.DebugScale(amount) : amount;
	}

	/// <summary>
	/// Whether this actor's current HP is at or below the given fraction of its max HP.<br/>
	/// Mirrors RPG Maker's troop page check (<c>hpRate() * 100 &lt;= X</c>).
	/// Useful for having battle conditions scale when the actor's stats are adjusted.
	/// </summary>
	/// <param name="fraction">The fraction of max HP to compare against (0.25f = 25%).</param>
	public bool IsBelowHP(float fraction)
	{
		double percent = Math.Round(fraction * 100.0, 2);
		return (double)CurrentHP / CurrentStats.MaxHP * 100.0 <= percent;
	}

	/// <summary>
	/// Whether this actor's current Juice is at or below the given fraction of its max Juice.<br/>
	/// See <see cref="IsBelowHP"/> for the HP version.
	/// </summary>
	/// <param name="fraction">The fraction of max Juice to compare against (0.25f = 25%).</param>
	public bool IsBelowJuice(float fraction)
	{
		double percent = Math.Round(fraction * 100.0, 2);
		return (double)CurrentJuice / CurrentStats.MaxJuice * 100.0 <= percent;
	}

	/// <summary>
	/// Makes this actor appear visually hurt.
	/// </summary>
	/// <remarks>
	/// Does nothing while an animation override is active (such as Plot Armor).
	/// </remarks>
	/// <param name="hurt">Whether this actor should appear hurt.</param>
	public virtual void SetHurt(bool hurt)
	{
		if (CurrentAnimation != null || !GodotObject.IsInstanceValid(Sprite))
			return;

		Sprite.Animation = hurt ? "hurt" : EmotionAnimation(CurrentEmotion);
	}

	/// <summary>
	/// Overrides this actor's sprite animation, without changing their emotion or stats.<br/>
	/// The override stays active until cleared or overwritten, including any emotion changes and damage.<br/>
	/// See <see cref="ClearAnimation"/>.
	/// </summary>
	/// <param name="animationName">The animation to play. Must exist in the actor's SpriteFrames.</param>
	public virtual void PlayAnimation(string animationName)
	{
		if (!GodotObject.IsInstanceValid(Sprite) || Sprite.SpriteFrames == null || !Sprite.SpriteFrames.HasAnimation(animationName))
		{
			GD.PushWarning(Name + " cannot play unknown animation: " + animationName);
			return;
		}

		CurrentAnimation = animationName;
		Sprite.Animation = animationName;
		BattleManager.Instance.RunAsReaction(() => OnAnimationChanged?.Invoke(this, EventArgs.Empty));
	}

	/// <summary>
	/// Clears the active animation override (if any) and restores the sprite animation of the actor's current emotion.<br/>
	/// Includes non-emotion vanilla states such as Plot Armor, Victory, and Toast.
	/// </summary>
	public virtual void ClearAnimation()
	{
		if (CurrentAnimation == null)
			return;

		CurrentAnimation = null;
		if (GodotObject.IsInstanceValid(Sprite))
			Sprite.Animation = IsToast ? "toast" : EmotionAnimation(CurrentEmotion);
		BattleManager.Instance.RunAsReaction(() => OnAnimationChanged?.Invoke(this, EventArgs.Empty));
	}

	/// <summary>
	/// Moves the point this actor's <see cref="Sprite"/> rotates and scales around, without moving the sprite on screen.
	/// </summary>
	/// <remarks>
	/// Shifts <see cref="Node2D.Position"/> and <see cref="AnimatedSprite2D.Offset"/> together, for centered and uncentered sprites.
	/// <see cref="CenterPoint"/> and enemy info boxes don't change, including after <see cref="Enemy.RefreshInfoBox"/>.
	/// </remarks>
	/// <param name="pivot">The pivot in pixels from the top-left corner of the sprite's current frame,
	/// e.g. <c>new Vector2(width / 2f, height)</c> for the bottom center.</param>
	public void SetSpritePivot(Vector2 pivot)
	{
		if (!GodotObject.IsInstanceValid(Sprite) || Sprite.SpriteFrames == null)
		{
			GD.PushWarning(Name + " has no sprite to set a pivot on.");
			return;
		}

		Vector2 size = Sprite.SpriteFrames.GetFrameTexture(Sprite.Animation, Sprite.Frame)?.GetSize() ?? Vector2.Zero;
		Vector2 offset = Sprite.Centered ? size / 2f - pivot : -pivot;
		Sprite.Position += Sprite.Transform.BasisXform(Sprite.Offset - offset);
		Sprite.Offset = offset;
	}

	/// <summary>
	/// Makes this actor toast. Resets their emotion to neutral and plays the toast animation as an override.
	/// </summary>
	/// <remarks>
	/// Does not change the actor's HP on its own.
	/// </remarks>
	public virtual void SetToast()
	{
		if (IsToast)
			return;
		
		IsToast = true;
		// toast clears any emotion lock and resets the emotion (and its stats) to neutral
		UnlockEmotion();
		CurrentEmotion = Database.NeutralEmotion;
		if (this is PartyMember member)
			member.PlayAnimation("toast", EmotionAsset.Toast);
		else
			PlayAnimation("toast");
	}

	/// <summary>
	/// Revives a toast actor with the given HP, clearing the toast animation. Does nothing if the actor isn't toast.
	/// </summary>
	/// <remarks>
	/// Revival does not restore the emotion, emotion lock, or stat modifiers that were cleared by
	/// death. Revived enemies that fell off-screen are restored to their original position.
	/// </remarks>
	/// <param name="hp">The HP the actor revives with.</param>
	public void Revive(int hp)
	{
		if (!IsToast)
		{
			GD.PushWarning("Tried to revive an actor that was already alive!");
			return;
		}

		IsToast = false;
		ClearAnimation();
		CurrentHP = hp;
		BattleManager.Instance.RunAsReaction(() => OnRevived?.Invoke(this, EventArgs.Empty));
	}

	/// <summary>
	/// Checks if this actor can feel the given emotion.
	/// </summary>
	/// <remarks>
	/// By default, this only checks <see cref="IsImmuneToEmotion"/>. An override adds the actor's own rules and doesn't need to call <c>base</c>:
	/// <see cref="SetEmotion"/> and <see cref="BattleManager.MakeEmotion"/> also check <see cref="IsImmuneToEmotion"/>.
	/// </remarks>
	/// <param name="emotion">The emotion to check.</param>
	/// <returns>True if this actor can feel the given <paramref name="emotion"/>.</returns>
	public virtual bool IsEmotionValid(Emotion emotion) { return !IsImmuneToEmotion(emotion); }

	/// <summary>
	/// Whether something this actor wears or has applied blocks the given emotion.
	/// See <see cref="Equipment.WithEmotionImmunity"/> and <see cref="StatModifier.WithEmotionImmunity"/>.
	/// </summary>
	/// <param name="emotion">The emotion to check.</param>
	/// <returns>True if an equipped item or an active stat modifier blocks the emotion. Neutral is never blocked.</returns>
	public bool IsImmuneToEmotion(Emotion emotion)
	{
		if (emotion == null || emotion.Id == "neutral")
			return false;
		foreach (Equipment item in EquippedItems)
		{
			if (item != null && item.BlocksEmotion(emotion))
				return true;
		}
		return StatModifiers.Values.Any(x => x.BlocksEmotion(emotion));
	}

	/// <summary>
	/// The equipment this actor wears, checked by <see cref="IsImmuneToEmotion"/>. Empty by default.
	/// </summary>
	protected virtual IEnumerable<Equipment> EquippedItems => [];

	/// <summary>
	/// Checks if this actor can feel the emotion with the given id. See <see cref="IsEmotionValid(Emotion)"/>.
	/// </summary>
	/// <param name="id">The id of the emotion to check.</param>
	/// <returns>True if the emotion exists and this actor can feel it, false for unknown ids.</returns>
	public bool IsEmotionValid(string id)
	{
		return Database.TryGetEmotion(id, out Emotion emotion) && IsEmotionValid(emotion) && !IsImmuneToEmotion(emotion);
	}

    /// <summary>
    /// Sets this actor's emotion by id. Will fail and log a battle message if the actor cannot feel the given emotion,
    /// or if their emotion is locked.<br/>
    /// See <see cref="IsEmotionValid(Emotion)"/> and <see cref="LockEmotion"/>.
    /// </summary>
    /// <remarks>
    /// An emotion this actor lost since its action result was last cleared can fail the same way, unless "Allow State Reapply on Removal" is enabled:
    /// going back to neutral records the emotion it had, and so does becoming AFRAID from a HAPPY, SAD or ANGRY emotion, while gaining a HAPPY, SAD or ANGRY emotion forgets the recorded HAPPY, SAD, ANGRY and AFRAID emotions.
    /// See <see cref="ClearResult"/>.
    /// </remarks>
    /// <param name="id">The id of the emotion to set this actor to.</param>
    /// <param name="silent">If true, success/failure messages will not be logged.</param>
    /// <returns>Whether the emotion was applied.</returns>
    public bool SetEmotion(string id, bool silent = false)
	{
		return SetEmotionCore(id, silent, true);
	}

	internal bool SetEmotionUnrecorded(string id)
	{
		return SetEmotionCore(id, true, false);
	}

	private bool SetEmotionCore(string id, bool silent, bool record)
	{
		if (!Database.TryGetEmotion(id, out Emotion emotion))
		{
			GD.PrintErr("Unknown emotion: " + id);
			return false;
		}

		if (!IsEmotionLocked && IsEmotionValid(emotion) && !IsImmuneToEmotion(emotion) && !IsReapplyBlocked(EmotionResultKey(emotion.Id)))
		{
			Emotion previous = CurrentEmotion;
			CurrentEmotion = emotion;
			RecordEmotionChange(previous, emotion, record);
			// vanilla refreshes the battler after an emotion state is added, which clamps HP/JUICE to the new maximums
			ClampHealthJuiceToMax();
			if (!silent)
			{
				BattleLogManager.Instance.QueueMessage(emotion.FeelsMessage(Name));
			}

			BattleManager.Instance.RunAsReaction(() => OnEmotionChanged?.Invoke(this, EventArgs.Empty));
			// only update the sprite if no animation override is active
			if (CurrentAnimation == null && GodotObject.IsInstanceValid(Sprite)) {
				Sprite.Animation = EmotionAnimation(emotion);
			}
			NotifyEmotionTransform(previous, emotion);
			return true;
		}

		if (!silent)
		{
			if (emotion.Group?.MaxTierMessage != null)
				BattleLogManager.Instance.QueueMessage(null, this, emotion.Group.MaxTierMessage);
			else
				BattleLogManager.Instance.QueueMessage(Name.ToUpper() + " cannot be " + emotion.DisplayName + "!");
		}
		return false;
	}

	/// <summary>
	/// Silently forces this actor to feel an emotion, bypassing emotion locks and validity checks. Mainly used for boss phase changes. Should be used sparingly.
	/// </summary>
	/// <remarks>
	/// It is never refused by the re-apply rule, and it doesn't record the emotion it replaces, see <see cref="ClearResult"/>.
	/// </remarks>
	/// <param name="id">The id of the emotion to force this actor to feel.</param>
	public void SetEmotionForced(string id)
	{
		if (!Database.TryGetEmotion(id, out Emotion emotion))
		{
			GD.PrintErr("Unknown emotion: " + id);
			return;
		}

		Emotion previous = CurrentEmotion;
		CurrentEmotion = emotion;
		RecordEmotionChange(previous, emotion, false);
		ClampHealthJuiceToMax();
		BattleManager.Instance.RunAsReaction(() => OnEmotionChanged?.Invoke(this, EventArgs.Empty));
		if (CurrentAnimation == null && GodotObject.IsInstanceValid(Sprite))
		{
			Sprite.Animation = EmotionAnimation(emotion);
		}
		NotifyEmotionTransform(previous, emotion);
	}
	
	internal string EmotionAnimation(Emotion emotion)
	{
		if (this is not Enemy)
			return emotion.AnimationName;
		return EnemyEmotionAnimation(Sprite?.SpriteFrames, emotion);
	}

	internal static string EnemyEmotionAnimation(SpriteFrames frames, Emotion emotion)
	{
		if (frames == null || frames.HasAnimation(emotion.AnimationName))
			return emotion.AnimationName;
		if (emotion.Group != null && Database.TryGetEmotionByGroupTier(emotion.Group.Id, 1, out Emotion groupBase)
		    && frames.HasAnimation(groupBase.AnimationName))
			return groupBase.AnimationName;
		return "neutral";
	}

	private const string EmotionKeyPrefix = "emotion:";

	private static string EmotionResultKey(string emotionId)
	{
		return EmotionKeyPrefix + emotionId;
	}

	private static bool IsEmotionalGroup(Emotion emotion)
	{
		return emotion.GroupId is "happy" or "sad" or "angry";
	}

	private static bool IsEmotionalRecord(string key)
	{
		return key.StartsWith(EmotionKeyPrefix, StringComparison.Ordinal)
			&& Database.TryGetEmotion(key[EmotionKeyPrefix.Length..], out Emotion emotion)
			&& (IsEmotionalGroup(emotion) || emotion.Id == "afraid");
	}

	private void RecordEmotionChange(Emotion previous, Emotion next, bool record)
	{
		if (previous == next)
			return;
		if (IsEmotionalGroup(next))
		{
			RemovedSinceResultClear.RemoveWhere(IsEmotionalRecord);
			return;
		}
		if (!record || previous.Id == "neutral" || previous.ReapplyAllowed)
			return;
		if (next.Id == "neutral" || (next.Id == "afraid" && IsEmotionalGroup(previous)))
			RemovedSinceResultClear.Add(EmotionResultKey(previous.Id));
	}

	private void NotifyEmotionTransform(Emotion previous, Emotion current)
	{
		if (this is Enemy enemy)
			BattleManager.Instance.OnEnemyEmotionChanged(enemy, previous, current);
	}
	
	/// <summary>
	/// Called once when this actor enters a battle. For the actors a battle starts with, that is before any start-of-battle hook. Use it to set up extra sprites or nodes.
	/// </summary>
	/// <remarks>
	/// The battle waits for it for the party members and enemies a battle, or a Boss Rush stage, starts with.
	/// For an actor that joins later, such as a summon or an enemy made by <see cref="BattleManager.TransformEnemy"/> or
	/// <see cref="BattleManager.ReplaceEnemy"/>, it runs right away and the battle doesn't wait for it.
	/// </remarks>
	public virtual async Task OnSpawned() { await Task.CompletedTask; }
	/// <summary>
	/// Called at the very start of the battle.
	/// </summary>
	public virtual async Task OnStartOfBattle() { await Task.CompletedTask; }
	/// <summary>
	/// Called when the battle is over, but before the victory screen.
	/// </summary>
	/// <param name="victory">Whether the battle was won by the player.</param>
	public virtual async Task OnEndOfBattle(bool victory) { await Task.CompletedTask; }

	/// <summary>
	/// The HP damage this actor has taken during the current action. It goes back to 0 when the next action starts.
	/// Damage taken between actions, such as at the start or end of a turn or from battle conditions, keeps adding up until then.
	/// </summary>
	public int DamageTakenThisAction { get; internal set; }

	/// <summary>
	/// Called once after an action in which this actor took HP damage, before the battle conditions are checked.
	/// Unlike <see cref="OnDamaged"/>, the battle waits for it, so it can play animations or transform the actor.
	/// </summary>
	/// <remarks>
	/// <paramref name="total"/> covers the whole action, so every hit of a multi-hit skill counts. It isn't called for an actor that is toast by then,
	/// or for damage taken outside an action, such as at the start or end of a turn. An enemy brought to 0 HP is still called, because enemy
	/// deaths are resolved after the battle conditions, unless a forced command defeated it.
	/// </remarks>
	/// <param name="total">The HP damage this actor took during the action. See <see cref="DamageTakenThisAction"/>.</param>
	public virtual async Task OnDamagedAfterAction(int total) { await Task.CompletedTask; }
}
