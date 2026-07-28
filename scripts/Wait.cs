using System;
using System.Threading.Tasks;
using Godot;

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
    /// <param name="timeSpan">The amount of seconds to wait for as a <see cref="TimeSpan"/>.</param>
    public static async Task Timespan(TimeSpan timeSpan)
    {
        await Seconds((float)timeSpan.TotalSeconds);
    }

    /// <summary>
    /// Waits for the given number of RPGMaker game frames (60 frames per second),
    /// matching the RPGMaker "wait X" command.
    /// </summary>
    /// <remarks>
    /// This is a time conversion (<c>frames / 60</c> seconds), not an engine-frame-locked wait. Useful for porting RPGMaker skills that utilize waits.
    /// </remarks>
    /// <param name="frames">The number of 60fps game frames to wait for.</param>
    public static async Task Frames(int frames)
    {
        await Seconds(frames / 60f);
    }

    /// <summary>
    /// Waits for the given number of battle animation frames, matching the fixed 15fps
    /// animation clock of <see cref="Animation.AnimationManager"/>,
    /// like the Yanfly "animation wait X" command.
    /// </summary>
    /// <remarks>
    /// This is a time conversion (<c>frames / 15</c> seconds), it does not synchronize with the animation clock itself. Useful for porting RPGMaker skills that utilize the 'animation wait' notetag.
    /// </remarks>
    /// <param name="frames">The number of 15fps animation frames to wait for.</param>
    public static async Task AnimationFrames(int frames)
    {
        await Seconds(frames / Animation.AnimationManager.FPS);
    }
}
