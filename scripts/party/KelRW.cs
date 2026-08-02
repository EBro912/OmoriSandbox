using Godot;

namespace OmoriSandbox.Actors;

internal sealed class KelRW : PartyMember
{
    public override string Name => "Kel";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/kel_rw.tres");

    private static readonly int[] HPTreeData = [130];
    public override int[] HPTree => HPTreeData;

    private static readonly int[] JuiceTreeData = [100];
    public override int[] JuiceTree => JuiceTreeData;

    private static readonly int[] ATKTreeData = [18];
    public override int[] ATKTree => ATKTreeData;

    private static readonly int[] DEFTreeData = [8];
    public override int[] DEFTree => DEFTreeData;

    private static readonly int[] SPDTreeData = [17];
    public override int[] SPDTree => SPDTreeData;

    public override int BaseLuck => 15;
    private static readonly string[] InvalidStatesData = ["miserable", "manic", "furious", "stressed"];
    public override string[] InvalidStates => InvalidStatesData;
    public override bool IsRealWorld => true;
    private static readonly string[] EquippableWeaponsData = ["Basketball (Real World)"];
    public override string[] EquippableWeapons => EquippableWeaponsData;
}