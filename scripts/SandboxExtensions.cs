using System.Threading.Tasks;
using Godot;

namespace OmoriSandbox.Extensions;

/// <summary>
/// Extension methods to help make certain things easier.
/// </summary>
public static class SandboxExtensions
{
    /// <summary>
    /// Retrieves the index of the specified item in the OptionButton.
    /// </summary>
    /// <returns>The index of the item, otherwise -1.</returns>
    public static int GetItemIndex(this OptionButton button, string item)
    {
        for (int i = 0; i < button.GetItemCount(); i++)
        {
            if (button.GetItemText(i) == item)
                return i;
        }
        return -1;
    }

    /// <summary>
    /// Creates a timer that waits for the given number of seconds.
    /// </summary>
    /// <remarks>
    /// If you are working outside of a Node, you can use <see cref="GameManager.Wait(float)"/> instead.
    /// </remarks>
    /// <param name="node">The node to attach the timer to.</param>
    /// <param name="seconds">The amount of seconds to wait for.</param>
    public static async Task Wait(this Node node, float seconds)
    {
        await node.ToSignal(node.GetTree().CreateTimer(seconds), SceneTreeTimer.SignalName.Timeout);
    }
}