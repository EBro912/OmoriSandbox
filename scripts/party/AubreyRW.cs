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
    protected override string[] InvalidStates => ["miserable", "manic", "furious", "stressed"];
    public override bool IsRealWorld => true;
}