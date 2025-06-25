public class KelRW : PartyMember
{
    public override string Name => "Kel";
    public override string AnimationPath => "res://animations/kel_rw.tres";

    public override int[] HPTree => [130];

    public override int[] JuiceTree => [100];

    public override int[] ATKTree => [18];

    public override int[] DEFTree => [8];

    public override int[] SPDTree => [17];

    public override int BaseLuck => 15;

    protected override string[] EquippedSkills => ["KRWAttack", "Encourage"];

    public override bool IsStateValid(string state)
    {
        return state != "miserable" &&
            state != "manic" &&
            state != "furious" &&
            state != "stressed";
    }

    public override bool IsRealWorld => true;
}