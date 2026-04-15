using System;
using System.Threading.Tasks;

namespace OmoriSandbox;

/// <summary>
/// Provides shorthand methods for waiting for a certain amount of time.
/// </summary>
/// <remarks>
/// This is a more Godot-friendly way of <see cref="Task.Delay(int)"/> and should be used instead in most cases.
/// </remarks>
public static class Wait
{
    /// <summary>
    /// Waits for the given number of <paramref name="seconds"/>.
    /// </summary>
    /// <param name="seconds">The amount of seconds to wait for.</param>
    public static async Task Seconds(float seconds)
    {
        await GameManager.Instance.Wait(seconds);
    }

    /// <summary>
    /// Waits for the given number of <paramref name="milliseconds"/>.
    /// </summary>
    /// <param name="milliseconds">The amount of milliseconds to wait for.</param>
    public static async Task Milliseconds(int milliseconds)
    {
        await Seconds(milliseconds / 1000f);
    }

    /// <summary>
    /// Waits for the amount of time specified in the <paramref name="timeSpan"/>.
    /// </summary>
    /// <param name="timeSpan">The amount of seconds to wait for as a <see cref="TimeSpan"/></param>
    public static async Task Timespan(TimeSpan timeSpan)
    {
        await Seconds((float)timeSpan.TotalSeconds);
    }
}