using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

/// <summary>
/// An <see cref="Actor"/> that is considered a party member. Can be inherited to make a new party member.
/// </summary>
public abstract class PartyMember : Actor
{
	/// <summary>
	/// Optional label/portrait asset shown on the battlecard while a <see cref="Actor.CurrentAnimation"/> override is active.
	/// </summary>
	public EmotionAsset CurrentAnimationAsset { get; private set; }

	/// <inheritdoc/>
	public sealed override PartyMemberComponent Component => (PartyMemberComponent)base.Component;

	/// <inheritdoc/>
	public override void PlayAnimation(string animationName)
	{
		CurrentAnimationAsset = null;
		base.PlayAnimation(animationName);
	}

	/// <summary>
	/// Overrides this PartyMember's emotion animation and shows the given label/portrait asset on their battlecard.<br/>
	/// See <see cref="Actor.PlayAnimation(string)"/>.<br/>
	/// If another animation override is already active, it will be replaced.
	/// </summary>
	/// <param name="animationName">The animation to play. Must exist in the actor's SpriteFrames.</param>
	/// <param name="asset">The label/portrait asset to show while the override is active.</param>
	public void PlayAnimation(string animationName, EmotionAsset asset)
	{
		CurrentAnimationAsset = asset;
		base.PlayAnimation(animationName);
	}

	/// <inheritdoc/>
	public override void ClearAnimation()
	{
		CurrentAnimationAsset = null;
		base.ClearAnimation();
	}

	internal bool Init(AnimatedSprite2D face, BattlePresetActor actor)
	{
		SpriteFrames animation = Animation;
        if (animation == null)
        {
            GD.PrintErr("Failed to load Face animations for PartyMember: " + Name);
            return false;
        }

		if (actor.Charm != null && !actor.Charm.Equals("none", StringComparison.CurrentCultureIgnoreCase))
		{
			if (!Database.TryGetEquipment(actor.Charm, out Equipment c))
			{
				GD.PrintErr("Failed to find Charm: " + actor.Charm);
				return false;
			}
			Charm = c;
		}

		if (actor.Weapon != null && !actor.Weapon.Equals("none", StringComparison.CurrentCultureIgnoreCase))
		{
			if (!Database.TryGetEquipment(actor.Weapon, out Equipment w))
			{
				GD.PrintErr("Failed to find Weapon: " + actor.Weapon);
				return false;
			}
			Weapon = w;
		}

        // if the party member starts toast, they technically start neutral
        bool startToast = actor.Emotion == "toast";
        string emotion = startToast ? "neutral" : actor.Emotion;
        // fall back to neutral on unknown or invalid preset emotions
        if (!Database.TryGetEmotion(emotion, out Emotion preset) || !animation.HasAnimation(preset.AnimationName) || !IsEmotionValid(preset.Id))
        {
            GD.PushWarning($"Invalid emotion '{emotion}' for PartyMember {Name}, defaulting to neutral.");
            actor.Emotion = "neutral";
            emotion = "neutral";
        }
        StartingEmotion = actor.Emotion;
        // init animation
        Sprite = face;
		Sprite.SpriteFrames = animation;
		SetEmotion(emotion, true);
		Sprite.Play();
		
        // init stats
        Level = actor.Level;
        int idx = Math.Clamp(actor.Level, 1, HPTree.Length) - 1;
		SetBaseStats(new Stats(HPTree[idx], JuiceTree[idx], ATKTree[idx], DEFTree[idx], SPDTree[idx], BaseLuck) + actor.AdjustedStats);

		if (startToast)
		{
			CurrentHP = 0;
			SetToast();
		}
		else
		{
			CurrentHP = CurrentStats.MaxHP;
		}
		CurrentJuice = CurrentStats.MaxJuice;

		EquippedSkills = actor.Skills;

		InitialSkills.AddRange(Skills.Keys);
		ResetSkillModel();

		return true;
	}

	private string AttackSkill;
	private readonly List<string> InitialSkills = [];
	private readonly List<string> PresetSkills = [];
	private readonly List<string> Learned = [];
	private readonly List<string> Slots = [null, null, null, null];

	/// <summary>
	/// Skills the member has learned during this battle.
	/// </summary>
	/// <remarks>
	/// The learned skills and the slots are the battle's own skills, kept after the preset's skills, which battle changes never remove.<br/>
	/// Every change rebuilds <see cref="Actor.Skills"/>.
	/// </remarks>
	public IReadOnlyList<string> LearnedSkills => Learned;

