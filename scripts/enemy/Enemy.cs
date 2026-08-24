using System;
using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Modifier;

namespace OmoriSandbox.Actors;

// TODO: Remaining enemies
// Omori
// Mari

/// <summary>
/// An <see cref="Actor"/> that is considered an enemy. Can be inherited to create a new enemy.
/// </summary>
public abstract class Enemy : Actor
{
	internal void Init(AnimatedSprite2D sprite, string initialState, bool fallsOffScreen, bool grayscaleOnDefeat, int layer, Stats adjustedStats = default)
	{
		SpriteFrames animation = Animation;
		if (animation == null)
		{
			GD.PrintErr("Failed to load Sprite animations for Enemy: " + Name);
			return;
		}
		// presets may start an enemy toast
		bool startToast = initialState == "toast";
		if (startToast)
			initialState = "neutral";
		// init animation
		Sprite = sprite;
		Sprite.SpriteFrames = animation;

		if (!SetEmotion(initialState, true))
		{
			GD.PushWarning($"Invalid emotion '{initialState}' for Enemy {Name}, defaulting to neutral.");
			SetEmotion("neutral", true);
		}
		Sprite.Play();
		AdjustedStats = adjustedStats;
		SetBaseStats(Stats + AdjustedStats);
		CurrentHP = BaseStats.HP;
		CurrentJuice = BaseStats.Juice;
		if (startToast)
			SetToast();

		FallsOffScreen = fallsOffScreen;
		GrayscaleOnDefeat = grayscaleOnDefeat;
		Layer = layer;

		foreach (string s in EquippedSkills)
		{
			if (Database.TryGetSkill(s, out var skill))
			{
				if (!Skills.TryAdd(s, skill)) 
					GD.PushWarning($"Actor {Name} already has skill {s} equipped! Skipping...");
				continue;
			}
			GD.PrintErr("Unknown skill: " + s);
		}
	}

	/// <summary>
	/// The stat adjustments applied to this enemy by the current preset.
	/// </summary>
	public Stats AdjustedStats { get; private set; }

	/// <summary>
	/// Position of this enemy's info box center relative to the actor's "feet" (the bottom of the sprite).
	/// Defaults to <c>Vector2.Zero</c> if not specified.
	/// </summary>
	public virtual Vector2 InfoBoxOffset => Vector2.Zero;

	/// <summary>
	/// Whether this enemy's info box finger renders above the box instead of below it.
	/// Behaves the same as vanilla's StatusCursorPosition note tag.
	/// </summary>
	public virtual bool InfoBoxCursorAboveBox => false;

	// the enemy's declared stats, used by the editor stat display
	internal Stats DeclaredStats => Stats;

	/// <summary>
	/// Sets the opacity of the enemy sprite. Can optionally change over a set duration.
	/// </summary>
	/// <param name="opacity">The opacity of the sprite, from 0 to 1.</param>
	/// <param name="duration">The duration of the change, in seconds.</param>
	public void SetOpacity(float opacity, float duration = 0f)
	{
		opacity = Math.Clamp(opacity, 0f, 1f);
		if (duration == 0f)
		{
			Sprite.Modulate = new Color(Sprite.Modulate, opacity);
		}
		else
		{
			Tween tween = BattleManager.Instance.GetTree().CreateTween();
			tween.TweenProperty(Sprite, "modulate:a", opacity, duration);
		}
	}

	/// <summary>
	/// Selects a target. Mainly used in <see cref="ProcessAI"/> for single-target skills. Can be overriden for custom targeting behavior.
	/// </summary>
	/// <remarks>This only includes alive party members by default.</remarks>
	/// <returns>The <see cref="PartyMember"/> that will be targeted.</returns>
	protected virtual PartyMember SelectTarget()
	{
		if (HasStatModifier("Charm"))
			return (StatModifiers["Charm"] as CharmStatModifier).CharmedBy;
		List<PartyMemberComponent> members = BattleManager.Instance.GetAlivePartyMembers();
		List<PartyMemberComponent> taunting = members.FindAll(x => x.Actor.HasStatModifier("Taunt"));
		if (taunting.Count == 0)
		{
			// if nobody is taunting, pick a random target
			return members[GameManager.Instance.Random.RandiRange(0, members.Count - 1)].Actor;
		}
		return taunting[GameManager.Instance.Random.RandiRange(0, taunting.Count - 1)].Actor;
	}

	/// <summary>
	/// Selects X random targets. Can include duplicates. Mainly used in <see cref="ProcessAI"/> for multi-target skills. Can be overriden for custom targeting behavior.
	/// </summary>
	/// <param name="amount">The amount of targets to select.</param>
	/// <remarks>This only includes alive party members by default. </remarks>
	/// <returns>The <see cref="PartyMember"/>s that will be targeted.</returns>
	protected virtual IReadOnlyList<PartyMember> SelectTargets(int amount)
	{
		List<PartyMemberComponent> targets = BattleManager.Instance.GetAlivePartyMembers();
		List<PartyMember> result = [];
		for (int i = 0; i < amount; i++)
			result.Add(targets[GameManager.Instance.Random.RandiRange(0, targets.Count - 1)].Actor);
		return result;
	}

