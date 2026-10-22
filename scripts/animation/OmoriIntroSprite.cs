using Godot;

namespace OmoriSandbox.Animation;

internal partial class OmoriIntroSprite : Sprite2D
{
	internal void ShowDrawing(int index)
	{
		RegionRect = new Rect2(index % 6 * 318, index / 6 * 389, 318, 389);
	}
}
