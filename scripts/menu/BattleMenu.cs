using System;
using Godot;

namespace OmoriSandbox.Menu;

internal partial class BattleMenu : Menu
{
	private Vector2I GridSize = new(2, 2);

	public override void _Ready()
	{
		Options = ["Attack", "Skill", "Snack", "Toy"];
		CursorPositions = [new Vector2I(-155, -20), new Vector2I(35, -20), new Vector2I(-155, 20), new Vector2I(35, 20)];
	}

    public override void OnOpen(SelectionMemory memory)
    {
		if (memory.SavedState == MenuState.Battle)
			CursorIndex = memory.SavedIndex;
		else if (memory.SavedState == MenuState.Skill)
			CursorIndex = 1;
		else if (memory.SavedState == MenuState.Snack)
			CursorIndex = 2;
		else if (memory.SavedState == MenuState.Toy)
			CursorIndex = 3;
        else
			CursorIndex = 0;
		CursorSprite.StartBounce();
		UpdateCursor();
		Show();
    }

	protected override void MoveCursor(Vector2I direction)
	{
		int old = CursorIndex;
		// omori menus have no wrapping
		// pressing left or right simply increments/decrements the index
		if (direction == Vector2.Left)
			CursorIndex = Math.Max(CursorIndex - 1, 0);
		else if (direction == Vector2.Right)
			CursorIndex = Math.Min(CursorIndex + 1, CursorPositions.Count - 1);
		else if (direction == Vector2.Up)
		{
			if (CursorIndex > 1)
				CursorIndex -= 2;
		}
		else if (direction == Vector2.Down)
		{
			if (CursorIndex < 2)
				CursorIndex += 2;
		}
		UpdateCursor();
		// only play a sound if the cursor actually moved
		if (old != CursorIndex)
			AudioManager.Instance.PlaySFX("SYS_move");
	}

	protected override void OnSelect()
	{
		CursorSprite.StopBounce();
		switch (Options[CursorIndex])
		{
			case "Attack":
				BattleManager.Instance.OnSelectAttack();
				break;
			case "Skill":
				BattleManager.Instance.OnSelectNotAttack(MenuState.Skill);
				break;
			case "Snack":
				BattleManager.Instance.OnSelectNotAttack(MenuState.Snack);
				break;
			case "Toy":
				BattleManager.Instance.OnSelectNotAttack(MenuState.Toy);
				break;
		}
		AudioManager.Instance.PlaySFX("SYS_select");
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
		// don't move down the battle menu for these three states
		// recreates the "slide over" effect from the original game
		if (newState is MenuState.Skill or MenuState.Snack or MenuState.Toy)
			return;	

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
