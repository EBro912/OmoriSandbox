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

    public override Stats Weapon => new Stats(atk: 2, hit: 95);

    protected override string[] EquippedSkills => ["SAttack", "CalmDown"];

    public override bool IsStateValid(string state)
    {
        return state == "neutral" || state == "hurt" || state == "sad" || state == "angry" || state == "afraid" || state == "stressed";
    }

    public override bool IsRealWorld => true;
}