	/// <summary>
	/// Selects all targets. Mainly used in <see cref="ProcessAI"/> for multi-target skills. Can be overriden for custom targeting behavior.
	/// </summary>
	/// <remarks>This only includes alive party members by default.</remarks>
	/// <returns>The <see cref="PartyMember"/>s that will be targeted.</returns>
	protected virtual IReadOnlyList<PartyMember> SelectAllTargets()
	{
		return BattleManager.Instance.GetAlivePartyMembers().Select(x => x.Actor).ToList();
	}

	/// <summary>
	/// Selects an alive enemy target. Mainly used in <see cref="ProcessAI"/> for single-target skills that target an enemy. Can be overriden for custom targeting behavior.<br/>
	/// <remarks>For targeting party members, use <see cref="SelectTarget"/>.</remarks>
	/// </summary>
	/// <returns>The <see cref="Enemy"/> that will be targeted.</returns>
	protected virtual Enemy SelectEnemy()
	{
		return BattleManager.Instance.GetRandomAliveEnemy();
	}

	/// <summary>
	/// Selects all alive enemy targets. Mainly used in <see cref="ProcessAI"/> for multi-target skills that target all enemies. Can be overriden for custom targeting behavior.
	/// </summary>
	/// <remarks>For targeting all party members, use <see cref="SelectAllTargets"/>.</remarks>
	/// <returns></returns>
	protected virtual IReadOnlyList<Enemy> SelectAllEnemies()
	{
		return BattleManager.Instance.GetAllAliveEnemies();
	}

	/// <summary>
	/// Rolls a number between 1 and 100 (inclusive). Mainly a helper function for calculating skill chances in <see cref="ProcessAI"/>
	/// </summary>
	/// <returns></returns>
	protected int Roll()
	{
		return GameManager.Instance.Random.RandiRange(1, 100);
	}

	/// <summary>
	/// The enemy's stats.
	/// </summary>
	protected abstract Stats Stats { get; }
	/// <summary>
	/// A list of skills that this enemy has equipped.
	/// </summary>
	protected abstract string[] EquippedSkills { get; }
	public abstract SpriteFrames Animation { get; }
	/// <summary>
	/// Called right before an enemy takes their turn.
	/// </summary>
	/// <returns>The <see cref="BattleCommand"/> that the enemy will perform on their turn.</returns>
	public abstract BattleCommand ProcessAI();
	/// <summary>
	/// Whether the enemy falls off the screen when killed.
	/// </summary>
	/// <remarks>
	/// Setting this value directly should be avoided as the player can set this manually in the preset settings.
	/// </remarks>
	public bool FallsOffScreen = true;

	/// <summary>
	/// Whether the enemy triggers a grayscale effect on defeat.
	/// </summary>
	/// <remarks>
	/// Setting this value directly should be avoided as the player can set this manually in the preset settings.
	/// </remarks>
	public bool GrayscaleOnDefeat = false;
	/// <summary>
	/// The layer this enemy is on
	/// </summary>
	public int Layer { get; protected set; } = 0;
	/// <summary>
	/// Whether this enemy has a multi-target skill equipped.
	/// </summary>
	public bool HasMultiTargetSkill {
		get
		{
			return Skills.Values.Any(x => x.Target is SkillTarget.AllAllies or SkillTarget.AllDeadAllies
				or SkillTarget.AllEnemies or SkillTarget.XRandomEnemies);
		}
	}
	/// <summary>
	/// Whether this enemy has a skill that hits several party members at once.
	/// Used by OBSERVE to pick a valid multi-target prediction.
	/// </summary>
	internal bool HasMultiTargetPartySkill =>
		Skills.Values.Any(x => x.Target is SkillTarget.AllEnemies or SkillTarget.XRandomEnemies);
	/// <summary>
	/// Called after each action finishes. Mainly used for boss events.
	/// </summary>
	public virtual async Task ProcessBattleConditions() { await Task.CompletedTask; }
	/// <summary>
	/// Called at the very start of the turn.
	/// </summary>
	public virtual async Task ProcessStartOfTurn() { await Task.CompletedTask; }
	/// <summary>
	/// Called after the player finishes selecting commands and before the first command executes.
	/// </summary>
	public virtual async Task ProcessStartOfCommands() { await Task.CompletedTask; }
	/// <summary>
	/// Called at the very end of the turn, but before it officially ends.
	/// </summary>
	public virtual async Task ProcessEndOfTurn() { await Task.CompletedTask; }
	/// <summary>
	/// Called when the enemy is defeated (HP reaches 0 and stays 0 after <see cref="ProcessBattleConditions"/>).
	/// </summary>
	public virtual async Task OnDefeat() { await Task.CompletedTask; }

	internal PartyMember ObserveTarget;
	internal bool ObserveMultiTarget;
	internal bool ObserveSetThisTurn;

	/// <summary>
	/// Whether this enemy is currently observing a target.
	/// </summary>
	/// <param name="target">The observed target, if any.</param>
	/// <returns>True if <paramref name="target"/> is not null.</returns>
	protected bool HasObserveTarget(out PartyMember target)
	{
		target = ObserveTarget;
		if (ObserveTarget == null) 
			return false;
		ObserveTarget = null;
		ObserveMultiTarget = false;
		return true;

	}

	/// <summary>
	/// Whether this enemy is currently observing everyone.
	/// </summary>
	/// <returns>True if the enemy is observing everyone.</returns>
	protected bool HasMultiTargetObserve()
	{
		if (!ObserveMultiTarget)
			return false;
		ObserveTarget = null;
		ObserveMultiTarget = false;
		return true;
	}
}
