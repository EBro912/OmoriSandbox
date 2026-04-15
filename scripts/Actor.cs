using Godot;
using OmoriSandbox.Battle;
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
	/// Fired whenever the actor's state (emotion) changes.
	/// </summary>
	public event EventHandler OnStateChanged;
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
	/// The actor's current state (emotion).
	/// </summary>
	public string CurrentState;
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
	/// <summary>
	/// The <see cref="StatModifier"/> that the actor's emotion gives.
	/// </summary>
	public StatModifier StateStatModifier { get; private set; } = new StatModifier([]);

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

			StateStatModifier.ApplyStats(ref current);

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
	/// <param name="turns">Overrides the default number of turns to give this modifier for. If unchanged, will use the default turn count for the modifier.
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
	/// Checks if this actor currently has a locked emotion.
	/// </summary>
	/// <returns>True if the actor's <see cref="StateStatModifier"/> is a <see cref="EmotionLockStatModifier"/>.</returns>
	public bool HasLockedEmotion()
	{
		return StateStatModifier is EmotionLockStatModifier;
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
			// temporarily set our state to plotarmor to trigger the state animator
			string temp = CurrentState;
			CurrentState = "plotarmor";
			Sprite.Animation = "plotarmor";
			OnStateChanged?.Invoke(this, EventArgs.Empty);
			CurrentState = temp;
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
	/// <param name="hurt">Whether this actor should appear hurt.</param>
	public virtual void SetHurt(bool hurt)
	{
		if (HasStatModifier("PlotArmor"))
			return;

		Sprite.Animation = hurt ? "hurt" : CurrentState;
	}

	/// <summary>
	/// Checks if this actor can feel the given state (emotion).
	/// </summary>
	/// <param name="state">The emotion to check.</param>
	/// <returns>True if this actor can feel the given <paramref name="state"/>.</returns>
	public virtual bool IsStateValid(string state) { return true; }

    /// <summary>
    /// Sets this actor's state to the given state (emotion). Will fail and log a battle message if the actor cannot feel the given <paramref name="state"/>.<br/>
    /// See <see cref="IsStateValid(string)"/>.
    /// </summary>
    /// <param name="state">The emotion to set this actor to.</param>
    /// <param name="silent">If true, success/failure messages will not be logged.</param>
    public void SetState(string state, bool silent = false)
	{
		if (IsStateValid(state))
		{
			CurrentState = state;
			if (!silent)
			{
				BattleLogManager.Instance.QueueMessage(Name.ToUpper() + " feels " + state.ToUpper() + "!");
			}

			// kinda dumb but the rest of the modifiers are capitalized so whatever
			StatModifier mod = Database.CreateModifier(Capitalize(CurrentState));
			if (mod != null)
			{
				StateStatModifier = mod;
			}

			OnStateChanged?.Invoke(this, EventArgs.Empty);
			// only update the face sprite if we're not in plot armor
			if (!HasStatModifier("PlotArmor")) {
				Sprite.Animation = state;
			}
		}
		else
		{
			BattleLogManager.Instance.QueueMessage(Name.ToUpper() + " cannot be " + state.ToUpper() + "!");
		}
	}


	/// <summary>
	/// Forces this actor to have a state (emotion) without any validity checks. Mainly used for bosses like Sweetheart. Should be used sparingly.
	/// </summary>
	/// <param name="state">The emotion to force this actor to have.</param>
	/// <param name="fakeState">If set, the actor will feel the <paramref name="fakeState"/> but use the stats of the <paramref name="state"/>.</param>
	public void ForceState(string state, string fakeState = null)
	{
		// TODO: attach emotion/animation info to non-emotion modifiers, like boss specific emotions
		if (fakeState != null)
		{
			Sprite.Animation = fakeState;
			CurrentState = fakeState;
		}
		else
		{
			Sprite.Animation = state;
			CurrentState = state;
		}
		StatModifier mod = Database.CreateModifier(Capitalize(state));
		if (mod != null)
		{
			StateStatModifier = mod;
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

	private string Capitalize(string s)
	{
		// using ToCharArray avoids an extra string allocation
		char[] a = s.ToCharArray();
		a[0] = char.ToUpper(a[0]);
		return new string(a);
	}
}
