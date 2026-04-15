using System.Collections.Generic;
using Godot;

namespace OmoriSandbox;

internal partial class FollowupBubbles : Node2D
{
    [Export] private FollowupDirection[] Directions;

    public void ShowBubbles(HashSet<InputDirection> disabledDirections = null)
    {
        foreach (FollowupDirection direction in Directions)
        {
            bool available = disabledDirections == null || !disabledDirections.Contains(direction.InputDir);
            direction.ShowBubble(available);
        }
    }

    public void HideBubbles()
    {
        foreach (FollowupDirection direction in Directions)
            direction.HideBubble();
    }

    public void HideBubblesExcept(InputDirection selected)
    {
        foreach (FollowupDirection direction in Directions)
        {
            if (direction.InputDir != selected)
                direction.HideBubble();
        }
    }
}
