public class AubreyRW : PartyMember
{
    public override string Name => "Aubrey";

    public override string AnimationPath => "res://animations/aubrey_rw.tres";

    public override int[] HPTree => [240];

    public override int[] JuiceTree => [25];

    public override int[] ATKTree => [22];

    public override int[] DEFTree => [12];

    public override int[] SPDTree => [12];

    public override int BaseLuck => 5;

    public override Stats Weapon => new Stats(atk: 3, hit: 95);

    protected override string[] EquippedSkills => ["ARWAttack", "Homerun"];

    public override bool IsStateValid(string state)
    {
        return state != "miserable" &&
            state != "manic" &&
            state != "furious" &&
            state != "stressed";
    }

    public override bool IsRealWorld => true;
}