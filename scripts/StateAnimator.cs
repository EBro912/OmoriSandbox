using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox;
internal partial class StateAnimator : Node
{
	[Export] private Sprite2D StateSprite;
	[Export] private Sprite2D FaceStateSprite;

	internal Sprite2D EmotionSprite => StateSprite;

	// the default atlas textures provided by the scene
	private Texture2D DefaultLabelTexture;
	private Texture2D DefaultFaceTexture;

	public override void _Ready()
	{
		DefaultLabelTexture = StateSprite?.Texture;
		DefaultFaceTexture = FaceStateSprite?.Texture;
	}

	public void SetState(string state)
	{
		ShowAsset(Resolve(state).Asset);
	}

	public void SetStateAtlas(string state)
	{
		ShowLabel(Resolve(state).Asset);
	}

	/// <summary>
	/// Shows a specific label/portrait asset instead of an emotion.
	/// </summary>
	public void ShowAsset(EmotionAsset asset)
	{
		if (asset == null)
			return;

		ShowLabel(asset);
		if (asset.FaceTexture != null)
			FadeInFace(asset.FaceTexture, null);
		else if (asset.FaceAtlasCell.HasValue)
			FadeInFace(DefaultFaceTexture, FaceStateAtlas(asset.FaceAtlasCell.Value.X, asset.FaceAtlasCell.Value.Y));
	}

	private void ShowLabel(EmotionAsset asset)
	{
		if (asset == null)
			return;

		if (asset.LabelTexture != null)
		{
			StateSprite.RegionEnabled = false;
			StateSprite.Texture = asset.LabelTexture;
		}
		else if (asset.LabelAtlasRow.HasValue)
		{
			StateSprite.Texture = DefaultLabelTexture;
			StateSprite.RegionEnabled = true;
			StateSprite.RegionRect = StateAtlas(asset.LabelAtlasRow.Value);
		}
	}

	private static Emotion Resolve(string state)
	{
		if (Database.TryGetEmotion(state, out Emotion emotion))
			return emotion;

		GD.PushWarning("Unknown emotion for state animator: " + state);
		Database.TryGetEmotion("neutral", out emotion);
		return emotion;
	}

	private void FadeInFace(Texture2D texture, Rect2? region)
	{
		if (FaceStateSprite == null)
			return;

		// emotions in the original game have a "fade in" effect here
		// so we do that by making a copy of the back sprite and fading in the new one
		FaceStateSprite.ZIndex = -4;
		Sprite2D newFaceSprite = (Sprite2D)FaceStateSprite.Duplicate();
		GetParent().AddChild(newFaceSprite);
		newFaceSprite.ZIndex = -3;
		newFaceSprite.Modulate = Colors.Transparent;
		newFaceSprite.Texture = texture;
		newFaceSprite.RegionEnabled = region.HasValue;
		if (region.HasValue)
			newFaceSprite.RegionRect = region.Value;
		Tween tween = newFaceSprite.CreateTween();
		tween.TweenProperty(newFaceSprite, "modulate:a", 1f, 0.25f);
		tween.TweenCallback(Callable.From(() =>
		{
			// after we fade in the new sprite, remove the old one
			FaceStateSprite.Free();
			FaceStateSprite = newFaceSprite;
		}));
	}

	private Rect2 StateAtlas(int y)
	{
		return new Rect2(17f, 24f * y, 98f, 22f);
	}

	private Rect2 FaceStateAtlas(int x = 0, int y = 0)
	{
		return new Rect2(100f * x, 100f * y, 100f, 100f);
	}
}
