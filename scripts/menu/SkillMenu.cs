using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using OmoriSandbox.Actors;
using OmoriSandbox.Battle;

namespace OmoriSandbox.Menu;

internal partial class SkillMenu : Menu
{
	[Export] public AutofitLabel[] SkillLabels;
	[Export] public Label CostText;
	private readonly List<Skill> Skills = [];
	private List<Vector2I> Positions = [new(28, 52), new(200, 52), new(28, 76), new(200, 76)];

	private Vector2I GridSize = new(2, 2);
	private PartyMember Actor;

	protected override Vector2 OpenPosition => new(138, 384);
	protected override Vector2 ClosedPosition => new(138, 490);

	public void Populate(PartyMember actor)
	{
		Skills.Clear();
		Actor = actor;
		CostText.Text = "0";
		foreach (AutofitLabel l in SkillLabels)
			l.Text = "";
		int idx = 0;
		foreach (Skill s in actor.Skills.Values.Where(x => !x.Hidden))
		{
			// since Actor.Skills is a dictionary, we need to check here if the skill is the PartyMember's attack skill,
			// i.e. the first one in their skill list
			// OrderedDictionary doesn't accept types for whatever reason...
			if (s.Name == actor.EquippedSkills[0])
				continue;
			if (idx > 3)
				break;
			SkillLabels[idx].SetFittedText(s.Name);
			if (actor.CurrentJuice < s.Cost(actor) || !s.MeetsRequirements(actor))
				SkillLabels[idx].AddThemeColorOverride("font_color", Colors.DimGray);
			else
				SkillLabels[idx].RemoveThemeColorOverride("font_color");
			Skills.Add(s);
			idx++;
		}
		if (SkillLabels.All(x => x.Text == ""))
		{
			CursorPositions = Positions.GetRange(0, 1);
			Empty = true;
			return;
		}
		Empty = false;
		CursorPositions = Positions.GetRange(0, Skills.Count);
	}
	
	private void ShowSkillInfo()
	{
		if (Empty) return;
		Skill s = Skills[CursorIndex];
		CostText.Text = s.Cost(Actor).ToString();
		BattleLogManager.Instance.ClearAndShowMessage($"[font_size=28]{s.Name}\n[font_size=20]{s.Description.Replace("[actor]", Actor.Name.ToUpper()).Replace("[first]", BattleManager.Instance.GetPartyMember(0).Name.ToUpper())}");
	}

	protected override void MoveCursor(Vector2I direction)
	{
		if (Empty) return;
		if (BattleManager.Instance.Phase == BattlePhase.TargetSelection) return;
		int old = CursorIndex;
		// omori menus have no wrapping
		// pressing left or right simply increments/decrements the index
		if (direction == Vector2.Left)
			CursorIndex = Math.Max(CursorIndex - 1, 0);
		else if (direction == Vector2.Right)
			CursorIndex = Math.Min(CursorIndex + 1, Skills.Count - 1);
		else if (direction == Vector2.Up)
		{
			if (CursorIndex > 1)
				CursorIndex -= 2;
		}
		else if (direction == Vector2.Down)
		{
			if (CursorIndex < 2 && Skills.Count > 2)
				CursorIndex = Math.Min(CursorIndex + 2, Skills.Count - 1);
		}
		if (CursorIndex != old)
		{
			UpdateCursor();
			ShowSkillInfo();
			AudioManager.Instance.PlaySFX("SYS_move");
		}
	}

	protected override void OnSelect()
	{
		if (Empty) return;
		Skill selected = Skills[CursorIndex];
		if (BattleManager.Instance.OnSelectSkill(selected))
			CursorSprite.StopBounce();
	}

	public override void OnOpen(SelectionMemory memory)
	{
		if (memory.SavedState == MenuState.Skill)
		{
			CursorIndex = memory.SavedIndex;
			Show();
			UpdateCursor();
		}
		else
			base.OnOpen(memory);
		ShowSkillInfo();
		CursorSprite.StartBounce();
	}
}
