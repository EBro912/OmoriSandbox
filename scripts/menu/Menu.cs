using System.Collections.Generic;
using Godot;

public abstract partial class Menu : Sprite2D
{
    [Export] public Sprite2D CursorSprite;
    protected List<string> Options = [];
    protected List<Vector2I> CursorPositions = [];
    protected int CursorIndex = 0;

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
        if (CursorPositions.Count > CursorIndex)
            CursorSprite.Position = CursorPositions[CursorIndex];
    }

    public Vector2I GetCursorPosition()
    {
        if (CursorPositions.Count > CursorIndex)
            return CursorPositions[CursorIndex];
        return Vector2I.Zero;
    }

    protected abstract void OnSelect();
    public virtual void OnOpen() { UpdateCursor(); Show(); }
    public virtual void OnClose() { Hide(); }
}