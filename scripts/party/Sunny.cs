using Godot;

namespace OmoriSandbox.Actors;

internal sealed class Sunny : PartyMember
{
    public override string Name => "Sunny";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/sunny.tres");

    private static readonly int[] HPTreeData = [80];
    public override int[] HPTree => HPTreeData;

    private static readonly int[] JuiceTreeData = [30];
    public override int[] JuiceTree => JuiceTreeData;

    private static readonly int[] ATKTreeData = [7];
    public override int[] ATKTree => ATKTreeData;

    private static readonly int[] DEFTreeData = [2];
    public override int[] DEFTree => DEFTreeData;

    private static readonly int[] SPDTreeData = [6];
    public override int[] SPDTree => SPDTreeData;

    public override int BaseLuck => 5;
    private static readonly string[] InvalidStatesData = ["happy", "ecstatic", "manic", "depressed", "miserable", "enraged", "furious"];
    public override string[] InvalidStates => InvalidStatesData;
    public override bool IsRealWorld => true;
    private static readonly string[] EquippableWeaponsData = ["Fly Swatter", "Steak Knife", "Hands", "Violin"];
    public override string[] EquippableWeapons => EquippableWeaponsData;
}