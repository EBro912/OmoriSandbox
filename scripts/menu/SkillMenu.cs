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
	[Export] private Label[] CostValues;
	[Export] private Sprite2D[] CostIcons;
	[Export] private Texture2D JuiceIcon;
	[Export] private Texture2D HPIcon;
	[Export] private Texture2D EnergyIcon;
	[Export] private Sprite2D PageUpSprite;
	[Export] private Sprite2D PageDownSprite;
	private readonly List<Skill> Skills = [];
	private List<Skill> DisplayedSkills = [];

	private PartyMember Actor;
	private Label CostHeader => CostText.GetParent<Label>();
	private Vector2 CostValuePosition;

	protected override Vector2 OpenPosition => new(140, 385);
	protected override Vector2 ClosedPosition => new(140, 480);

	protected override int TotalCount => Skills.Count;
	protected override int DisplayedCount => DisplayedSkills.Count;

	public override void _Ready()
	{
		base._Ready();
		CostValuePosition = CostText.Position;
	}

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
		CostText.GetParent<Label>().Visible = !Empty;
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
			BattleLogManager.Instance.ClearBattleLog();
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
			bool usable = BattleManager.Instance.GetInputUseFailure(Actor, DisplayedSkills[i]) == null;
			if (DisplayedSkills[i].MenuFontColor(usable) is Color color)
				SkillLabels[i].AddThemeColorOverride("font_color", color);
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
		UpdateCostHeader(s);
		BattleLogManager.Instance.ClearAndShowDescription($"[font_size=28]{s.Name}\n[font_size=20]{s.Description.Replace("[actor]", Actor.Name.ToUpper()).Replace("[first]", BattleManager.Instance.GetPartyMember(0).Name.ToUpper())}");
	}

	private void UpdateCostHeader(Skill skill)
	{
		int cooldown = BattleManager.Instance.GetInputCooldown(Actor, skill);
		if (cooldown > 0)
		{
			CostHeader.Text = "COOLDOWN:";
			CostText.Text = cooldown.ToString();
			CostText.HorizontalAlignment = HorizontalAlignment.Left;
			float width = CostHeader.GetThemeFont("font").GetStringSize("COOLDOWN: ", fontSize: CostHeader.GetThemeFontSize("font_size")).X;
			CostText.Position = new Vector2(width, CostValuePosition.Y);
			for (int i = 0; i < CostValues.Length; i++)
			{
				CostValues[i].Visible = i == 0;
				CostIcons[i].Visible = false;
			}
			return;
		}
		CostHeader.Text = "COST:";
		CostText.HorizontalAlignment = HorizontalAlignment.Right;
		CostText.Position = CostValuePosition;
		List<(int Amount, Texture2D Icon)> costs = [];
		int juice = skill.Cost(Actor);
		if (juice > 0)
			costs.Add((juice, JuiceIcon));
		int hp = skill.HPCost(Actor);
		if (hp > 0)
			costs.Add((hp, HPIcon));
		int energy = skill.EnergyCost(Actor);
		if (energy > 0)
			costs.Add((energy, EnergyIcon));
		if (costs.Count == 0)
			costs.Add((juice, JuiceIcon));
		for (int i = 0; i < CostValues.Length; i++)
		{
			bool shown = i < costs.Count;
			CostValues[i].Visible = shown;
			CostIcons[i].Visible = shown;
			if (!shown)
				continue;
			CostValues[i].Text = costs[i].Amount.ToString();
			CostIcons[i].Texture = costs[i].Icon;
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
