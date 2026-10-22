using System;
using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Modifier;
using OmoriSandbox.Editor;

namespace OmoriSandbox.Actors;

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

		if (!SetEmotionUnrecorded(initialState))
		{
			GD.PushWarning($"Invalid emotion '{initialState}' for Enemy {Name}, defaulting to neutral.");
			SetEmotionUnrecorded("neutral");
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

	internal Enemy TransformedInto { get; set; }
	internal bool KeepsBattler { get; set; }

	/// <inheritdoc/>
	public sealed override EnemyComponent Component => (EnemyComponent)base.Component;

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

	/// <summary>
	/// Re-places this enemy's info box and refreshes its name and width, for both the regular and the "Show More Info" box.<br/>
	/// Call this after moving the enemy or changing <see cref="InfoBoxOffset"/>, <see cref="InfoBoxCursorAboveBox"/> or the name.
	/// </summary>
	/// <remarks>
	/// <see cref="Actor.CenterPoint"/> is recomputed from the enemy's node, then the box is placed at the sprite's "feet" plus
	/// <see cref="InfoBoxOffset"/>, with its finger side from <see cref="InfoBoxCursorAboveBox"/>. The name is also refreshed
	/// every time the box is shown.<br/>
	/// Move the enemy by moving its node (the parent of <see cref="Actor.Sprite"/>), not the sprite, as the node's position is
	/// what a revive restores. Does nothing if the enemy isn't spawned in a battle.
	/// </remarks>
	public void RefreshInfoBox()
	{
		Component?.RefreshInfoBox();
	}

	/// <summary>
	/// Whether this enemy blinks white while it is selected as a target. Defaults to true.
	/// Vanilla turns the blink off for a few fights, such as the fear bosses.
	/// </summary>
	public virtual bool BlinksWhenSelected => true;

	/// <summary>
	/// Position of this enemy's damage numbers relative to the actor's "feet" (the bottom of the sprite).
	/// Behaves the same as vanilla's DamageOffsetX and DamageOffsetY note tags. Defaults to <c>Vector2.Zero</c> if not specified.
	/// </summary>
	public virtual Vector2 DamageNumberOffset => Vector2.Zero;

	// the enemy's declared stats, used by the editor stat display
	internal Stats DeclaredStats => Stats;

	/// <summary>
	/// Sets the opacity of the enemy sprite. Can optionally change over a set duration.
	/// </summary>
	/// <param name="opacity">The opacity of the sprite, from 0 to 1.</param>
	/// <param name="duration">The duration of the change, in seconds.</param>
	public void SetOpacity(float opacity, float duration = 0f)
	{
		if (!GodotObject.IsInstanceValid(Sprite))
			return;
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

	internal void SetBlendColor(Color color)
	{
		if (GodotObject.IsInstanceValid(Sprite))
			Sprite.SetInstanceShaderParameter("blend_color", color);
	}

	/// <summary>
	/// Selects a target. Mainly used in <see cref="ProcessAI"/> for single-target skills. Can be overriden for custom targeting behavior.
	/// </summary>
	/// <remarks>
	/// This only includes alive party members by default. The member who charmed this enemy comes first, then a taunting party member.
	/// Taunts are ignored while this enemy has the <c>"ObserveIgnoreTaunt"</c> stat modifier, which OBSERVE usually gives the enemy it targets.
	/// A charmed enemy's single-target skill aimed at the party goes to the member who charmed it when the skill runs, whatever target it was given.
	/// </remarks>
	/// <returns>The <see cref="PartyMember"/> that will be targeted.</returns>
	protected virtual PartyMember SelectTarget()
	{
		if (HasStatModifier("Charm"))
			return (StatModifiers["Charm"] as CharmStatModifier).CharmedBy;
		List<PartyMember> members = BattleManager.Instance.GetAlivePartyMembers();
		List<PartyMember> taunting = HasStatModifier("ObserveIgnoreTaunt") ? [] : members.FindAll(x => x.HasStatModifier("Taunt"));
		if (taunting.Count == 0)
		{
			// if nobody is taunting, pick a random target
			return members[GameManager.Instance.Random.RandiRange(0, members.Count - 1)];
		}
		return taunting[GameManager.Instance.Random.RandiRange(0, taunting.Count - 1)];
	}

	/// <summary>
	/// Selects X random targets. Can include duplicates. Mainly used in <see cref="ProcessAI"/> for multi-target skills. Can be overriden for custom targeting behavior.
	/// </summary>
	/// <param name="amount">The amount of targets to select.</param>
	/// <remarks>This only includes alive party members by default. </remarks>
	/// <returns>The <see cref="PartyMember"/>s that will be targeted.</returns>
	protected virtual IReadOnlyList<PartyMember> SelectTargets(int amount)
	{
		List<PartyMember> targets = BattleManager.Instance.GetAlivePartyMembers();
		List<PartyMember> result = [];
		for (int i = 0; i < amount; i++)
			result.Add(targets[GameManager.Instance.Random.RandiRange(0, targets.Count - 1)]);
		return result;
	}

	/// <summary>
	/// Selects the living party member with the lowest value of the given stat.
	/// </summary>
	/// <remarks>Unlike <see cref="SelectTarget"/>, a charm isn't taken into account. Ties are broken like the base game: a random living member is picked first and only replaced by a strictly better one, so among tied members the first in party order is the most likely. A charmed enemy's single-target skill aimed at the party still goes to the member who charmed it when the skill runs.</remarks>
	/// <param name="stat">The stat to compare, read from <see cref="Actor.CurrentStats"/>.</param>
	/// <returns>The selected <see cref="PartyMember"/>, or null if nobody is alive.</returns>
	protected PartyMember SelectTargetWithLowest(StatType stat)
	{
		return SelectTargetWithLowest(member => member.CurrentStats.GetStat(stat));
	}

	/// <summary>
	/// Selects the living party member with the lowest <paramref name="value"/>, such as current HP.
	/// </summary>
	/// <remarks>Unlike <see cref="SelectTarget"/>, a charm isn't taken into account. Ties are broken like the base game: a random living member is picked first and only replaced by a strictly better one, so among tied members the first in party order is the most likely. A charmed enemy's single-target skill aimed at the party still goes to the member who charmed it when the skill runs.</remarks>
	/// <param name="value">Reads the value to compare from a party member. If null, a random living member is picked.</param>
	/// <returns>The selected <see cref="PartyMember"/>, or null if nobody is alive.</returns>
	protected PartyMember SelectTargetWithLowest(Func<PartyMember, float> value)
	{
		return SelectTargetBy(value ?? (_ => 0f), false);
	}

	/// <summary>
	/// Selects the living party member with the highest value of the given stat.
	/// </summary>
	/// <remarks>Unlike <see cref="SelectTarget"/>, a charm isn't taken into account. Ties are broken like the base game: a random living member is picked first and only replaced by a strictly better one, so among tied members the first in party order is the most likely. A charmed enemy's single-target skill aimed at the party still goes to the member who charmed it when the skill runs.</remarks>
	/// <param name="stat">The stat to compare, read from <see cref="Actor.CurrentStats"/>.</param>
	/// <returns>The selected <see cref="PartyMember"/>, or null if nobody is alive.</returns>
	protected PartyMember SelectTargetWithHighest(StatType stat)
	{
		return SelectTargetWithHighest(member => member.CurrentStats.GetStat(stat));
	}

	/// <summary>
	/// Selects the living party member with the highest <paramref name="value"/>, such as current HP.
	/// </summary>
	/// <remarks>Unlike <see cref="SelectTarget"/>, a charm isn't taken into account. Ties are broken like the base game: a random living member is picked first and only replaced by a strictly better one, so among tied members the first in party order is the most likely. A charmed enemy's single-target skill aimed at the party still goes to the member who charmed it when the skill runs.</remarks>
	/// <param name="value">Reads the value to compare from a party member. If null, a random living member is picked.</param>
	/// <returns>The selected <see cref="PartyMember"/>, or null if nobody is alive.</returns>
	protected PartyMember SelectTargetWithHighest(Func<PartyMember, float> value)
	{
		return SelectTargetBy(value ?? (_ => 0f), true);
	}
	
	/// <summary>
	/// Selects the a random living party member matching or above the given HP fraction/percentage.
	/// </summary>
	/// <param name="fraction">The HP fraction (percent) to select, from 0 to 1.</param>
	/// <returns>A random <see cref="PartyMember"/> at or above the given HP fraction.</returns>
	protected PartyMember SelectTargetAtOrAboveHP(float fraction)
	{
		List<PartyMember> valid = BattleManager.Instance.GetAlivePartyMembers().FindAll(x => x.CurrentHP / (float)x.CurrentStats.MaxHP >= fraction);
		return valid.Count == 0 ? null : valid[GameManager.Instance.Random.RandiRange(0, valid.Count - 1)];
	}

	/// <summary>
	/// Selects the enemy with the highest <paramref name="value"/>, such as SPEED, from <see cref="SelectAllEnemies"/>, which includes this enemy.
	/// </summary>
	/// <remarks>Ties are broken like <see cref="SelectTargetWithHighest(Func{PartyMember, float})"/>: a random enemy is picked first and only replaced by a strictly better one.</remarks>
	/// <param name="value">Reads the value to compare from an enemy. If null, a random enemy is picked.</param>
	/// <param name="filter">Only enemies it returns true for are considered. If null, every enemy is.</param>
	/// <returns>The selected <see cref="Enemy"/>, or null if no enemy is considered.</returns>
	protected Enemy SelectEnemyWithHighest(Func<Enemy, float> value, Func<Enemy, bool> filter = null)
	{
		return SelectBy(SelectAllEnemies().Where(filter ?? (_ => true)).ToList(), value ?? (_ => 0f), true);
	}

	/// <summary>
	/// Selects the enemy with the lowest <paramref name="value"/>, such as the share of HP left, from <see cref="SelectAllEnemies"/>, which includes this enemy.
	/// </summary>
	/// <remarks>Ties are broken like <see cref="SelectTargetWithLowest(Func{PartyMember, float})"/>: a random enemy is picked first and only replaced by a strictly better one.</remarks>
	/// <param name="value">Reads the value to compare from an enemy. If null, a random enemy is picked.</param>
	/// <param name="filter">Only enemies it returns true for are considered. If null, every enemy is.</param>
	/// <returns>The selected <see cref="Enemy"/>, or null if no enemy is considered.</returns>
	protected Enemy SelectEnemyWithLowest(Func<Enemy, float> value, Func<Enemy, bool> filter = null)
	{
		return SelectBy(SelectAllEnemies().Where(filter ?? (_ => true)).ToList(), value ?? (_ => 0f), false);
	}

	private static PartyMember SelectTargetBy(Func<PartyMember, float> value, bool highest)
	{
		return SelectBy(BattleManager.Instance.GetAlivePartyMembers(), value, highest);
	}

	private static T SelectBy<T>(IReadOnlyList<T> members, Func<T, float> value, bool highest) where T : Actor
	{
		if (members.Count == 0)
			return null;
		T target = members[GameManager.Instance.Random.RandiRange(0, members.Count - 1)];
		foreach (T member in members)
		{
			if (highest ? value(member) > value(target) : value(member) < value(target))
				target = member;
		}
		return target;
	}

	/// <summary>
	/// Selects all targets. Mainly used in <see cref="ProcessAI"/> for multi-target skills. Can be overriden for custom targeting behavior.
	/// </summary>
	/// <remarks>This only includes alive party members by default.</remarks>
	/// <returns>The <see cref="PartyMember"/>s that will be targeted.</returns>
	protected virtual IReadOnlyList<PartyMember> SelectAllTargets()
	{
		return BattleManager.Instance.GetAlivePartyMembers();
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
	/// <remarks>
	/// Return null to take no action. That action is skipped, and once the enemy has no actions left the turn goes on with the next actor.<br/>
	/// An enemy with more than one action this turn (see <see cref="Actor.ActionTimes"/>) is asked again for each action, and returning null uses up that action. It's also asked again right after it uses an instant skill that doesn't use up an action (see <see cref="Skill.WithInstantCast"/>).
	/// Unless something changed <see cref="Actor.RemainingActions"/> this turn, <c>ActionTimes - RemainingActions</c> is the number of actions it has already taken this turn.
	/// </remarks>
	/// <returns>The <see cref="BattleCommand"/> that the enemy will perform on their turn, or null for no action.</returns>
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

	internal bool PreventBattlebackChanges { get; set; }
	internal bool PreventBGMChanges { get; set; }

	/// <summary>
	/// Whether this enemy may change the battleback.
	/// </summary>
	/// <remarks>
	/// False if the preset locks the BGM and battleback, or if this enemy's preset entry has "Prevent Battleback Changes" on.
	/// The battleback methods on this enemy do nothing while this is false.
	/// </remarks>
	public bool CanChangeBattleback => !PreventBattlebackChanges && !GameManager.Instance.BattlebackLocked;

	/// <summary>
	/// Whether this enemy may change the BGM.
	/// </summary>
	/// <remarks>
	/// False if the preset locks the BGM and battleback, or if this enemy's preset entry has "Prevent BGM Changes" on.
	/// The BGM methods on this enemy do nothing while this is false.
	/// </remarks>
	public bool CanChangeBGM => !PreventBGMChanges && !AudioManager.Instance.BGMLocked;

	/// <summary>
	/// Changes the battleback image.
	/// </summary>
	/// <remarks>
	/// Only the battleback itself changes. Any battleback layers stay as they are. Use <see cref="ClearBattlebackLayers"/> to remove them.
	/// Does nothing while <see cref="CanChangeBattleback"/> is false.
	/// </remarks>
	/// <param name="name">The name of the battleback to show.</param>
	/// <returns>True if the battleback was changed, false if this enemy can't change it.</returns>
	public bool ChangeBattleback(string name)
	{
		if (!CanChangeBattleback)
			return false;
		GameManager.Instance.ForceSetBattleback(name);
		return true;
	}

	/// <summary>
	/// Adds a battleback layer, which fades in over a third of a second.
	/// </summary>
	/// <remarks>
	/// Layers are numbered images drawn over the battleback, with higher numbers drawn on top. Layers start at 3, and
	/// numbers below 3 are ignored. Adding to a number that is already in use does nothing.
	/// Does nothing while <see cref="CanChangeBattleback"/> is false.
	/// </remarks>
	/// <param name="layer">The layer number, 3 or higher.</param>
	/// <param name="name">The name of the battleback to show on the layer.</param>
	/// <returns>True if the layer was added, false if this enemy can't change the battleback.</returns>
	public bool AddBattlebackLayer(int layer, string name)
	{
		if (!CanChangeBattleback)
			return false;
		GameManager.Instance.BattlebackLayers.Add(layer, name);
		return true;
	}

	/// <summary>
	/// Removes a battleback layer, fading it out over a third of a second.
	/// </summary>
	/// <remarks>
	/// Once the layer is gone, every higher layer moves down one number. Does nothing for a number with no layer.
	/// Does nothing while <see cref="CanChangeBattleback"/> is false.
	/// </remarks>
	/// <param name="layer">The layer number.</param>
	/// <returns>True if the layer was added, false if this enemy can't change the battleback.</returns>
	public bool RemoveBattlebackLayer(int layer)
	{
		if (!CanChangeBattleback)
			return false;
		GameManager.Instance.BattlebackLayers.Remove(layer);
		return true;
	}

	/// <summary>
	/// Swaps the image of a battleback layer immediately.
	/// </summary>
	/// <remarks>
	/// Does nothing for a number with no layer. Also does nothing while <see cref="CanChangeBattleback"/> is false.
	/// </remarks>
	/// <param name="layer">The layer number.</param>
	/// <param name="name">The name of the battleback to show on the layer.</param>
	/// <returns>True if the layer was changed, false if this enemy can't change the battleback.</returns>
	public bool ChangeBattlebackLayer(int layer, string name)
	{
		if (!CanChangeBattleback)
			return false;
		GameManager.Instance.BattlebackLayers.Change(layer, name);
		return true;
	}

	/// <summary>
	/// Fades a battleback layer to the given opacity.
	/// </summary>
	/// <remarks>
	/// Layer numbers 0 and 1 fade the battleback itself. Does nothing for a number with no layer, such as 2.
	/// Does nothing while <see cref="CanChangeBattleback"/> is false.
	/// </remarks>
	/// <param name="layer">The layer number.</param>
	/// <param name="opacity">The opacity to fade to, from 0 to 1.</param>
	/// <param name="frames">How long the fade takes, in frames at 60 per second. 0 or less applies it at once.</param>
	/// <returns>True if the layer was faded, false if this enemy can't change the battleback.</returns>
	public bool FadeBattlebackLayer(int layer, float opacity, int frames)
	{
		if (!CanChangeBattleback)
			return false;
		GameManager.Instance.BattlebackLayers.Fade(layer, opacity, frames);
		return true;
	}

	/// <summary>
	/// Scrolls a battleback layer at a constant speed.
	/// </summary>
	/// <remarks>
	/// Does nothing for a number with no layer. Also does nothing while <see cref="CanChangeBattleback"/> is false.
	/// </remarks>
	/// <param name="layer">The layer number.</param>
	/// <param name="pixelsPerSecond">How far the layer moves each second.</param>
	/// <returns>True if the layer began scrolling, false if this enemy can't change the battleback.</returns>
	public bool ScrollBattlebackLayer(int layer, Vector2 pixelsPerSecond)
	{
		if (!CanChangeBattleback)
			return false;
		GameManager.Instance.BattlebackLayers.SetScroll(layer, pixelsPerSecond);
		return true;
	}

	/// <summary>
	/// Removes every battleback layer at once and makes the battleback fully visible again.
	/// </summary>
	/// <remarks>
	/// The battleback image is not changed. Does nothing while <see cref="CanChangeBattleback"/> is false.
	/// </remarks>
	/// <returns>True if the layers were cleared, false if this enemy can't change the battleback.</returns>
	public bool ClearBattlebackLayers()
	{
		if (!CanChangeBattleback)
			return false;
		GameManager.Instance.BattlebackLayers.Clear();
		return true;
	}

	/// <summary>
	/// Starts a BGM from the beginning.
	/// </summary>
	/// <remarks>
	/// Stops any BGM fade in progress, so the new BGM plays at the given volume.
	/// Does nothing while <see cref="CanChangeBGM"/> is false.
	/// </remarks>
	/// <param name="name">The name of the BGM to play.</param>
	/// <param name="volume">The volume to play the BGM at, from 0.001 to 3.0.</param>
	/// <param name="pitch">The pitch to play the BGM at, from 0.1 to 2.0.</param>
	/// <returns>True if the BGM was started, false if this enemy can't change the BGM.</returns>
	public bool ChangeBGM(string name, float volume = 1f, float pitch = 1f)
	{
		if (!CanChangeBGM)
			return false;
		AudioManager.Instance.ForcePlayBGM(name, volume, pitch);
		AudioManager.Instance.FadeBGMTo(volume, 0f);
		return true;
	}

	/// <summary>
	/// Fades the BGM to the given volume.
	/// </summary>
	/// <remarks>
	/// Does nothing while <see cref="CanChangeBGM"/> is false.
	/// </remarks>
	/// <param name="volume">The volume to fade to, from 0.001 to 3.0.</param>
	/// <param name="seconds">How long the fade takes, in seconds.</param>
	/// <returns>True if the fade was started, false if this enemy can't change the BGM.</returns>
	public bool FadeBGM(float volume, float seconds)
	{
		if (!CanChangeBGM)
			return false;
		AudioManager.Instance.FadeBGMTo(volume, seconds);
		return true;
	}

	/// <summary>
	/// Stops the BGM.
	/// </summary>
	/// <remarks>
	/// Does nothing while <see cref="CanChangeBGM"/> is false.
	/// </remarks>
	/// <returns>True if the BGM was stopped, false if this enemy can't change the BGM.</returns>
	public bool StopBGM()
	{
		if (!CanChangeBGM)
			return false;
		AudioManager.Instance.BGM.Stop();
		return true;
	}
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
	/// Vanilla's <c>Passive State: 184</c> "Observe: Has Multi". Only these enemies can be the one OBSERVE
	/// reports as having "eyes on everyone". The party-wide prediction it creates is read by every enemy's AI.
	/// </summary>
	protected internal virtual bool ObserveHasMulti => false;

	/// <summary>
	/// True while <see cref="BattleManager"/> evaluates <see cref="ProcessAI"/> for turn ordering only.
	/// <see cref="ProcessAI"/> must not change state, consume predictions or queue actions while this is set.
	/// </summary>
	/// <remarks>
	/// In vanilla OMORI, enemy AI is rolled at the start of the turn, again whenever the enemy's emotion group changes
	/// before it has acted (due to the enemy "transforming" into a different enemy), and once more
	/// before it is time to take their action. Only the final roll is used as the earlier rolls only decide turn order.
	/// This flag can help avoid running unrelated code during those ordering rolls.
	/// </remarks>
	protected bool PreRolling { get; private set; }

	/// <summary>
	/// The true turn number to check for in <see cref="ProcessAI"/> for an enemy that uses certain skills on certain turns.
	/// </summary>
	/// <remarks>
	/// During turn N it reads N, like <see cref="BattleManager.TurnCount"/>, except in the turn-order roll at the start of commands,
	/// which reads N - 1 because the base game makes that roll before the turn count goes up.
	/// The roll that picks the enemy's action and a turn-order re-roll after its emotion changes both read N.
	/// </remarks>
	protected int TrueTurnCount => BattleManager.Instance.TurnCount - (PreRolling && !EmotionRerolling ? 1 : 0);

	/// <summary>
	/// Turn-order priority taken from this turn's pre-rolled action.
	/// </summary>
	internal int TurnPriority { get; private set; }

	internal bool HasPreRolledAction { get; set; }

	private bool EmotionRerolling;

	internal void PreRollTurnPriority(bool emotionReroll = false)
	{
		PreRolling = true;
		EmotionRerolling = emotionReroll;
		HasPreRolledAction = true;
		try
		{
			TurnPriority = ProcessAI()?.Action.Priority ?? 0;
		}
		finally
		{
			PreRolling = false;
			EmotionRerolling = false;
		}
	}
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
	/// Called when the enemy is defeated, right before it becomes toast.
	/// </summary>
	public virtual async Task OnDefeat() { await Task.CompletedTask; }

	/// <summary>
	/// Gives this enemy IMMORTAL. It can drop to 0 HP, but it doesn't die until <see cref="RemoveImmortal"/> is called.
	/// </summary>
	public void AddImmortal()
	{
		ImmortalReleasePending = false;
		AddStatModifier("Immortal", silent: true);
		ClearResult();
	}

	/// <summary>
	/// Removes this enemy's IMMORTAL status. If it's at 0 HP, it then dies.
	/// </summary>
	/// <param name="afterForcedCommands">Whether to wait until this enemy's queued forced commands have run.</param>
	public void RemoveImmortal(bool afterForcedCommands = false)
	{
		if (afterForcedCommands)
		{
			ImmortalReleasePending = true;
			return;
		}
		ImmortalReleasePending = false;
		RemoveStatModifier("Immortal", true);
		ClearResult();
	}

	/// <summary>
	/// Whether this enemy is currently IMMORTAL. See <see cref="AddImmortal"/>.
	/// </summary>
	public bool IsImmortal => HasStatModifier("Immortal");

	internal bool ImmortalReleasePending { get; set; }

	/// <summary>
	/// Ends the battle early, the way the base game ends some story and scripted fights.
	/// </summary>
	/// <remarks>
	/// With the Allow Enemies to Abort Battle rule on, this enemy is held at its current HP, its <see cref="OnDefeat"/> runs for its lines, and the battle ends once the current action finishes, even with other enemies left. With the rule off, it is defeated instead, as if it died normally.<br/>
	/// Call it from <see cref="ProcessBattleConditions"/>.
	/// </remarks>
	/// <returns>True if the battle will end as aborted, false if this enemy is defeated instead.</returns>
	public async Task<bool> AbortBattle()
	{
		if (BattleManager.Instance.AbortRequested)
			return true;
		if (!SettingsMenuManager.Instance.AllowEnemyAbort)
		{
			RemoveImmortal();
			CurrentHP = 0;
			return false;
		}
		if (!IsImmortal || ImmortalReleasePending)
			AddImmortal();
		BattleManager.Instance.RequestAbort();
		await OnDefeat();
		return true;
	}

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
		if (PreRolling)
			return true;
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
		if (PreRolling)
			return true;
		ObserveTarget = null;
		ObserveMultiTarget = false;
		return true;
	}
}
