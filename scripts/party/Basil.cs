using Godot;

namespace OmoriSandbox.Actors;
internal sealed class Basil : PartyMember
{
    public override string Name => "Basil";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/basil.tres");
    public override int[] HPTree => new[] { 25, 31, 33, 37, 41, 47, 53, 55, 61, 65, 71, 74, 79, 88, 91, 100, 106, 108, 113, 117, 123, 129, 135, 141, 143, 148, 154, 159, 169, 172, 181, 185, 189, 192, 197, 201, 206, 220, 235, 250, 265, 280, 295, 310, 325, 340, 355, 370, 385, 400 };
    public override int[] JuiceTree => new[] { 33, 36, 41, 49, 55, 59, 66, 71, 74, 81, 88, 94, 97, 106, 112, 120, 126, 131, 138, 141, 148, 153, 160, 166, 172, 177, 184, 189, 195, 205, 213, 219, 224, 231, 239, 246, 255, 260, 268, 276, 283, 294, 300, 310, 318, 324, 332, 338, 342, 350 };
    public override int[] ATKTree => new[] { 6, 7, 8, 8, 9, 10, 11, 13, 15, 15, 17, 18, 19, 22, 23, 25, 26, 27, 28, 30, 30, 31, 32, 33, 35, 35, 36, 37, 37, 39, 42, 44, 47, 50, 53, 56, 59, 61, 63, 65, 67, 69, 71, 73, 75, 76, 77, 78, 79, 80 };
    public override int[] DEFTree => new[] { 5, 6, 8, 9, 10, 12, 13, 15, 16, 17, 19, 21, 22, 25, 26, 29, 30, 31, 33, 35, 36, 37, 39, 41, 42, 43, 44, 45, 48, 49, 51, 52, 54, 56, 57, 59, 61, 64, 67, 70, 73, 76, 79, 82, 85, 86, 87, 88, 89, 90 };
    public override int[] SPDTree => new[] { 1, 3, 5, 6, 8, 10, 11, 13, 14, 16, 18, 19, 21, 23, 24, 26, 27, 29, 31, 32, 34, 36, 37, 39, 40, 42, 44, 45, 47, 49, 50, 52, 53, 55, 57, 58, 60, 62, 63, 65, 66, 68, 70, 71, 73, 75, 76, 78, 79, 80 };
    public override int BaseLuck => 10;
    public override string[] InvalidStates => ["stressed"];
    public override bool IsRealWorld => false;
    public override string[] EquippableWeapons => ["Garden Shears"];
}