using Godot;

namespace OmoriSandbox.Actors;
internal sealed class AubreyRW : PartyMember
{
    public override string Name => "Aubrey";

    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/aubrey_rw.tres");

    private static readonly int[] HPTreeData = [240];
    public override int[] HPTree => HPTreeData;

    private static readonly int[] JuiceTreeData = [25];
    public override int[] JuiceTree => JuiceTreeData;

    private static readonly int[] ATKTreeData = [22];
    public override int[] ATKTree => ATKTreeData;

    private static readonly int[] DEFTreeData = [12];
    public override int[] DEFTree => DEFTreeData;

    private static readonly int[] SPDTreeData = [12];
    public override int[] SPDTree => SPDTreeData;

    public override int BaseLuck => 5;
    private static readonly string[] InvalidStatesData = ["miserable", "manic", "furious", "stressed"];
    public override string[] InvalidStates => InvalidStatesData;
    public override bool IsRealWorld => true;
    private static readonly string[] EquippableWeaponsData = ["Nail Bat"];
    public override string[] EquippableWeapons => EquippableWeaponsData;
}