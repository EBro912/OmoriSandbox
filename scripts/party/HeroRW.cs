using Godot;

namespace OmoriSandbox.Actors;

internal sealed class HeroRW : PartyMember
{
    public override string Name => "Hero";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/hero_rw.tres");

    public override int[] HPTree => [260];

    public override int[] JuiceTree => [60];

    public override int[] ATKTree => [20];

    public override int[] DEFTree => [20];

    public override int[] SPDTree => [10];

    public override int BaseLuck => 10;
    public override string[] InvalidStates => ["miserable", "manic", "furious", "stressed"];
    public override bool IsRealWorld => true;
    public override string[] EquippableWeapons => ["Fist"];
}