using Godot;

namespace OmoriSandbox.Actors;
internal sealed class AubreyRW : PartyMember
{
    public override string Name => "Aubrey";

    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/aubrey_rw.tres");

    public override int[] HPTree => [240];

    public override int[] JuiceTree => [25];

    public override int[] ATKTree => [22];

    public override int[] DEFTree => [12];

    public override int[] SPDTree => [12];

    public override int BaseLuck => 5;
    public override string[] InvalidStates => ["miserable", "manic", "furious", "stressed"];
    public override bool IsRealWorld => true;
    public override string[] EquippableWeapons => ["Nail Bat"];
}