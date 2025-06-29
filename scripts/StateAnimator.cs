using Godot;

public partial class StateAnimator : Node
{
	private Sprite2D StateSprite;
	private Sprite2D FaceStateSprite;

	public override void _Ready()
	{
		StateSprite = GetNode<Sprite2D>("../State");
		FaceStateSprite = GetNode<Sprite2D>("../FaceState");
	}

	public void SetState(string state)
	{
		Rect2 target = new();
		switch (state)
		{
			case "neutral":
			case "victory":
				StateSprite.RegionRect = StateAtlas(0);
				target = FaceStateAtlas();
				break;
			case "toast":
				StateSprite.RegionRect = StateAtlas(1);
				target = FaceStateAtlas();
				break;
			case "stressed":
				StateSprite.RegionRect = StateAtlas(2);
				target = FaceStateAtlas(1);
				break;
			case "happy":
				StateSprite.RegionRect = StateAtlas(3);
				target = FaceStateAtlas(2);
				break;
			case "ecstatic":
				StateSprite.RegionRect = StateAtlas(4);
				target = FaceStateAtlas(3);
				break;
			case "manic":
				StateSprite.RegionRect = StateAtlas(5);
				target = FaceStateAtlas(0, 1);
				break;
			case "sad":
				StateSprite.RegionRect = StateAtlas(6);
				target = FaceStateAtlas(1, 1);
				break;
			case "depressed":
				StateSprite.RegionRect = StateAtlas(7);
				target = FaceStateAtlas(2, 1);
				break;
			case "miserable":
				StateSprite.RegionRect = StateAtlas(8);
				target = FaceStateAtlas(3, 1);
				break;
			case "angry":
				StateSprite.RegionRect = StateAtlas(9);
				target = FaceStateAtlas(0, 2);
				break;
			case "enraged":
				StateSprite.RegionRect = StateAtlas(10);
				target = FaceStateAtlas(1, 2);
				break;
			case "furious":
				StateSprite.RegionRect = StateAtlas(11);
				target = FaceStateAtlas(2, 2);
				break;
			case "afraid":
				StateSprite.RegionRect = StateAtlas(12);
				target = FaceStateAtlas(3, 2);
				break;

		}

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
