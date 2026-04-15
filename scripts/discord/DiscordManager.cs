using Godot;
using System;
using OmoriSandbox;

namespace Discord;

internal class DiscordManager
{
    private Discord DiscordSDK;
    private Activity Activity;
    private bool DiscordDisabled = false;
    public DiscordManager()
    {
        try
        {
            DiscordSDK = new Discord(1410108043525488812, (ulong)CreateFlags.NoRequireDiscord);
            Activity = new Activity()
            {
                Details = "On the Main Menu",
                Timestamps =
                {
                    Start = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                },
                Assets =
                {
                    LargeImage = "icon",
                    LargeText = GameManager.Version
                },
                Instance = false
            };

            DiscordSDK.GetActivityManager().UpdateActivity(Activity, (result) =>
            {
                if (result == Result.Ok)
                {
                    GD.Print("Initialized Discord Activity");
                }
                else
                {
                    GD.PushWarning("Failed to init Discord Activity, got result: " + result);
                }
            });
        }
        catch
        {
            GD.PushWarning("Failed to initialize Discord SDK, disabling Discord integration!");
            DiscordDisabled = true;
        }
    }

    public void Tick()
    {
        if (DiscordDisabled) return;
        try
        {
            DiscordSDK.RunCallbacks();
        }
        catch (ResultException)
        {
            GD.PushWarning("Ran into an exception while running Discord SDK. Disabling...");
            DiscordDisabled = true;
        }
    }

    public void SetMainMenu()
    {
        if (DiscordDisabled) return;
        Activity.Details = "On the Main Menu";
        DiscordSDK.GetActivityManager().UpdateActivity(Activity, (_) => { });
    }

    public void SetEditingPreset(GameModeType mode)
    {
        if (DiscordDisabled) return;
        Activity.Details = $"Editing a {(mode is GameModeType.Normal ? "Normal" : "Boss Rush")} Preset";
        DiscordSDK.GetActivityManager().UpdateActivity(Activity, (_) => { });
    }

    public void SetBattling(int enemies)
    {
        if (DiscordDisabled) return;
        Activity.Details = enemies == 1 ? "Battling 1 Enemy" : $"Battling {enemies} Enemies";
        DiscordSDK.GetActivityManager().UpdateActivity(Activity, (_) => { });
    }

    public void Shutdown()
    {
        if (DiscordDisabled) return;
        DiscordSDK.Dispose();
    }
}