	/// <summary>
	/// The member's battle skill slots. An empty slot is null.
	/// </summary>
	/// <remarks>
	/// The learned skills and the slots are the battle's own skills, kept after the preset's skills, which battle changes never remove.<br/>
	/// Every change rebuilds <see cref="Actor.Skills"/>.
	/// </remarks>
	public IReadOnlyList<string> SkillSlots => Slots;

	internal bool SkillModelChanged { get; private set; }

	/// <summary>
	/// Learns a skill. A newly learned skill goes into the first empty slot, if there is one.
	/// </summary>
	/// <remarks>
	/// The learned skills and the slots are the battle's own skills, kept after the preset's skills, which battle changes never remove.<br/>
	/// Every change rebuilds <see cref="Actor.Skills"/>.
	/// </remarks>
	/// <param name="skill">The skill's key.</param>
	public void LearnSkill(string skill)
	{
		if (string.IsNullOrWhiteSpace(skill))
			return;
		Learn(skill);
		SkillModelChanged = true;
		RebuildSkills();
	}

	/// <summary>
	/// Forgets a skill. Its slots keep it.
	/// </summary>
	/// <remarks>
	/// The learned skills and the slots are the battle's own skills, kept after the preset's skills, which battle changes never remove.<br/>
	/// Every change rebuilds <see cref="Actor.Skills"/>.
	/// </remarks>
	/// <param name="skill">The skill's key.</param>
	public void ForgetSkill(string skill)
	{
		Learned.Remove(skill);
		SkillModelChanged = true;
		RebuildSkills();
	}

	/// <summary>
	/// Puts a skill in a battle slot.
	/// </summary>
	/// <remarks>
	/// The learned skills and the slots are the battle's own skills, kept after the preset's skills, which battle changes never remove.<br/>
	/// Every change rebuilds <see cref="Actor.Skills"/>.
	/// </remarks>
	/// <param name="slot">The slot, from 0. A slot past the end adds empty slots up to it.</param>
	/// <param name="skill">The skill's key. Can include an unlearned skill.</param>
	public void EquipSkill(int slot, string skill)
	{
		if (slot < 0)
		{
			GD.PushError($"Invalid skill slot {slot} for {Name}.");
			return;
		}
		ExtendSlots(slot);
		Slots[slot] = string.IsNullOrWhiteSpace(skill) ? null : skill;
		SkillModelChanged = true;
		RebuildSkills();
	}

	/// <summary>
	/// Empties a battle slot, and learns its skill again unless <paramref name="relearn"/> is false.
	/// </summary>
	/// <remarks>
	/// A skill that wasn't learned yet can land in another empty slot when it's learned again.<br/>
	/// The learned skills and the slots are the battle's own skills, kept after the preset's skills, which battle changes never remove.<br/>
	/// Every change rebuilds <see cref="Actor.Skills"/>.
	/// </remarks>
	/// <param name="slot">The slot, from 0. A slot past the end adds empty slots up to it.</param>
	/// <param name="relearn">Whether the slot's skill is learned again before the slot empties.</param>
	public void UnequipSkill(int slot, bool relearn = true)
	{
		if (slot < 0)
		{
			GD.PushError($"Invalid skill slot {slot} for {Name}.");
			return;
		}
		string held = slot < Slots.Count ? Slots[slot] : null;
		if (relearn && held != null)
			Learn(held);
		ExtendSlots(slot);
		Slots[slot] = null;
		SkillModelChanged = true;
		RebuildSkills();
	}

	/// <summary>
	/// Marks skills as already learned, without equipping them.
	/// </summary>
	/// <remarks>
	/// The learned skills and the slots are the battle's own skills, kept after the preset's skills, which battle changes never remove.<br/>
	/// Every change rebuilds <see cref="Actor.Skills"/>.
	/// </remarks>
	/// <param name="skills">The skills' keys.</param>
	public void SeedLearnedSkills(params string[] skills)
	{
		foreach (string skill in skills ?? [])
		{
			if (!string.IsNullOrWhiteSpace(skill) && !Learned.Contains(skill))
				Learned.Add(skill);
		}
		SkillModelChanged = true;
		RebuildSkills();
	}

	internal void SetAttackSkill(string skill)
	{
		AttackSkill = skill;
		SkillModelChanged = true;
		RebuildSkills();
	}

	internal void ResetSkillModel()
	{
		AttackSkill = EquippedSkills is { Length: > 0 } ? EquippedSkills[0] : null;
		PresetSkills.Clear();
		PresetSkills.AddRange(InitialSkills);
		if (EquippedSkills != null)
			PresetSkills.AddRange(EquippedSkills.Skip(1).Where(s => !string.IsNullOrWhiteSpace(s)));
		Learned.Clear();
		Slots.Clear();
		Slots.AddRange(new string[4]);
		SkillModelChanged = false;
		RebuildSkills(true);
	}

