using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class SkillMenu : Menu
{
	[Export] public Label[] SkillLabels;
	[Export] public Label CostText;
	private readonly List<Skill> Skills = [];
	private List<Vector2I> Positions = [new Vector2I(170, 435), new Vector2I(340, 435), new Vector2I(170, 457), new Vector2I(340, 457)];

	private Vector2I GridSize = new(2, 2);

	public void Populate(Actor actor)
	{
		Skills.Clear();
		foreach (Label l in SkillLabels)
			l.Text = "";
		int idx = 0;
		foreach (Skill s in actor.Skills.Values.Where(x => !x.Hidden))
		{
			if (idx > 3)
				break;
			SkillLabels[idx].Text = s.Name;
			Skills.Add(s);
			idx++;
		}
		CursorIndex = 0;
		CursorPositions = Positions.GetRange(0, Skills.Count);
		UpdateCursor();
		ShowSkillInfo();
	}
	
	private void ShowSkillInfo()
	{
		Skill s = Skills[CursorIndex];
		CostText.Text = s.Cost.ToString();
		BattleLogManager.Instance.ClearAndShowMessage($"{s.Name}\n{s.Description}");
	}

	protected override void MoveCursor(Vector2I direction)
	{
		int x = CursorIndex % 2;
		int y = CursorIndex / 2;
		x = (x + direction.X + GridSize.X) % GridSize.X;
		y = (y + direction.Y + GridSize.Y) % GridSize.Y;
		int newIndex = y * GridSize.X + x;
		newIndex = Mathf.Min(newIndex, Skills.Count - 1);
		CursorIndex = newIndex;
		UpdateCursor();
		ShowSkillInfo();
		AudioManager.Instance.PlaySFX("SYS_move");
	}

	protected override void OnSelect()
	{
		Skill selected = Skills[CursorIndex];
		BattleManager.Instance.OnSelectSkill(selected);
		MenuManager.Instance.ShowMenu(MenuState.None);
	}
}
