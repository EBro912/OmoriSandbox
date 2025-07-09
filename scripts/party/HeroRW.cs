public class HeroRW : PartyMember
{
    public override string Name => "Hero";
    public override string AnimationPath => "res://animations/hero_rw.tres";

    public override int[] HPTree => [260];

    public override int[] JuiceTree => [60];

    public override int[] ATKTree => [20];

    public override int[] DEFTree => [20];

    public override int[] SPDTree => [10];

    public override int BaseLuck => 10;

    protected override string[] EquippedSkills => ["HRWAttack", "FirstAid"];
    protected override string[] InvalidStates => ["miserable", "manic", "furious", "stressed"];
    public override bool IsRealWorld => true;
}