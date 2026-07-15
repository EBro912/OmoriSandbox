using System.Threading.Tasks;
using Godot;

namespace OmoriSandbox.Actors;

internal sealed class Kel : PartyMember
{
    public override string Name => "Kel";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/kel.tres");

    private static readonly int[] HPTreeData = new[] { 37, 38, 40, 42, 47, 52, 58, 61, 66, 71, 75, 77, 81, 90, 93, 103, 108, 110, 115, 119, 124, 128, 133, 139, 141, 147, 151, 157, 165, 167, 174, 179, 185, 187, 192, 196, 201, 205, 210, 218, 227, 236, 243, 251, 260, 269, 276, 286, 296, 300 };
    public override int[] HPTree => HPTreeData;
    private static readonly int[] JuiceTreeData = new[] { 25, 31, 33, 37, 41, 47, 53, 55, 61, 65, 71, 74, 79, 88, 91, 100, 106, 108, 113, 117, 123, 129, 135, 141, 143, 148, 154, 159, 169, 172, 181, 185, 189, 192, 197, 201, 206, 211, 215, 222, 230, 239, 248, 255, 265, 275, 285, 295, 305, 315 };
    public override int[] JuiceTree => JuiceTreeData;
    private static readonly int[] ATKTreeData = new[] { 4, 5, 6, 8, 9, 10, 11, 12, 13, 14, 16, 17, 18, 21, 22, 24, 25, 26, 27, 28, 29, 31, 32, 33, 34, 36, 38, 40, 42, 43, 46, 48, 49, 50, 51, 52, 54, 55, 56, 58, 61, 63, 66, 68, 70, 72, 74, 76, 78, 80 };
    public override int[] ATKTree => ATKTreeData;
    private static readonly int[] DEFTreeData = new[] { 4, 5, 5, 6, 7, 8, 9, 9, 10, 11, 12, 12, 13, 15, 16, 18, 19, 20, 22, 23, 24, 26, 27, 28, 29, 30, 32, 34, 36, 37, 39, 41, 42, 42, 43, 44, 46, 47, 48, 50, 52, 54, 56, 58, 59, 62, 64, 66, 68, 70 };
    public override int[] DEFTree => DEFTreeData;
    private static readonly int[] SPDTreeData = new[] { 5, 6, 9, 12, 15, 17, 19, 20, 22, 25, 28, 30, 33, 36, 37, 40, 43, 44, 46, 49, 51, 53, 55, 58, 60, 62, 64, 66, 69, 70, 73, 75, 78, 79, 81, 84, 87, 90, 93, 96, 99, 102, 105, 108, 114, 114, 117, 120, 125, 130 };
    public override int[] SPDTree => SPDTreeData;
    public override int BaseLuck => 7;
    private static readonly string[] InvalidStatesData = ["miserable", "manic", "furious", "stressed"];
    public override string[] InvalidStates => InvalidStatesData;
    public override bool IsRealWorld => false;
    private static readonly string[] EquippableWeaponsData = ["Rubber Ball", "Meteor Ball", "Blood Orange", "Jack", "Beach Ball", "Coconut", "Globe", "Chicken Ball", "Snowball", "Basketball"];
    public override string[] EquippableWeapons => EquippableWeaponsData;
}