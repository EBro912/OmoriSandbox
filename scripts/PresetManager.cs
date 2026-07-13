using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Newtonsoft.Json;
using OmoriSandbox.Modding;

namespace OmoriSandbox;

internal partial class PresetManager : Node
{
    public static PresetManager Instance { get; private set; }

    private readonly Dictionary<string, BattlePreset> Presets = [];
    
    public override void _EnterTree()
    {
        Instance = this;
        
        CreatePresetDirIfMissing();
        
        string[] presets = DirAccess.GetFilesAt("user://presets");
        foreach (string file in presets)
        {
            if (file.EndsWith(".json"))
            {
                using FileAccess f = FileAccess.Open("user://presets/" + file, FileAccess.ModeFlags.Read);
                if (f == null)
                {
                    GD.PrintErr($"Failed to open file {file}:\n {FileAccess.GetOpenError()}");
                    continue;
                }
                BattlePreset preset;
                try
                {
                    preset = JsonConvert.DeserializeObject<BattlePreset>(f.GetAsText());
                }
                catch (KeyNotFoundException ek)
                {
                    GD.PrintErr($"Failed to parse preset {file} due to missing key:\n{ek}");
                    continue;
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"Failed to parse preset {file} due to an error:\n{ex}");
                    continue;
                }

                if (preset == null)
                    continue;

                if (!Presets.TryAdd(preset.Name, preset))
                {
                    GD.PrintErr($"Failed to load preset {preset.Name}, a preset with this name already exists.");
                }
            }
        }
    }

    public void LoadModdedPreset(string file, ModLoadReport report)
    {
        using FileAccess f = FileAccess.Open(file, FileAccess.ModeFlags.Read);
        if (f == null)
        {
            report.Error("presets", file.GetFile(), FileAccess.GetOpenError().ToString());
            report.CountSkipped();
            return;
        }
        BattlePreset preset;
        try
        {
            preset = JsonConvert.DeserializeObject<BattlePreset>(f.GetAsText());
        }
        catch (KeyNotFoundException ek)
        {
            report.Error("presets", file.GetFile(),$"Missing JSON key: {ek}");
            report.CountSkipped();
            return;
        }
        catch (Exception ex)
        {
            report.Error("presets", file.GetFile(),$"Generic Error: {ex}");
            report.CountSkipped();
            return;
        }
        
        if (preset == null)
            return;

        if (!Presets.TryAdd(preset.Name, preset))
        {
            report.Error("presets", file.GetFile(),"A preset with this name already exists.");
            report.CountSkipped();
            return;
        }
        report.CountLoaded();
    }

    public void SavePreset(BattlePreset preset)
    {
        CreatePresetDirIfMissing();
        string result = JsonConvert.SerializeObject(preset, Formatting.Indented);
        using FileAccess file = FileAccess.Open("user://presets/" + preset.Name + ".json", FileAccess.ModeFlags.Write);
        file?.StoreString(result);
        Presets[preset.Name] = preset;
    }

    public bool RemovePreset(string name)
    {
        using DirAccess access = DirAccess.Open("user://presets");
        if (access == null)
        {
            GD.PrintErr("Failed to open presets directory");
            return false;
        }
        Error err = access.Remove(name + ".json");
        if (err == Error.Ok)
        {
            Presets.Remove(name);
            return true;
        }

        GD.PrintErr("Failed to delete preset " + name + " due to error: " + err);
        return false;
    }

    private void CreatePresetDirIfMissing()
    {
        if (DirAccess.DirExistsAbsolute("user://presets")) return;
        using DirAccess access = DirAccess.Open("user://");
        access.MakeDir("presets");
        GD.Print("Created user://presets directory");
    }

    public bool PresetExists(string name)
    {
        return Presets.ContainsKey(name);
    }

    public bool TryGetPreset(string name, out BattlePreset preset)
    {
        return Presets.TryGetValue(name, out preset);
    }

    public IEnumerable<BattlePreset> GetAllPresets()
    {
        return Presets.OrderBy(x => x.Key).Select(x => x.Value);
    }
}