using System.Collections.Generic;
using Godot;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox;
internal partial class StateAnimator : Node
{
	[Export] private Sprite2D StateSprite;
	[Export] private Sprite2D FaceStateSprite;
	
	internal Sprite2D EmotionSprite => StateSprite;

	// Atlas index of the above head emotion label
	private readonly Dictionary<string, int> StateAtlases = new()
	{
		{ "neutral", 0 },
		{ "victory", 0 },
		{ "toast", 1 },
		{ "stressed", 2},
		{ "happy", 3 },
		{ "ecstatic", 4 },
		{ "manic", 5 },
		{ "sad", 6 },
		{ "depressed", 7 },
		{ "miserable", 8 },
		{ "angry", 9 },
		{ "enraged", 10 },
		{ "furious", 11 },
		{ "afraid", 12 }
	};

	private readonly Dictionary<string, (int, int)> FaceStateAtlases = new()
	{
		{ "neutral", (0, 0) },
		{ "victory", (0, 0) },
		{ "toast", (1, 0) },
		{ "stressed", (1, 0) },
		{ "happy", (2, 0) },
		{ "ecstatic", (3, 0) },
		{ "manic", (0, 1) },
		{ "sad", (1, 1) },
		{ "depressed", (2, 1) },
		{ "miserable", (3, 1) },
		{ "angry", (0, 2) },
		{ "enraged", (1, 2) },
		{ "furious", (2, 2) },
		{ "afraid", (3, 2) },
		{ "plotarmor", (3, 2) }	
	};

	public void SetState(string state)
	{
		// these are really only split up because of the special case with plot armor
		// if emotion changes while in plot armor, the above head sprite changes
		// but the back sprite does not
		// TODO: improve once emotions are moved away from just being strings
		SetStateAtlas(state);
		SetFaceStateAtlas(state);
	}

	/// <summary>
	/// Shows a specific label/portrait asset instead of an emotion.
	/// </summary>
	public void ShowAsset(EmotionAsset asset)
	{
		if (asset == null)
			return;

		if (asset.LabelAtlasRow.HasValue)
			StateSprite.RegionRect = StateAtlas(asset.LabelAtlasRow.Value);
		if (asset.FaceAtlasCell.HasValue)
			FadeInFace(FaceStateAtlas(asset.FaceAtlasCell.Value.X, asset.FaceAtlasCell.Value.Y));
	}

	public void SetStateAtlas(string state)
	{
		StateSprite.RegionRect = StateAtlas(StateAtlases.GetValueOrDefault(state, 1));
	}

	private void SetFaceStateAtlas(string state)
	{
		if (!FaceStateAtlases.TryGetValue(state, out (int, int) index))
			return;

		FadeInFace(FaceStateAtlas(index.Item1, index.Item2));
	}

	private void FadeInFace(Rect2 target)
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
		newFaceSprite.RegionRect = target;
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
