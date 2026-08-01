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
	[Export] private Sprite2D PageUpSprite;
	[Export] private Sprite2D PageDownSprite;
	private readonly List<Skill> Skills = [];
	private List<Skill> DisplayedSkills = [];
	public int Page { get; private set; } = 0;
	private List<Vector2I> Positions = [new(28, 52), new(200, 52), new(28, 76), new(200, 76)];

	private PartyMember Actor;

	protected override Vector2 OpenPosition => new(138, 384);
	protected override Vector2 ClosedPosition => new(138, 490);

	private int MaxPage => Math.Max(0, (Skills.Count - 3) / 2);

	public override void OnOpen(SelectionMemory memory)
	{
		PageUpSprite.Visible = false;
		PageDownSprite.Visible = false;
		if (memory.SavedState == MenuState.Skill &&
			memory.SavedPage <= MaxPage &&
			memory.SavedIndex < Skills.Count)
		{
			CursorIndex = memory.SavedIndex;
			Page = memory.SavedPage;
		}
		else
		{
			CursorIndex = 0;
			Page = 0;
		}
		CursorSprite.StartBounce();
		UpdatePage();
		Show();
	}

	public void Populate(PartyMember actor)
	{
		Skills.Clear();
		Actor = actor;
		foreach (var s in actor.Skills.Where(x => !x.Value.Hidden))
		{
			if (actor.EquippedSkills == null || actor.EquippedSkills.Length == 0)
			{
				GD.PrintErr($"Actor {actor.Name} has no equipped skills.");
				break;
			}
			// since Actor.Skills is a dictionary, we need to check here if the skill is the PartyMember's attack skill,
			// i.e. the first one in their skill list
			// OrderedDictionary doesn't accept types for whatever reason...
			if (s.Key == actor.EquippedSkills[0])
				continue;
			Skills.Add(s.Value);
		}
		Empty = Skills.Count == 0;
	}

	private void UpdatePage()
	{
		CostText.Text = "0";
		foreach (AutofitLabel l in SkillLabels)
		{
			l.Text = "";
			l.RemoveThemeColorOverride("font_color");
		}
		if (Empty)
		{
			CursorPositions = Positions.GetRange(0, 1);
			CursorIndex = 0;
			UpdateCursor();
			return;
		}

		PageUpSprite.Visible = Page > 0;
		PageDownSprite.Visible = Page < MaxPage;
		int start = Page * 2;
		int end = Mathf.Min(start + 4, Skills.Count);
		DisplayedSkills = Skills.GetRange(start, end - start);
		for (int i = 0; i < DisplayedSkills.Count; i++)
		{
			SkillLabels[i].SetFittedText(DisplayedSkills[i].Name);
			if (Actor.CurrentJuice < DisplayedSkills[i].Cost(Actor) || !DisplayedSkills[i].MeetsRequirements(Actor))
				SkillLabels[i].AddThemeColorOverride("font_color", Colors.DimGray);
		}
		CursorPositions = Positions.GetRange(0, DisplayedSkills.Count);
		if (CursorIndex >= DisplayedSkills.Count)
			CursorIndex = DisplayedSkills.Count - 1;
		UpdateCursor();
		ShowSkillInfo();
	}

	private void ShowSkillInfo()
	{
		if (Empty) return;
		Skill s = DisplayedSkills[CursorIndex];
		CostText.Text = s.Cost(Actor).ToString();
		BattleLogManager.Instance.ClearAndShowMessage($"[font_size=28]{s.Name}\n[font_size=20]{s.Description.Replace("[actor]", Actor.Name.ToUpper()).Replace("[first]", BattleManager.Instance.GetPartyMember(0).Name.ToUpper())}");
	}

	protected override void MoveCursor(Vector2I direction)
	{
		if (Empty) return;
		if (BattleManager.Instance.Phase == BattlePhase.TargetSelection) return;
		if (direction == Vector2.Down && Page < MaxPage && CursorIndex > 1)
		{
			Page++;
			CursorIndex -= 2;
			AudioManager.Instance.PlaySFX("SYS_move");
			UpdatePage();
			return;
		}
		if (direction == Vector2.Up && Page > 0 && CursorIndex < 2)
		{
			Page--;
			CursorIndex += 2;
			AudioManager.Instance.PlaySFX("SYS_move");
			UpdatePage();
			return;
		}

		int old = CursorIndex;
		// omori menus have no wrapping
		// pressing left or right simply increments/decrements the index
		if (direction == Vector2.Left)
		{
			if (CursorIndex > 0)
				CursorIndex--;
			else if (Page > 0)
			{
				Page--;
				CursorIndex = 3;
				AudioManager.Instance.PlaySFX("SYS_move");
				UpdatePage();
				return;
			}
		}
		else if (direction == Vector2.Right)
		{
			if (CursorIndex < DisplayedSkills.Count - 1)
				CursorIndex++;
			else if (Page < MaxPage)
			{
				Page++;
				CursorIndex = 0;
				AudioManager.Instance.PlaySFX("SYS_move");
				UpdatePage();
				return;
			}
		}
		else if (direction == Vector2.Up)
		{
			if (CursorIndex > 1)
				CursorIndex -= 2;
		}
		else if (direction == Vector2.Down)
		{
			if (CursorIndex < 2 && DisplayedSkills.Count > 2)
				CursorIndex = Math.Min(CursorIndex + 2, DisplayedSkills.Count - 1);
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
		Skill selected = DisplayedSkills[CursorIndex];
		if (BattleManager.Instance.OnSelectSkill(selected))
			CursorSprite.StopBounce();
	}
}
