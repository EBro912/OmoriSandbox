using Godot;

namespace OmoriSandbox.Actors;

internal sealed class HeroRW : PartyMember
{
    public override string Name => "Hero";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/hero_rw.tres");

    private static readonly int[] HPTreeData = [260];
    public override int[] HPTree => HPTreeData;

    private static readonly int[] JuiceTreeData = [60];
    public override int[] JuiceTree => JuiceTreeData;

    private static readonly int[] ATKTreeData = [20];
    public override int[] ATKTree => ATKTreeData;

    private static readonly int[] DEFTreeData = [20];
    public override int[] DEFTree => DEFTreeData;

    private static readonly int[] SPDTreeData = [10];
    public override int[] SPDTree => SPDTreeData;

    public override int BaseLuck => 10;
    private static readonly string[] InvalidStatesData = ["miserable", "manic", "furious", "stressed"];
    public override string[] InvalidStates => InvalidStatesData;
    public override bool IsRealWorld => true;
    private static readonly string[] EquippableWeaponsData = ["Fist"];
    public override string[] EquippableWeapons => EquippableWeaponsData;
}