public class Hero : PartyMember
{
    public override string Name => "Hero";
    public override string AnimationPath => "res://animations/hero.tres";

    public override int[] HPTree => new[] { 30, 36, 43, 46, 53, 62, 66, 73, 80, 84, 90, 97, 103, 114, 117, 128, 130, 137, 142, 148, 152, 158, 165, 172, 178, 180, 187, 194, 205, 209, 221, 227, 234, 242, 244, 251, 257, 264, 271, 282, 295, 306, 319, 331, 342, 354, 366, 377, 388, 400 };
    public override int[] JuiceTree => new[] { 20, 25, 29, 31, 35, 39, 41, 46, 50, 53, 57, 61, 64, 73, 75, 82, 83, 86, 90, 95, 96, 100, 103, 106, 110, 112, 116, 121, 127, 129, 137, 142, 146, 150, 151, 154, 158, 161, 166, 173, 179, 186, 194, 203, 210, 218, 226, 232, 239, 250 };
    public override int[] ATKTree => new[] { 4, 5, 6, 7, 8, 8, 9, 10, 10, 11, 13, 14, 14, 16, 17, 19, 20, 21, 22, 22, 23, 24, 25, 26, 27, 29, 30, 30, 32, 33, 35, 36, 37, 38, 39, 40, 41, 42, 43, 45, 47, 49, 51, 53, 55, 57, 59, 60, 61, 63 };
    public override int[] DEFTree => new[] { 6, 7, 8, 9, 10, 11, 12, 14, 15, 17, 18, 19, 20, 24, 25, 29, 30, 32, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 47, 48, 50, 51, 52, 53, 54, 55, 56, 57, 58, 60, 62, 64, 66, 68, 70, 73, 76, 78, 80, 85 };
    public override int[] SPDTree => new[] { 2, 3, 4, 5, 6, 6, 7, 8, 8, 9, 10, 11, 12, 14, 14, 15, 16, 17, 18, 18, 19, 20, 20, 21, 22, 23, 24, 24, 25, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45 };
    public override int BaseLuck => 5;

    public override Stats Weapon => new(atk: 4, hit: 100);

    protected override string[] EquippedSkills => ["HAttack", "Massage"];

    public override bool IsStateValid(string state)
    {
        return state != "miserable" &&
            state != "manic" &&
            state != "furious" &&
            state != "stressed";
    }
}