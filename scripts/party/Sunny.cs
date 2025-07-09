public class Sunny : PartyMember
{
    public override string Name => "Sunny";
    public override string AnimationPath => "res://animations/sunny.tres";

    public override int[] HPTree => [80];

    public override int[] JuiceTree => [30];

    public override int[] ATKTree => [7];

    public override int[] DEFTree => [2];

    public override int[] SPDTree => [6];

    public override int BaseLuck => 5;

    protected override string[] EquippedSkills => ["SAttack", "CalmDown", "PepTalk"];
    protected override string[] InvalidStates => ["happy", "ecstatic", "manic", "depressed", "miserable", "enraged", "furious"];
    public override bool IsRealWorld => true;
}