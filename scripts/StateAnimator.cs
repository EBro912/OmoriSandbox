using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox;
/// <summary>
/// Shows an actor's emotion label and portrait on a battlecard or enemy info box, fading the portrait between emotions.
/// </summary>
public partial class StateAnimator : Node
{
	[Export] private Sprite2D StateSprite;
	[Export] private Sprite2D FaceStateSprite;

	internal Sprite2D EmotionSprite => StateSprite;

	// the default atlas textures provided by the scene
	private Texture2D DefaultLabelTexture;
	private Texture2D DefaultFaceTexture;
	private Vector2? DefaultLabelOffset;

	/// <inheritdoc/>
	public override void _Ready()
	{
		CacheDefaultTextures();
	}

	// the atlas textures come from the scene, capture them before the first write so
	// anything run before _Ready (e.g. infoboxes configured before AddChild) are safe
	private void CacheDefaultTextures()
	{
		DefaultLabelTexture ??= StateSprite?.Texture;
		DefaultFaceTexture ??= FaceStateSprite?.Texture;
		DefaultLabelOffset ??= StateSprite?.Offset;
	}

	/// <summary>
	/// Shows the label and portrait of the emotion with the given id, fading the portrait in.
	/// Unknown ids show neutral and log a warning.
	/// </summary>
	/// <param name="state">The id of the emotion to show.</param>
	public void SetState(string state)
	{
		ShowAsset(Resolve(state).Asset);
	}

	/// <summary>
	/// Shows only the label of the emotion with the given id, keeping the current portrait.
	/// Unknown ids show neutral and log a warning.
	/// </summary>
	/// <param name="state">The id of the emotion to show.</param>
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

		CacheDefaultTextures();
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

		CacheDefaultTextures();
		if (asset.LabelTexture != null)
		{
			StateSprite.RegionEnabled = false;
			StateSprite.Texture = asset.LabelTexture;
			StateSprite.Offset = DefaultLabelOffset.Value;
		}
		else if (asset.LabelAtlasRow.HasValue)
		{
			StateSprite.Texture = DefaultLabelTexture;
			StateSprite.RegionEnabled = true;
			StateSprite.RegionRect = StateAtlas(asset.LabelAtlasRow.Value);
			StateSprite.Offset = DefaultLabelOffset.Value + new Vector2(1f, 1f);
		}
	}

	private static Emotion Resolve(string state)
	{
		if (Database.TryGetEmotion(state, out Emotion emotion))
			return emotion;

		GD.PushWarning("Unknown emotion for state animator: " + state);
		return Database.NeutralEmotion;
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
		return new Rect2(0f, 24f * y, 134f, 24f);
	}

	private Rect2 FaceStateAtlas(int x = 0, int y = 0)
	{
		return new Rect2(100f * x, 100f * y, 100f, 100f);
	}
}
