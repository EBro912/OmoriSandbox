using Godot;
using System.Linq;

public abstract class PartyMember : Actor
{
	public void Init(AnimatedSprite2D face, string initialState, int level, string weapon, string charm)
	{
		SpriteFrames animation = GD.Load<SpriteFrames>(AnimationPath);
		if (animation == null)
		{
			GD.PrintErr("Failed to load Face animations for PartyMember: " + Name);
			return;
		}
		// init animation
		Sprite = face;
		Sprite.SpriteFrames = animation;
		Sprite.Animation = initialState;
		Sprite.Play();
		CurrentState = initialState;

		// init stats
		int idx = level - 1;
		BaseStats = new Stats(HPTree[idx], JuiceTree[idx], ATKTree[idx], DEFTree[idx], SPDTree[idx], BaseLuck, 0);
		if (!Database.TryGetWeapon(weapon, out Weapon w))
		{
			GD.PrintErr("Failed to find Weapon: " + weapon);
			return;
		}
		Weapon = w;
		
		if (charm != null)
		{
			if (!Database.TryGetCharm(charm, out Charm c))
			{
				GD.PrintErr("Failed to find Charm: " + charm);
				return;
			}
			Charm = c;
		}

		CurrentHP = CurrentStats.HP;
		CurrentJuice = CurrentStats.Juice;

		foreach (string s in EquippedSkills)
		{
			if (Database.TryGetSkill(s, out var skill))
			{
				Skills.Add(s, skill);
				continue;
			}
			GD.PrintErr("Unknown skill: " + s);
		}
	}

	protected override Stats GetBaseStats()
	{
		Stats stats = BaseStats + Weapon.Stats;
		if (Charm != null)
		{
			stats += Charm.Apply();
		}
		return stats;
	}

	public override bool IsStateValid(string state)
	{
		return !(InvalidStates.Any(x => x == state) || (Charm != null && Charm.Name == "Paper Bag"));
	}

	public abstract string AnimationPath { get; }
	public abstract int[] HPTree { get; }
	public abstract int[] JuiceTree { get; }
	public abstract int[] ATKTree { get; }
	public abstract int[] DEFTree { get; }
	public abstract int[] SPDTree { get; }
	public abstract int BaseLuck { get; }
	public Charm Charm { get; private set; }
	public Weapon Weapon { get; private set; }
	protected abstract string[] EquippedSkills { get; }
	protected abstract string[] InvalidStates { get; }
	public abstract bool IsRealWorld { get; }
}
