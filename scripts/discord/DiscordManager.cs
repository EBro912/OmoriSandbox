using Godot;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using OmoriSandbox;

namespace Discord;

internal class DiscordManager
{
    private Discord DiscordSDK;
    private Activity Activity;
    private bool DiscordDisabled = false;
    private static bool ResolverRegistered = false;

    public DiscordManager()
    {
        try
        {
            RegisterNativeResolver();
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
        catch (Exception ex)
        {
            GD.PushWarning($"Failed to initialize Discord SDK, disabling Discord integration! ({ex.Message})");
            DiscordDisabled = true;
        }
    }

    // resolve the Discord SDK when running in the editor
    private static void RegisterNativeResolver()
    {
        if (ResolverRegistered) return;
        ResolverRegistered = true;
        NativeLibrary.SetDllImportResolver(typeof(DiscordManager).Assembly, (name, assembly, _) =>
        {
            if (name != Constants.DllName)
                return IntPtr.Zero;

            string[] candidates =
            [
                "libdiscord_game_sdk.so", "discord_game_sdk.so",
                "discord_game_sdk.dll",
                "libdiscord_game_sdk.dylib", "discord_game_sdk.dylib"
            ];
            string[] dirs =
            [
                Path.GetDirectoryName(assembly.Location),
                OS.GetExecutablePath().GetBaseDir(),
                OS.HasFeature("editor") ? ProjectSettings.GlobalizePath("res://") : null
            ];
            foreach (string dir in dirs)
            {
                if (string.IsNullOrEmpty(dir))
                    continue;
                foreach (string candidate in candidates)
                {
                    string path = Path.Combine(dir, candidate);
                    if (File.Exists(path) && NativeLibrary.TryLoad(path, out IntPtr handle))
                        return handle;
                }
            }

            // fall back to the runtime's default probing
            return IntPtr.Zero;
        });
    }

    public void Tick()
    {
        if (DiscordDisabled) return;
        try
        {
            DiscordSDK.RunCallbacks();
        }
        catch (ResultException ex)
        {
            GD.PushWarning($"Ran into an exception while running Discord SDK ({ex.Message}). Disabling...");
            DiscordDisabled = true;
            DiscordSDK.Dispose();
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
        try
        {
            // best-effort clear the activity on shutdown to prevent the activity from persisting in certain cases
            // such as the stop button in the editor
            bool acknowledged = false;
            DiscordSDK.GetActivityManager().ClearActivity(_ => acknowledged = true);
            // attempt a few times until acknowledged
            for (int i = 0; i < 20 && !acknowledged; i++)
            {
                DiscordSDK.RunCallbacks();
                OS.DelayMsec(10);
            }
        }
        catch (ResultException)
        {
            // Discord isn't running or went away mid-shutdown, nothing to clear
        }
        DiscordSDK.Dispose();
        DiscordDisabled = true;
    }
}