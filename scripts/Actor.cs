using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public abstract class Actor
{
	public event EventHandler OnStateChanged;
	public event EventHandler OnHPChanged;
	public event EventHandler OnJuiceChanged;

	public abstract string Name { get; }
	public AnimatedSprite2D Sprite;
	public Vector2 CenterPoint = Vector2.Zero;
	public string CurrentState;
	public Dictionary<string, Skill> Skills = [];
	public bool IsHurt = false;
	public int Level = 1;

	public Stats BaseStats;

	public List<StatModifier> StatModifiers = [];

	private int _CurrentHP = 0;
	public int CurrentHP
	{
		get { return _CurrentHP; }
		set
		{
			_CurrentHP = value;
			OnHPChanged?.Invoke(this, EventArgs.Empty);
		}
	} 

	private int _CurrentJuice = 0;
	public int CurrentJuice
	{
		get { return _CurrentJuice; }
		set
		{
			_CurrentJuice = value;
			OnJuiceChanged?.Invoke(this, EventArgs.Empty);
		}
	}

	protected virtual Stats GetBaseStats() { return BaseStats; }

	/// <summary>
	/// The Actor's base stats, any adjusted stats from weapons or buffs, and emotion stats.
	/// </summary>
	public Stats CurrentStats
	{
		get
		{
			Stats current = GetBaseStats();
			// TODO: afraid and stressed out
			switch (CurrentState)
			{
				case "happy":
					current.LCK *= 2;
					current.SPD = RoundedStat(current.SPD * 1.25f);
					current.HIT -= 10;
					break;
				case "ecstatic":
					current.LCK *= 3;
					current.SPD = RoundedStat(current.SPD * 1.5f);
					current.HIT -= 20;
					break;
				case "manic":
					current.LCK *= 4;
					current.SPD = RoundedStat(current.SPD * 2f);
					current.HIT -= 30;
					break;
				case "angry":
					current.ATK = RoundedStat(current.ATK * 1.3f);
					current.DEF = RoundedStat(current.DEF * 0.5f);
					break;
				case "enraged":
					current.ATK = RoundedStat(current.ATK * 1.5f);
					current.DEF = RoundedStat(current.DEF * 0.3f);
					break;
				case "furious":
					current.ATK = RoundedStat(current.ATK * 2f);
					current.DEF = RoundedStat(current.DEF * 0.15f);
					break;
				case "sad":
					current.DEF = RoundedStat(current.DEF * 1.25f);
					current.SPD = RoundedStat(current.SPD * 0.8f);
					break;
				case "depressed":
					current.DEF = RoundedStat(current.DEF * 1.35f);
					current.SPD = RoundedStat(current.SPD * 0.65f);
					break;
				case "miserable":
					current.DEF = RoundedStat(current.DEF * 1.5f);
					current.SPD = RoundedStat(current.SPD * 0.5f);
					break;
			}

			// TODO: try NOT to do this???
			// needs to be refactored 
			foreach (StatModifier modifier in StatModifiers)
			{
				switch (modifier.Modifier)
				{
					case Modifier.AttackUp:
						if (modifier.Tier == 1)
							current.ATK = RoundedStat(current.ATK * 1.1f);
						else if (modifier.Tier == 2)
							current.ATK = RoundedStat(current.ATK * 1.25f);
						else
							current.ATK = RoundedStat(current.ATK * 1.5f);
						break;
					case Modifier.AttackDown:
						if (modifier.Tier == 1)
							current.ATK = RoundedStat(current.ATK * 0.9f);
						else if (modifier.Tier == 2)
							current.ATK = RoundedStat(current.ATK * 0.8f);
						else
							current.ATK = RoundedStat(current.ATK * 0.7f);
						break;
					case Modifier.DefenseUp:
						if (modifier.Tier == 1)
							current.DEF = RoundedStat(current.DEF * 1.15f);
						else if (modifier.Tier == 2)
							current.DEF = RoundedStat(current.DEF * 1.3f);
						else
							current.DEF = RoundedStat(current.DEF * 1.5f);
						break;
					case Modifier.DefenseDown:
						if (modifier.Tier == 1)
							current.DEF = RoundedStat(current.DEF * 0.75f);
						else if (modifier.Tier == 2)
							current.DEF = RoundedStat(current.DEF * 0.5f);
						else
							current.DEF = RoundedStat(current.DEF * 0.25f);
						break;
					case Modifier.SpeedUp:
						if (modifier.Tier == 1)
							current.SPD = RoundedStat(current.SPD * 1.5f);
						else if (modifier.Tier == 2)
							current.SPD = RoundedStat(current.SPD * 2f);
						else
							current.SPD = RoundedStat(current.SPD * 5f);
						break;
					case Modifier.SpeedDown:
						if (modifier.Tier == 1)
							current.SPD = RoundedStat(current.SPD * 0.8f);
						else if (modifier.Tier == 2)
							current.SPD = RoundedStat(current.SPD * 0.5f);
						else
							current.SPD = RoundedStat(current.SPD * 0.25f);
						break;
					case Modifier.Flex:
						current.HIT += 1000;
						break;
					case Modifier.ReleaseEnergy:
						current.SPD = RoundedStat(current.SPD * 1.25f);
						current.ATK = RoundedStat(current.ATK * 1.25f);
						current.DEF = RoundedStat(current.DEF * 1.25f);
						current.LCK = RoundedStat(current.LCK * 1.25f);
						break;
					case Modifier.SnoCone:
                        current.SPD = RoundedStat(current.SPD * 1.2f);
                        current.ATK = RoundedStat(current.ATK * 1.2f);
                        current.DEF = RoundedStat(current.DEF * 1.2f);
                        current.LCK = RoundedStat(current.LCK * 1.2f);
                        break;
                }
			}

			return current;
		}
	}

	private int RoundedStat(float value)
	{
		return (int)Math.Round(value, MidpointRounding.AwayFromZero);
	}

	public void AddStatModifier(Modifier modifier, int tier = 1, int turns = 6, bool silent = false)
	{
		foreach (StatModifier mod in StatModifiers)
		{
			if (mod.Modifier == modifier)
			{
				if (mod.IncreaseTier())
					if (!silent)
						ShowStatSuccess(modifier);
				else if (!silent)
					ShowStatFail(modifier);
				return;
			}
		}
		StatModifiers.Add(new StatModifier(modifier, tier, turns));
		ShowStatSuccess(modifier);
	}

	public void RemoveStatModifier(Modifier modifier)
	{
		StatModifier mod = StatModifiers.FirstOrDefault(x => x.Modifier == modifier);
		if (mod != null)
			StatModifiers.Remove(mod);
	}

	private void ShowStatFail(Modifier modifer)
	{
		switch (modifer)
		{
			case Modifier.AttackUp:
				BattleLogManager.Instance.QueueMessage(Name.ToUpper() + "'s ATTACK cannot go any higher!");
				return;
			case Modifier.AttackDown:
				BattleLogManager.Instance.QueueMessage(Name.ToUpper() + "'s ATTACK cannot go any lower!");
				return;
			case Modifier.DefenseUp:
				BattleLogManager.Instance.QueueMessage(Name.ToUpper() + "'s DEFENSE cannot go any higher!");
				return;
			case Modifier.DefenseDown:
				BattleLogManager.Instance.QueueMessage(Name.ToUpper() + "'s DEFENSE cannot go any lower!");
				return;
			case Modifier.SpeedUp:
				BattleLogManager.Instance.QueueMessage(Name.ToUpper() + "'s SPEED cannot go any higher!");
				return;
			case Modifier.SpeedDown:
				BattleLogManager.Instance.QueueMessage(Name.ToUpper() + "'s SPEED cannot go any lower!");
				return;
		}
	}

	private void ShowStatSuccess(Modifier modifer)
	{
		switch (modifer)
		{
			case Modifier.AttackUp:
				BattleLogManager.Instance.QueueMessage(Name.ToUpper() + "'s ATTACK rose!");
				return;
			case Modifier.AttackDown:
				BattleLogManager.Instance.QueueMessage(Name.ToUpper() + "'s ATTACK fell!");
				return;
			case Modifier.DefenseUp:
				BattleLogManager.Instance.QueueMessage(Name.ToUpper() + "'s DEFENSE rose!");
				return;
			case Modifier.DefenseDown:
				BattleLogManager.Instance.QueueMessage(Name.ToUpper() + "'s DEFENSE fell!");
				return;
			case Modifier.SpeedUp:
				BattleLogManager.Instance.QueueMessage(Name.ToUpper() + "'s SPEED rose!");
				return;
			case Modifier.SpeedDown:
				BattleLogManager.Instance.QueueMessage(Name.ToUpper() + "'s SPEED fell!");
				return;
		}
	}

	public void DecreaseStatTurnCounter()
	{
		StatModifiers.ForEach(x => x.DecreaseTurn());
		if (this is Omori && HasStatModifier(Modifier.PlotArmor))
		{

		}
		StatModifiers.RemoveAll(x =>
		{
			if (this is Omori omori && x.Modifier == Modifier.PlotArmor)
			{
				// set the emotion background back to what it was before
				SetState(omori.OldEmotion, true);
			}
			return x.TurnsLeft <= 0;
		});

	}

	public bool HasStatModifier(Modifier modifier)
	{
		return StatModifiers.Any(x => x.Modifier == modifier);
	}

	public void Damage(int damage)
	{
		CurrentHP -= damage;
		if (CurrentHP < 0)
			CurrentHP = 0;
		SetHurt(true);

		// TODO: allow other characters to have plot armor if desired
		if (this is Omori omori && CurrentHP == 0 && !omori.HasUsedPlotArmor)
		{
			CurrentHP = 1;
			SetHurt(false);
			// keep track of omori's old emotion for when plot armor expires
			omori.OldEmotion = omori.CurrentState;
			SetState("plotarmor", true);
			AddStatModifier(Modifier.PlotArmor, 1, 1);
			omori.HasUsedPlotArmor = true;
		}
	}

	public void Heal(int health)
	{
		CurrentHP += health;
		if (CurrentHP > CurrentStats.MaxHP)
			CurrentHP = CurrentStats.MaxHP;
	}

	public void HealJuice(int juice)
	{
		CurrentJuice += juice;
		if (CurrentJuice > CurrentStats.MaxJuice)
			CurrentJuice = CurrentStats.MaxJuice;
	}


	public void SetHurt(bool hurt)
	{
		Sprite.Animation = hurt ? "hurt" : CurrentState;
		IsHurt = hurt;
	}

	public virtual bool IsStateValid(string state) { return true; }

	public void SetState(string state, bool silent = false)
	{
		if (IsStateValid(state))
		{
			Sprite.Animation = state;
			CurrentState = state;
			if (!silent)
			{
				BattleLogManager.Instance.QueueMessage(Name.ToUpper() + " feels " + state.ToUpper() + "!");
			}
			OnStateChanged?.Invoke(this, EventArgs.Empty);
		}
		else
		{
			BattleLogManager.Instance.QueueMessage(Name.ToUpper() + " cannot feel " + state.ToUpper() + "!");
		}
	}

	// forces a state without any validity checks
	// mainly used for bosses like Sweetheart
	public void ForceState(string state, bool silent = false)
	{
		Sprite.Animation = state;
		CurrentState = state;
		if (!silent)
			BattleLogManager.Instance.QueueMessage(Name.ToUpper() + " became " + state.ToUpper() + "!");
	}
}
