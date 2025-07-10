public class Omori : PartyMember
{
    public override string Name => "Omori";
    public override string AnimationPath => "res://animations/omori.tres";

    public override int[] HPTree => new[] { 33, 36, 41, 49, 55, 59, 66, 71, 74, 81, 88, 94, 97, 106, 112, 120, 126, 131, 138, 141, 148, 153, 160, 166, 172, 177, 184, 189, 195, 205, 213, 219, 224, 231, 239, 246, 255, 260, 268, 276, 283, 294, 300, 310, 318, 324, 332, 342, 350, 360 };
    public override int[] JuiceTree => new[] { 20, 25, 29, 31, 35, 39, 41, 46, 50, 53, 57, 61, 64, 73, 75, 82, 83, 86, 90, 95, 96, 100, 103, 106, 110, 112, 116, 121, 127, 129, 137, 142, 146, 150, 151, 154, 158, 161, 166, 173, 179, 186, 194, 203, 210, 218, 226, 232, 239, 250 };
    public override int[] ATKTree => new[] { 5, 6, 8, 9, 10, 12, 13, 15, 16, 17, 19, 21, 22, 25, 26, 29, 30, 31, 33, 35, 36, 37, 39, 41, 42, 43, 44, 45, 48, 49, 51, 52, 54, 56, 57, 59, 61, 62, 63, 65, 67, 70, 72, 74, 77, 80, 82, 84, 87, 90 };
    public override int[] DEFTree => new[] { 6, 7, 8, 8, 9, 10, 11, 13, 15, 15, 17, 18, 19, 22, 23, 25, 26, 27, 28, 30, 30, 31, 32, 33, 35, 35, 36, 37, 37, 39, 42, 43, 44, 46, 47, 49, 50, 51, 52, 54, 56, 59, 61, 63, 65, 68, 70, 72, 74, 77 };
    public override int[] SPDTree => new[] { 6, 8, 9, 10, 11, 13, 15, 17, 19, 20, 21, 23, 26, 28, 29, 31, 32, 35, 37, 40, 41, 44, 46, 48, 50, 51, 53, 54, 57, 58, 60, 62, 64, 65, 66, 68, 70, 71, 74, 77, 79, 82, 85, 88, 90, 92, 94, 96, 98, 100 };
    public override int BaseLuck => 5;
    protected override string[] InvalidStates => ["afraid", "stressed"];

    public override bool IsRealWorld => false;

    public bool HasUsedPlotArmor = false;
    public string OldEmotion;
}