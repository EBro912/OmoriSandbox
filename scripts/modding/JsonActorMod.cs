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
}