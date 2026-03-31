using Godot;
using OmoriSandbox.Editor;

namespace OmoriSandbox.Menu;
internal partial class PartyMenu : Menu, ISkinnableMenu
{
	[Export] private Sprite2D FightSprite;
	[Export] private Sprite2D RunSprite;
	
	protected override Vector2 OpenPosition => new(320, 480);
	protected override Vector2 ClosedPosition => new(320, 575);
	
	public override void _Ready()
	{
		Options = ["Fight", "Run"];
		CursorPositions = [new Vector2I(-105, -72), new Vector2I(-105, -27)];
	}

	public void SetSkinMode(MenuSkinMode mode)
	{
		switch (mode)
		{
			case MenuSkinMode.Dreamworld:
				FightSprite.RegionRect = new Rect2(653, 376, 362, 40);
				RunSprite.RegionRect = new Rect2(653, 418, 362, 40);
				break;
			case MenuSkinMode.Faraway:
				FightSprite.RegionRect = new Rect2(653, 459, 362, 39);
				RunSprite.RegionRect = new Rect2(653, 499, 362, 39);
				break;
			case MenuSkinMode.Blackspace:
				FightSprite.RegionRect = new Rect2(653, 294, 362, 40);
				RunSprite.RegionRect = new Rect2(653, 355, 362, 39);
				break;
			default:
				GD.PrintErr("Unknown MenuSkinMode:	" + mode);
				break;
		}
	}

	protected override void MoveCursor(Vector2I direction)
	{
		int old = CursorIndex;
		CursorIndex = (CursorIndex + direction.Y + Options.Count) % Options.Count;
		UpdateCursor();
		// only play a sound if the cursor actually moved
		if (old != CursorIndex)
			AudioManager.Instance.PlaySFX("SYS_move");
	}

    public override void OnOpen(SelectionMemory memory)
    {
        CursorSprite.StartBounce();
		base.OnOpen(memory);
    }

    private bool WaitForSecondRun = false;

    protected override void OnSelect()
    {
	    CursorSprite.StopBounce();
	    AudioManager.Instance.PlaySFX("SYS_select");
	    if (CursorIndex == 0)
	    {
		    WaitForSecondRun = false;
		    BattleManager.Instance.OnFightSelected();
	    }
	    else
	    {
		    if (SettingsMenuManager.Instance.PreventAccidentalRun && !WaitForSecondRun)
		    {
			    WaitForSecondRun = true;
			    CursorSprite.StartBounce();
			    return;
		    }

		    WaitForSecondRun = false;
		    BattleManager.Instance.Reset();
		    MainMenuManager.Instance.ReturnToTitle();
	    }
    }
}
