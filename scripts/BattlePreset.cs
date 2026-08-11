using System.Collections.Generic;
using Newtonsoft.Json;
using OmoriSandbox.Battle;

namespace OmoriSandbox;

internal class BattlePreset
{
    public GameModeType Type { get; set; } = GameModeType.Normal;
    [JsonRequired] public string Name { get; set; }
    public string Battleback { get; set; } = "battleback_vf_default";
    public string BGM { get; set; } = "battle_vf";
    public double BGMPitch { get; set; } = 1d;
    public double BGMLoopPoint { get; set; } = 0d;
    public int StartingEnergy { get; set; } = 3;
    public int FollowupTier { get; set; } = 1;
    // legacy, kept for reading old presets
    public bool BasilFollowups { get; set; } = false;
    public bool ShouldSerializeBasilFollowups() => false;
    // legacy, kept for reading old presets
    public bool BasilReleaseEnergy { get; set; } = false;
    public bool ShouldSerializeBasilReleaseEnergy() => false;
    public bool DisableDialogue { get; set; } = false;
    public bool DisableDamageNumbers { get; set; } = false;
    public bool CombinedBuffsDebuffs { get; set; } = false;
    public Dictionary<string, int> Items { get; set; } = [];

    [JsonRequired] public List<BattlePresetActor> Actors { get; set; } = [];

    public List<BattlePresetEnemy> Enemies { get; set; } = [];
    public List<BattlePresetBossRushStage> Stages { get; set; } = [];
}

internal class BattlePresetActor
{
    [JsonRequired] public string Name { get; set; }
    public int Level { get; set; } = 1;
    public string Weapon { get; set; } = "Baguette";
    public string Charm { get; set; } = "None";
    public string Emotion { get; set; } = "neutral";
    // legacy, kept for reading old presets
    public bool FollowupsDisabled { get; set; } = false;
    public bool ShouldSerializeFollowupsDisabled() => false;
    public string FollowupSet { get; set; } = null;
    public string[] Skills { get; set; } = ["OAttack", "", "", "", ""];
    [JsonRequired] public int Position { get; set; }
    public Stats AdjustedStats { get; set; } = new();
}

internal class BattlePresetEnemy
{
    [JsonRequired] public string Name { get; set; }
    public string Position { get; set; } = "Vector2(320, 240)";
    public string Emotion { get; set; } = "neutral";
    public double Layer { get; set; } = 0;
    public bool FallsOffScreen { get; set; } = true;
    public bool GrayscaleOnDefeat { get; set; } = false;
    public Stats AdjustedStats { get; set; } = new();
}

internal class BattlePresetBossRushStage
{
    [JsonRequired] public int StageNumber { get; set; }
    public string Battleback { get; set; } = "battleback_vf_default";
    public string BGM { get; set; } = "battle_vf";
    public double BGMPitch { get; set; } = 1d;
    public double BGMLoopPoint { get; set; } = 0d;
    public bool HealParty { get; set; } = false;
    public bool KeepEmotion { get; set; } = false;
    public bool KeepStatusEffects { get; set; } = false;
    [JsonRequired] public List<BattlePresetEnemy> Enemies { get; set; } = [];
}

internal enum GameModeType
{
    Normal,
    BossRush
}