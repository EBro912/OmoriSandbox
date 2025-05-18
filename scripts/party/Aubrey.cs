public class Aubrey : PartyMember
{
    public override string Name => "Aubrey";
    public override string AnimationPath => "res://animations/aubrey.tres";

    public override int[] HPTree => new[] { 35, 43, 49, 55, 60, 68, 75, 81, 87, 93, 97, 105, 113, 125, 130, 140, 148, 154, 158, 164, 171, 176, 182, 187, 193, 200, 204, 210, 221, 226, 238, 244, 249, 256, 263, 269, 275, 281, 288, 300, 312, 324, 337, 348, 360, 375, 391, 407, 424, 444 };
    public override int[] JuiceTree => new[] { 10, 13, 15, 18, 20, 22, 24, 27, 29, 31, 32, 34, 36, 41, 43, 47, 50, 53, 54, 56, 58, 59, 62, 64, 66, 69, 71, 73, 77, 78, 83, 86, 87, 90, 93, 95, 98, 101, 104, 109, 114, 119, 124, 129, 134, 139, 144, 147, 148, 150 };
    public override int[] ATKTree => new[] { 6, 8, 10, 11, 12, 13, 15, 17, 19, 20, 21, 23, 24, 27, 28, 32, 35, 36, 37, 40, 42, 43, 44, 45, 47, 49, 50, 52, 55, 56, 60, 62, 63, 64, 65, 67, 68, 70, 72, 75, 78, 81, 84, 87, 90, 94, 98, 102, 106, 110 };
    public override int[] DEFTree => new[] { 3, 4, 5, 6, 7, 8, 9, 9, 10, 12, 13, 14, 15, 17, 18, 20, 21, 22, 23, 25, 26, 27, 28, 29, 31, 32, 33, 34, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 47, 49, 51, 53, 55, 57, 59, 62, 64, 66, 68, 70 };
    public override int[] SPDTree => new[] { 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 23, 24, 25, 26, 27, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55 };
    public override int BaseLuck => 3;

    public override Stats Weapon => new(atk: 4, hit: 100);

    protected override string[] EquippedSkills => ["AAttack", "PepTalk", "Headbutt", "PowerHit", "Twirl"];

    public override bool IsStateValid(string state)
    {
        return state != "miserable" &&
            state != "manic" &&
            state != "furious" &&
            state != "stressed";
    }

    public override bool IsRealWorld => false;
}