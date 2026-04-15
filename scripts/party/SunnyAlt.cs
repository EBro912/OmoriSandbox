using Godot;

namespace OmoriSandbox.Actors;

internal sealed class SunnyAlt : PartyMember
{
    public override string Name => "Sunny";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/sunny_alt.tres");

    public override int[] HPTree => [300];

    public override int[] JuiceTree => [30];

    public override int[] ATKTree => [46];

    public override int[] DEFTree => [72];

    public override int[] SPDTree => [66];
    public override int BaseLuck => 5;
    public override string[] EquippableWeapons => ["Violin"];
    public override string[] InvalidStates => ["happy", "ecstatic", "manic", "depressed", "miserable", "enraged", "furious"];
    public override bool IsRealWorld => true;
}