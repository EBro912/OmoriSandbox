using Godot;
using OmoriSandbox.Editor;

namespace OmoriSandbox.Menu;
internal partial class PartyMenu : Menu
{
	public override void _Ready()
	{
		Options = ["Fight", "Run"];
		CursorPositions = [new Vector2I(-125, -20), new Vector2I(-125, 20)];
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
			BattleManager.Instance.Reset();
			MainMenuManager.Instance.ReturnToTitle();
		}
	}

    public override void MoveUp(bool immediate)
    {
        Tween?.Kill();
        if (immediate)
        {
            Position = new Vector2(Position.X, 429);
        }
        else
        {
            Tween = CreateTween();
            Tween.TweenProperty(this, "position", new Vector2(Position.X, 429), 0.2f).SetTrans(Tween.TransitionType.Sine);
        }
    }

    public override void MoveDown(MenuState newState, bool immediate, bool noHide = false)
    {
        Tween?.Kill();
        if (immediate)
        {
            Position = new Vector2(Position.X, 529);
			Visible = noHide;
        }
        else
        {
            Tween = CreateTween();
            Tween.TweenProperty(this, "position", new Vector2(Position.X, 529), 0.2f).SetTrans(Tween.TransitionType.Sine);
			Tween.TweenCallback(Callable.From(() => Visible = noHide));
        }
    }
}
