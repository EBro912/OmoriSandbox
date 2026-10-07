using Godot;

namespace OmoriSandbox;

internal sealed class WindowSkin
{
	internal Texture2D Texture { get; }
	internal bool UseBackground { get; }
	internal Texture2D Background { get; }
	internal Texture2D BackgroundTile { get; }

	internal WindowSkin(Texture2D texture, bool useBackground)
	{
		Texture = texture;
		UseBackground = useBackground;
		if (!useBackground)
			return;
		Image image = texture.GetImage();
		if (image.IsCompressed())
			image.Decompress();
		Background = ImageTexture.CreateFromImage(image.GetRegion(new Rect2I(0, 0, 96, 96)));
		BackgroundTile = ImageTexture.CreateFromImage(image.GetRegion(new Rect2I(0, 96, 96, 96)));
	}
}
