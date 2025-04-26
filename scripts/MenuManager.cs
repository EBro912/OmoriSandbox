using Godot;

public partial class MenuManager : Node
{
	[Export] public Node2D PartyCommandsMenu;
	[Export] public Node2D BattleCommandsMenu;
	[Export] public Sprite2D Cursor;

	private string CurrentMenu = "PartyCommand";
	private string CurrentSelection = "Fight";

	public static MenuManager Instance { get; private set; }

	public string DisplayedMenu => CurrentMenu;
	public string CursorSelection => CurrentSelection;

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
				Cursor.Visible = true;
				MoveCursorTo("Fight");
				CurrentMenu = "PartyCommand";
				break;
			case "BattleCommand":
				PartyCommandsMenu.Visible = false;
				BattleCommandsMenu.Visible = true;
				Cursor.Visible = true;
				MoveCursorTo("Attack");
				CurrentMenu = "BattleCommand";
				break;
			case "None":
				PartyCommandsMenu.Visible = false;
				BattleCommandsMenu.Visible = false;
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
				AudioManager.Instance.PlaySFX("Move");
				if (CurrentSelection == "Fight")
					MoveCursorTo("Run");
				else
					MoveCursorTo("Fight");
				return;
			}
			if (CurrentMenu == "BattleCommand")
			{
				AudioManager.Instance.PlaySFX("Move");
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
		}

		if (Input.IsActionJustPressed("MenuLeft") || Input.IsActionJustPressed("MenuRight"))
		{
			if (CurrentMenu == "BattleCommand")
			{
				AudioManager.Instance.PlaySFX("Move");
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
		}
	}
}
