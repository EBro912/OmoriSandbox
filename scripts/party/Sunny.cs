using Godot;

namespace OmoriSandbox.Actors;

internal sealed class Sunny : PartyMember
{
    public override string Name => "Sunny";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/sunny.tres");

    public override int[] HPTree => [80];

    public override int[] JuiceTree => [30];

    public override int[] ATKTree => [7];

    public override int[] DEFTree => [2];

    public override int[] SPDTree => [6];

    public override int BaseLuck => 5;
    public override string[] InvalidStates => ["happy", "ecstatic", "manic", "depressed", "miserable", "enraged", "furious"];
    public override bool IsRealWorld => true;
    public override string[] EquippableWeapons => ["Fly Swatter", "Steak Knife", "Hands", "Violin"];
}