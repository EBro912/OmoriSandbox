using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class MenuManager : Node
{
	[Export] public Node2D PartyCommandsMenu;
	[Export] public Node2D BattleCommandsMenu;
	[Export] public TextureRect SkillMenu;
	[Export] public Label[] SkillLabels;
	[Export] public Label ValueLabel;
	[Export] public Sprite2D Cursor;

	private string CurrentMenu = "PartyCommand";
	private string CurrentSelection = "Fight";

	public static MenuManager Instance { get; private set; }

	public string DisplayedMenu => CurrentMenu;
	public string CursorSelection => CurrentSelection;

	private int SelectedSkill = 1;

	private List<Skill> CurrentSkills = [];

	public override void _EnterTree()
	{
		Instance = this;
	}

	public void ShowMenu(string menu)
	{
		switch (menu)
		{
			case "PartyCommand":
				PartyCommandsMenu.Visible = true;
				BattleCommandsMenu.Visible = false;
				SkillMenu.Visible = false;
				Cursor.Visible = true;
				MoveCursorTo("Fight");
				CurrentMenu = "PartyCommand";
				break;
			case "BattleCommand":
				PartyCommandsMenu.Visible = false;
				BattleCommandsMenu.Visible = true;
				SkillMenu.Visible = false;
				Cursor.Visible = true;
				MoveCursorTo("Attack");
				CurrentMenu = "BattleCommand";
				break;
			case "SkillMenu":
				PartyCommandsMenu.Visible = false;
				BattleCommandsMenu.Visible = false;
				SkillMenu.Visible = true;
				Cursor.Visible = true;
				MoveCursorTo("Skill1");
				ShowSkillInfo(1);
				CurrentMenu = "SkillMenu";
				break;
			case "None":
				PartyCommandsMenu.Visible = false;
				BattleCommandsMenu.Visible = false;
				SkillMenu.Visible = false;
				Cursor.Visible = false;
				CurrentMenu = "None";
				break;
		}
	}


	public override void _Input(InputEvent @event)
	{
		if (Input.IsActionJustPressed("MenuDown") || Input.IsActionJustPressed("MenuUp"))
		{
			if (CurrentMenu == "PartyCommand")
			{
				AudioManager.Instance.PlaySFX("SYS_move");
				if (CurrentSelection == "Fight")
					MoveCursorTo("Run");
				else
					MoveCursorTo("Fight");
				return;
			}
			if (CurrentMenu == "BattleCommand")
			{
				AudioManager.Instance.PlaySFX("SYS_move");
				if (CurrentSelection == "Attack")
				{
					MoveCursorTo("Snack");
					return;
				}
				if (CurrentSelection == "Snack")
				{
					MoveCursorTo("Attack");
					return;
				}
				if (CurrentSelection == "Skill")
				{
					MoveCursorTo("Toy");
					return;
				}
				if (CurrentSelection == "Toy")
				{
					MoveCursorTo("Skill");
					return;
				}
			}
			if (CurrentMenu == "SkillMenu")
			{
				AudioManager.Instance.PlaySFX("SYS_move");
				if (CurrentSelection == "Skill1")
				{
					MoveCursorTo("Skill2");
					ShowSkillInfo(2);
					return;
				}
				if (CurrentSelection == "Skill2")
				{
					MoveCursorTo("Skill1");
					ShowSkillInfo(1);
					return;
				}
				if (CurrentSelection == "Skill3")
				{
					MoveCursorTo("Skill4");
					ShowSkillInfo(4);
					return;
				}
				if (CurrentSelection == "Skill4")
				{
					MoveCursorTo("Skill3");
					ShowSkillInfo(3);
					return;
				}
			}
		}

		if (Input.IsActionJustPressed("MenuLeft") || Input.IsActionJustPressed("MenuRight"))
		{
			if (CurrentMenu == "BattleCommand")
			{
				AudioManager.Instance.PlaySFX("SYS_move");
				if (CurrentSelection == "Attack")
				{
					MoveCursorTo("Skill");
					return;
				}
				if (CurrentSelection == "Skill")
				{
					MoveCursorTo("Attack");
					return;
				}
				if (CurrentSelection == "Snack")
				{
					MoveCursorTo("Toy");
					return;
				}
				if (CurrentSelection == "Toy")
				{
					MoveCursorTo("Snack");
					return;
				}
			}
			if (CurrentMenu == "SkillMenu")
			{
				AudioManager.Instance.PlaySFX("SYS_move");
				if (CurrentSelection == "Skill1")
				{
					MoveCursorTo("Skill3");
					ShowSkillInfo(3);
					return;
				}
				if (CurrentSelection == "Skill3")
				{
					MoveCursorTo("Skill1");
					ShowSkillInfo(1);
					return;
				}
				if (CurrentSelection == "Skill2")
				{
					MoveCursorTo("Skill4");
					ShowSkillInfo(4);
					return;
				}
				if (CurrentSelection == "Skill4")
				{
					MoveCursorTo("Skill2");
					ShowSkillInfo(2);
					return;
				}
			}
		}
	}

	public void PopulateSkillMenu(Actor actor)
	{
		CurrentSkills.Clear();
		foreach (Label l in SkillLabels)
			l.Text = "";
		int idx = 0;
		foreach (Skill skill in actor.Skills.Values.Where(x => !x.Hidden))
		{
			if (idx > 3)
				return;
			SkillLabels[idx].Text = skill.Name;
			CurrentSkills.Add(skill);
			idx++;
		}	
	}

	public Skill GetSelectedSkill()
	{
		return CurrentSkills[SelectedSkill - 1];
	}

	private void ShowSkillInfo(int idx)
	{
		SelectedSkill = idx;
		if (idx <= CurrentSkills.Count)
		{
			Skill s = CurrentSkills[idx - 1];
			ValueLabel.Text = s.Cost.ToString();
			GameManager.Instance.ClearAndMessageBattleLog($"{s.Name}\n{s.Description}");
		}
	}

	private void MoveCursorTo(string position)
	{
		CurrentSelection = position;
		switch (position)
		{
			case "Fight":
				Cursor.Position = new Vector2(250, 410);
				break;
			case "Run":
				Cursor.Position = new Vector2(250, 450);
				break;
			case "Attack":
				Cursor.Position = new Vector2(165, 410);
				break;
			case "Snack":
				Cursor.Position = new Vector2(165, 450);
				break;
			case "Skill":
				Cursor.Position = new Vector2(350, 410);
				break;
			case "Toy":
				Cursor.Position = new Vector2(350, 450);
				break;
			case "Skill1":
				Cursor.Position = new Vector2(170, 435);
				break;
			case "Skill2":
				Cursor.Position = new Vector2(170, 457);
				break;
			case "Skill3":
				Cursor.Position = new Vector2(340, 435);
				break;
			case "Skill4":
				Cursor.Position = new Vector2(340, 457);
				break;
		}
	}
}
