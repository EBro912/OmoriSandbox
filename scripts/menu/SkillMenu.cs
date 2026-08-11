using System.Collections.Generic;
using System.Linq;
using Godot;
using OmoriSandbox.Actors;
using OmoriSandbox.Battle;

namespace OmoriSandbox.Menu;

internal partial class SkillMenu : PagedMenu
{
	[Export] public AutofitLabel[] SkillLabels;
	[Export] public Label CostText;
	[Export] private Sprite2D PageUpSprite;
	[Export] private Sprite2D PageDownSprite;
	private readonly List<Skill> Skills = [];
	private List<Skill> DisplayedSkills = [];

	private PartyMember Actor;

	protected override Vector2 OpenPosition => new(138, 384);
	protected override Vector2 ClosedPosition => new(138, 490);

	protected override int TotalCount => Skills.Count;
	protected override int DisplayedCount => DisplayedSkills.Count;

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

	protected override void UpdatePage()
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
		ShowInfo();
	}

	protected override void ShowInfo()
	{
		if (Empty) return;
		Skill s = DisplayedSkills[CursorIndex];
		CostText.Text = s.Cost(Actor).ToString();
		BattleLogManager.Instance.ClearAndShowMessage($"[font_size=28]{s.Name}\n[font_size=20]{s.Description.Replace("[actor]", Actor.Name.ToUpper()).Replace("[first]", BattleManager.Instance.GetPartyMember(0).Name.ToUpper())}");
	}

	protected override void OnSelect()
	{
		if (Empty) return;
		Skill selected = DisplayedSkills[CursorIndex];
		if (BattleManager.Instance.OnSelectSkill(selected))
			CursorSprite.StopBounce();
	}
}
