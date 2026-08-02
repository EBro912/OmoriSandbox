using System.IO;
using System.Linq;

namespace OmoriSandbox.Modding;

internal struct JsonActorMod
{
    public string Name { get; set; }
    public string Atlas { get; set; }
    public JsonModAnimationData[] Animation { get; set; }
    public int[] HP { get; set; }
    public int[] Juice { get; set; }
    public int[] ATK { get; set; }
    public int[] DEF { get; set; }
    public int[] SPD { get; set; }
    public int LCK { get; set; }
    public string[] InvalidStates { get; set; }
    public bool RealWorld { get; set; }
    public bool PlotArmor { get; set; }
    public string[] EquippableWeapons { get; set; }

    internal bool Validate(ModLoadReport report)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            report.Error("actors", "(unknown)", "Missing required field 'name'");
            return false;
        }
        if (string.IsNullOrWhiteSpace(Atlas))
        {
            report.Error("actors", Name, "Missing required field 'atlas'");
            return false;
        }
        if (Atlas.Contains("..") || Atlas.Contains("://") || Path.IsPathRooted(Atlas))
        {
            report.Error("actors", Name, $"Invalid atlas path '{Atlas}' (path traversal not allowed)");
            return false;
        }
        if (Animation == null || Animation.Length == 0)
        {
            report.Error("actors", Name, "Missing or empty 'animation' array");
            return false;
        }
        if (HP == null || Juice == null || ATK == null || DEF == null || SPD == null)
        {
            report.Error("actors", Name, "One or more stat arrays (HP, Juice, ATK, DEF, SPD) are missing");
            return false;
        }
        if (HP.Length == 0 || Juice.Length == 0 || ATK.Length == 0 || DEF.Length == 0 || SPD.Length == 0)
        {
            report.Error("actors", Name, "One or more stat arrays are empty");
            return false;
        }
        if (HP.Length != Juice.Length || HP.Length != ATK.Length || HP.Length != DEF.Length || HP.Length != SPD.Length)
        {
            report.Error("actors", Name, $"Stat array length mismatch (HP={HP.Length}, Juice={Juice.Length}, ATK={ATK.Length}, DEF={DEF.Length}, SPD={SPD.Length})");
            return false;
        }
        // check for the presence of the four required animations
        foreach (string required in new[] { "neutral", "hurt", "toast", "victory" })
        {
            if (Animation.All(a => a.Emotion != required))
            {
                report.Error("actors", Name, $"Missing required animation '{required}'");
                return false;
            }
        }
        return true;
    }
}