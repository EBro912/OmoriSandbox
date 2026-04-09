namespace OmoriSandbox.Modding;

internal struct JsonEnemyMod
{
    public string Name { get; set; }
    public string Atlas { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public JsonModAnimationData[] Animation { get; set; }
    public int HP { get; set; }
    public int Juice { get; set; }
    public int ATK { get; set; }
    public int DEF { get; set; }
    public int SPD { get; set; }
    public int LCK { get; set; }
    public int HIT { get; set; }
    public string[] InvalidStates { get; set; }
    public string[] EquippedSkills { get; set; }
    public string ObserveMultiSkill { get; set; }
    public string ObserveSingleSkill { get; set; }
    public JsonEnemyAIData[] AI { get; set; }

    internal bool Validate(ModLoadReport report)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            report.Error("enemies", "(unknown)", "Missing required field 'name'");
            return false;
        }
        if (string.IsNullOrWhiteSpace(Atlas))
        {
            report.Error("enemies", Name, "Missing required field 'atlas'");
            return false;
        }
        if (Atlas.Contains("..") || Atlas.Contains("://") || System.IO.Path.IsPathRooted(Atlas))
        {
            report.Error("enemies", Name, $"Invalid atlas path '{Atlas}' (path traversal not allowed)");
            return false;
        }
        if (Animation == null || Animation.Length == 0)
        {
            report.Error("enemies", Name, "Missing or empty 'animation' array");
            return false;
        }
        if (Width <= 0 || Height <= 0)
        {
            report.Error("enemies", Name, $"Invalid dimensions (Width={Width}, Height={Height}), must be > 0");
            return false;
        }
        if (AI == null)
            report.Warn("enemies", Name, "No AI data defined");
        return true;
    }
}

internal struct JsonEnemyAIData
{
    public string Emotion { get; set; }
    public JsonEnemyAIEntry[] Entries { get; set; }
}

internal struct JsonEnemyAIEntry
{
    public int Chance { get; set; }
    public string Skill { get; set; }
    public int? NumTargets { get; set; }
}