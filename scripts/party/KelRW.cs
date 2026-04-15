using Godot;

namespace OmoriSandbox.Actors;

internal sealed class KelRW : PartyMember
{
    public override string Name => "Kel";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/kel_rw.tres");

    public override int[] HPTree => [130];

    public override int[] JuiceTree => [100];

    public override int[] ATKTree => [18];

    public override int[] DEFTree => [8];

    public override int[] SPDTree => [17];

    public override int BaseLuck => 15;
    public override string[] InvalidStates => ["miserable", "manic", "furious", "stressed"];
    public override bool IsRealWorld => true;
    public override string[] EquippableWeapons => ["Basketball (Real World)"];
}