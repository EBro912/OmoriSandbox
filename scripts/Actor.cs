using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;
using OmoriSandbox.Battle.Modifier;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
	public Vector2 CenterPoint = Vector2.Zero;
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
			OnHPChanged?.Invoke(this, EventArgs.Empty);
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
			OnJuiceChanged?.Invoke(this, EventArgs.Empty);
		}
	}

	/// <summary>
	/// Whether the actor is currently stunned. Setting this value to true will cause their actions to be skipped.
	/// </summary>
	public bool Stunned = false;

	/// <summary>
	/// Whether the actor is toast.
	/// </summary>
	public bool IsToast { get; private set; }
	
	/// <summary>
	/// The actor's base stats without any modifiers.
	/// </summary>
	/// <returns></returns>
	protected virtual Stats GetBaseStats() { return BaseStats; }

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
	/// </summary>
	public Stats CurrentStats
	{
		get
		{
			Stats current = GetBaseStats();

			// emotion stats always apply before any other modifiers
			StatBonus.ApplyAll(ref current, GetEmotionStatBonuses(CurrentEmotion));

			foreach (StatModifier mod in StatModifiers.Values)
			{
				mod.ApplyStats(ref current);
			}
			
			return current;
		}
	}

	/// <summary>
	/// Adds a new <see cref="StatModifier"/> to this actor.
	/// </summary>
	/// <param name="modifier">The name of the modifier to add.</param>
	/// <param name="turns">Overrides the default number of turns to give this modifier for. If unchanged, will use the default turn count for the modifier.</param>
	/// <param name="silent">If true, success/failure messages will not be logged.</param>
	public void AddStatModifier(string modifier, int turns = -1, bool silent = false)
	{
		if (StatModifiers.TryGetValue(modifier, out StatModifier m))
		{
			if (m is TierStatModifier tier)
			{
				bool success = tier.ApplyTier(1);
				if (success)
				{
                    GD.Print("Increased tier of " + modifier + " on " + Name + " to " + tier.CurrentTier);
                }
				if (!silent && tier.SuccessMessage != null)
					ShowStatMessage(success ? tier.SuccessMessage : tier.FailureMessage);
				return;
			}
			m.RefreshTurns();
			GD.Print("Refreshed modifier " + modifier + " on " + Name);
		}
		else
		{
			StatModifier mod = Database.CreateModifier(modifier);
			if (mod == null)
			{
				GD.PrintErr("Unknown stat modifier: " + modifier);
				return;
			}

			if (turns > -1)
			{
				mod.SetTurnsLeft(turns);
			}

			StatModifiers.Add(modifier, mod);
			mod.OnAdd(this);
			GD.Print("Added modifier " + modifier + " to " + Name);
			if (mod is TierStatModifier t && t.SuccessMessage != null && !silent)
				ShowStatMessage(t.SuccessMessage);
		}
	}

    /// <summary>
    /// Adds a new <see cref="TierStatModifier"/> to this actor.
    /// </summary>
    /// <param name="modifier">The name of the tier modifier to add.</param>
    /// <param name="tier">The tier that this modifier will start at.</param>
    /// <param name="turns">The number of turns left the modifier will start at.</param>
    /// <param name="silent">If true, success/failure messages will not be logged.</param>
    public void AddTierStatModifier(string modifier, int tier = 1, int turns = 6, bool silent = false)
	{
		StatModifier mod = Database.CreateModifier(modifier);
		if (mod is not TierStatModifier t)
		{
			GD.PushWarning("Tried to add a non-tiered stat modifier with tier and turns: " + modifier);
			AddStatModifier(modifier, silent: silent);
			return;
		}
		if (StatModifiers.TryGetValue(modifier, out StatModifier m))
		{
			TierStatModifier existing = m as TierStatModifier;
			bool success = existing.ApplyTier(tier);
			if (success)
			{
                GD.Print("Increased tier of " + modifier + " on " + Name + " to " + existing.CurrentTier);
			}
			if (!silent && existing.SuccessMessage != null)
			{
				ShowStatMessage(success ? existing.SuccessMessage : existing.FailureMessage);
			}
			return;
		}
		t.WithTier(tier);
		t.SetTurnsLeft(turns);
		StatModifiers.Add(modifier, t);
		t.OnAdd(this);
		GD.Print("Added modifier " + modifier + " to " + Name);
		if (!silent && t.SuccessMessage != null)
			ShowStatMessage(t.SuccessMessage);
	}

	/// <summary>
	/// Removes a <see cref="StatModifier"/> of the given name from this actor.
	/// </summary>
	/// <param name="modifier">The name of the modifier to remove.</param>
	public void RemoveStatModifier(string modifier)
	{
		StatModifiers.Remove(modifier);
	}

	/// <summary>
	/// Removes all <see cref="StatModifier"/>s from this actor.
	/// </summary>
	public void RemoveAllStatModifiers()
	{
		StatModifiers.Clear();
	}

	private void ShowStatMessage(string message)
	{
		BattleLogManager.Instance.QueueMessage($"{Name.ToUpper()}'s {message}");
	}

	internal void DecreaseStatTurnCounter()
	{
		foreach (var mod in StatModifiers)
		{
			if (mod.Value.TurnsLeft != -1)
			{
				mod.Value.DecreaseTurns();
				if (mod.Value.TurnsLeft <= 0)
				{
					GD.Print("Removed modifier " + mod.Key + " from " + Name);
					StatModifiers.Remove(mod.Key);
				}
			}
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
	/// If the actor does not have the requested modifier or if it is not a tiered stat modifier, -1 is returned.
	/// </remarks>
	/// <param name="modifier">The modifier to get the current tier of.</param>
	/// <returns>The current tier if the actor has the tiered modifier, otherwise -1.</returns>
	public int GetStatModifierTier(string modifier)
	{
		if (!StatModifiers.TryGetValue(modifier, out StatModifier mod))
			return -1;
		if (mod is TierStatModifier tier)
			return tier.CurrentTier;
		return -1;
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
	/// Negative values should not be used. See <see cref="Heal(int)"/> for healing actors.
	/// </remarks>
	/// <param name="damage">The amount of damage to deal to this actor.</param>
	public void Damage(int damage)
	{
		if (damage <= 0)
			return;

		CurrentHP -= damage;
		if (CurrentHP < 0)
			CurrentHP = 0;

		if (this is PartyMember member && member.HasPlotArmor && CurrentHP == 0 && !member.HasUsedPlotArmor)
		{
			CurrentHP = 1;
			member.HasUsedPlotArmor = true;
			member.PlayAnimation("plotarmor", EmotionAsset.PlotArmor);
			AddStatModifier("PlotArmor");
			return;
		}

		OnDamaged?.Invoke(this, EventArgs.Empty);
	}

    /// <summary>
    /// Damages this actor's juice by the given amount.
    /// </summary>
	/// <remarks>
	/// Negative values should not be used. See <see cref="HealJuice(int)"/> for healing juice.<br/>
	/// This will also not cause the actor to show the hurt animation.
	/// </remarks>
    /// <param name="damage">The amount of juice damage to deal to this actor.</param>
    public void DamageJuice(int damage)
	{
		if (damage <= 0)
			return;

		CurrentJuice -= damage;
		if (CurrentJuice < 0)
			CurrentJuice = 0;
    }

    /// <summary>
    /// Heals this actor by the given amount.
    /// </summary>
    /// <param name="health">The amount of health to heal.</param>
    public void Heal(int health)
	{
		CurrentHP += health;
		if (CurrentHP > CurrentStats.MaxHP)
			CurrentHP = CurrentStats.MaxHP;
	}

	/// <summary>
	/// Heals this actor's juice by the given amount.
	/// </summary>
	/// <param name="juice">The amount of juice to heal.</param>
	public void HealJuice(int juice)
	{
		CurrentJuice += juice;
		if (CurrentJuice > CurrentStats.MaxJuice)
			CurrentJuice = CurrentStats.MaxJuice;
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
		if (CurrentAnimation != null)
			return;

		Sprite.Animation = hurt ? "hurt" : CurrentEmotion.AnimationName;
	}

	/// <summary>
	/// Overrides this actor's sprite animation, without changing their emotion or stats.<br/>
	/// The override stays active until cleared or overwritten, including any emotion changes and damage.<br/>
	/// See <see cref="ClearAnimation"/>.
	/// </summary>
	/// <param name="animationName">The animation to play. Must exist in the actor's SpriteFrames.</param>
	public virtual void PlayAnimation(string animationName)
	{
		if (Sprite?.SpriteFrames == null || !Sprite.SpriteFrames.HasAnimation(animationName))
		{
			GD.PushWarning(Name + " cannot play unknown animation: " + animationName);
			return;
		}

		CurrentAnimation = animationName;
		Sprite.Animation = animationName;
		OnAnimationChanged?.Invoke(this, EventArgs.Empty);
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
		Sprite.Animation = IsToast ? "toast" : CurrentEmotion.AnimationName;
		OnAnimationChanged?.Invoke(this, EventArgs.Empty);
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
	}

	/// <summary>
	/// Checks if this actor can feel the given emotion.
	/// </summary>
	/// <param name="emotion">The emotion to check.</param>
	/// <returns>True if this actor can feel the given <paramref name="emotion"/>.</returns>
	public virtual bool IsEmotionValid(Emotion emotion) { return true; }

    /// <summary>
    /// Sets this actor's emotion by id. Will fail and log a battle message if the actor cannot feel the given emotion,
    /// or if their emotion is locked.<br/>
    /// See <see cref="IsEmotionValid(Emotion)"/> and <see cref="LockEmotion"/>.
    /// </summary>
    /// <param name="id">The id of the emotion to set this actor to.</param>
    /// <param name="silent">If true, success/failure messages will not be logged.</param>
    /// <returns>Whether the emotion was applied.</returns>
    public bool SetEmotion(string id, bool silent = false)
	{
		if (!Database.TryGetEmotion(id, out Emotion emotion))
		{
			GD.PrintErr("Unknown emotion: " + id);
			return false;
		}

		if (!IsEmotionLocked && IsEmotionValid(emotion))
		{
			CurrentEmotion = emotion;
			if (!silent)
			{
				BattleLogManager.Instance.QueueMessage(Name.ToUpper() + " feels " + emotion.DisplayName + "!");
			}

			OnEmotionChanged?.Invoke(this, EventArgs.Empty);
			// only update the sprite if no animation override is active
			if (CurrentAnimation == null) {
				Sprite.Animation = emotion.AnimationName;
			}
			return true;
		}

		if (!silent)
		{
			BattleLogManager.Instance.QueueMessage(Name.ToUpper() + " cannot be " + emotion.DisplayName + "!");
		}
		return false;
	}

	/// <summary>
	/// Silently forces this actor to feel an emotion, bypassing emotion locks and validity checks. Mainly used for boss phase changes. Should be used sparingly.
	/// </summary>
	/// <param name="id">The id of the emotion to force this actor to feel.</param>
	public void SetEmotionForced(string id)
	{
		if (!Database.TryGetEmotion(id, out Emotion emotion))
		{
			GD.PrintErr("Unknown emotion: " + id);
			return;
		}

		CurrentEmotion = emotion;
		OnEmotionChanged?.Invoke(this, EventArgs.Empty);
		if (CurrentAnimation == null)
		{
			Sprite.Animation = emotion.AnimationName;
		}
	}
	/// <summary>
	/// Called at the very start of the battle.
	/// </summary>
	public virtual async Task OnStartOfBattle() { await Task.CompletedTask; }
	/// <summary>
	/// Called when the battle is over, but before the victory screen.
	/// </summary>
	/// <param name="victory">Whether the battle was won by the player.</param>
	public virtual async Task OnEndOfBattle(bool victory) { await Task.CompletedTask; }
}
