using System.Collections.Generic;
using Godot;
using OmoriSandbox.Battle;

namespace OmoriSandbox;

internal partial class FollowupBubbles : Node2D
{
    [Export] private FollowupDirection[] Directions;

    // apply graphics from a followup entry
    // in base game, followup graphics are hardcoded based on actor position
    internal void ApplySet(FollowupSet set, int position)
    {
        foreach (FollowupDirection direction in Directions)
        {
            FollowupInput? role = FollowupSets.InputFor(direction.InputDir, position);
            if (role != null && set.Entries.TryGetValue(role.Value, out FollowupEntry entry))
                direction.Apply(entry);
            else
                direction.Disable();
        }
    }

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