	private void Learn(string skill)
	{
		if (Learned.Contains(skill))
			return;
		Learned.Add(skill);
		int empty = Slots.IndexOf(null);
		if (empty >= 0)
			Slots[empty] = skill;
	}

	private void ExtendSlots(int slot)
	{
		while (Slots.Count <= slot)
			Slots.Add(null);
	}

	private void RebuildSkills(bool report = false)
	{
		Skills.Clear();
		foreach (string s in PresetSkills.Prepend(AttackSkill).Concat(Slots))
		{
			if (string.IsNullOrWhiteSpace(s))
				continue;

			if (Database.TryGetSkill(s, out Skill skill))
			{
				if (!Skills.TryAdd(s, skill) && report)
					GD.PushWarning($"Actor {Name} already has skill {s} equipped! Skipping...");
				continue;
			}
			if (report)
				GD.PrintErr("Unknown skill: " + s);
		}
	}

	/// <summary>
	/// Replaces one of this party member's skills with another. Does nothing if the member doesn't have <paramref name="from"/> or <paramref name="to"/> doesn't exist.
	/// </summary>
	/// <remarks>
	/// The replacement takes <paramref name="from"/>'s place and lasts through later skill changes in this battle. The member's skills go back to the preset's when the battle ends, so in Boss Rush a replacement doesn't carry over to the next stage.
	/// </remarks>
	/// <param name="from">The skill to replace.</param>
	/// <param name="to">The replacement skill.</param>
	public void ReplaceSkill(string from, string to)
	{
		if (!Skills.ContainsKey(from) || !Database.TryGetSkill(to, out Skill replacement))
			return;
		List<KeyValuePair<string, Skill>> entries = Skills.ToList();
		Skills.Clear();
		foreach (KeyValuePair<string, Skill> entry in entries)
		{
			if (entry.Key == from)
				Skills.TryAdd(to, replacement);
			else
				Skills.TryAdd(entry.Key, entry.Value);
		}
		for (int i = 0; i < PresetSkills.Count; i++)
		{
			if (PresetSkills[i] == from)
				PresetSkills[i] = to;
		}
		for (int i = 0; i < Slots.Count; i++)
		{
			if (Slots[i] == from)
				Slots[i] = to;
		}
		if (AttackSkill == from)
			AttackSkill = to;
		SkillModelChanged = true;
	}

	internal void RestoreEquippedSkills()
	{
		ResetSkillModel();
	}

	/// <summary>
	/// The party member's base stats, plus any stats given by a <see cref="Battle.Weapon"/> and/or <see cref="Equipment"/>.
	/// </summary>
	/// <returns></returns>
	public override Stats GetBaseStats()
	{
		Stats stats = BaseStats;
		Weapon?.Apply(ref stats);
		Charm?.Apply(ref stats);
		return stats;
	}

	/// <inheritdoc/>
	public override bool IsEmotionValid(Emotion emotion)
	{
		if (emotion.Id == "neutral")
			return true;
		if (IsImmuneToEmotion(emotion))
			return false;
		return !InvalidStates.Contains(emotion.Id);
	}

	/// <inheritdoc/>
	protected override IEnumerable<Equipment> EquippedItems => [Weapon, Charm];

    /// <inheritdoc/>
    public override async Task OnStartOfBattle()
    {
		if (Weapon != null)
			await Weapon.StartOfBattle(this);
		if (Charm != null)
			await Charm.StartOfBattle(this);
    }

	/// <summary>
	/// Called after each action finishes, before the enemies' <see cref="Enemy.ProcessBattleConditions"/>.
	/// Not called if the party member is toast.
	/// </summary>
	public virtual async Task ProcessBattleConditions() { await Task.CompletedTask; }
	
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
	/// A list of skill IDs that this actor has equipped. The first one is the basic attack, used by ATTACK.
	/// </summary>
	public string[] EquippedSkills { get; protected set; }
	/// <summary>
	/// A list of Weapons that this actor can equip in the base game.<br/>
	/// Mainly used for the "Filter Equippable" setting in the editor.
	/// </summary>
	public virtual string[] EquippableWeapons { get; protected set; } = [];
	/// <summary>
	/// A list of invalid emotion ids this party member cannot feel. Used in <see cref="IsEmotionValid(Emotion)"/>
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
	internal string StartingEmotion { get; private set; }
}
