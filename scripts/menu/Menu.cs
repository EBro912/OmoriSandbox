using System.Collections.Generic;
using Godot;

namespace OmoriSandbox.Menu;

internal abstract partial class Menu : Control
{
	[Export] protected CursorBounce CursorSprite;
	protected List<string> Options = [];
	protected List<Vector2I> CursorPositions = [];
	public int CursorIndex { get; protected set; } = 0;
	protected bool Empty = false;
	protected Tween Tween;

	protected abstract Vector2 OpenPosition { get; }
	protected abstract Vector2 ClosedPosition { get; }

	public void OnInput(Vector2I direction)
	{
		// kind of a wacky way to do this but I didn't feel like making an enum when I can just use the struct for directions
		if (direction == Vector2I.Zero)
			OnSelect();
		else
			MoveCursor(direction);
	}

	protected virtual void MoveCursor(Vector2I direction) {}

	protected virtual void UpdateCursor()
	{
		CursorSprite.Position = CursorPositions[CursorIndex];
	}

	public Vector2I GetCursorPosition()
	{
		if (CursorPositions.Count > CursorIndex)
			return CursorPositions[CursorIndex];
		return Vector2I.Zero;
	}

	protected abstract void OnSelect();
	public virtual void OnOpen(SelectionMemory memory) 
	{
		CursorIndex = 0;
		Show();
		UpdateCursor();
	}

	protected virtual bool ShouldCloseVisually(MenuState newState)
	{
		return true;
	}

	public void MoveUp(bool immediate)
	{
		Visible = true;
		Tween?.Kill();
		if (immediate)
		{
			Position = OpenPosition;
		}
		else
		{
			Tween = CreateTween();
			Tween.TweenProperty(this, "position", OpenPosition, 0.2f).SetTrans(Tween.TransitionType.Sine);
		}
	}

    public void MoveDown(MenuState newState, bool immediate)
    {
	    if (ShouldCloseVisually(newState))
	    {
		    Tween?.Kill();
		    if (immediate)
		    {
			    Position = ClosedPosition;
			    Visible = false;
		    }
		    else
		    {
			    Tween = CreateTween();
			    Tween.TweenProperty(this, "position", ClosedPosition, 0.2f).SetTrans(Tween.TransitionType.Sine);
			    Tween.TweenCallback(Callable.From(() => Visible = false));
		    }
	    }
    }
}
