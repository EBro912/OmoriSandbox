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
		switch (state)
		{
			case "neutral":
			case "victory":
				StateSprite.RegionRect = StateAtlas(0);
				FaceStateSprite.RegionRect = FaceStateAtlas();
				break;
			case "toast":
				StateSprite.RegionRect = StateAtlas(1);
				FaceStateSprite.RegionRect = FaceStateAtlas();
				break;
			case "stressed":
				StateSprite.RegionRect = StateAtlas(2);
				FaceStateSprite.RegionRect = FaceStateAtlas(1);
				break;
			case "happy":
				StateSprite.RegionRect = StateAtlas(3);
				FaceStateSprite.RegionRect = FaceStateAtlas(2);
				break;
			case "ecstatic":
				StateSprite.RegionRect = StateAtlas(4);
				FaceStateSprite.RegionRect = FaceStateAtlas(3);
				break;
			case "manic":
				StateSprite.RegionRect = StateAtlas(5);
				FaceStateSprite.RegionRect = FaceStateAtlas(0, 1);
				break;
			case "sad":
				StateSprite.RegionRect = StateAtlas(6);
				FaceStateSprite.RegionRect = FaceStateAtlas(1, 1);
				break;
			case "depressed":
				StateSprite.RegionRect = StateAtlas(7);
				FaceStateSprite.RegionRect = FaceStateAtlas(2, 1);
				break;
			case "miserable":
				StateSprite.RegionRect = StateAtlas(8);
				FaceStateSprite.RegionRect = FaceStateAtlas(3, 1);
				break;
			case "angry":
				StateSprite.RegionRect = StateAtlas(9);
				FaceStateSprite.RegionRect = FaceStateAtlas(0, 2);
				break;
			case "enraged":
				StateSprite.RegionRect = StateAtlas(10);
				FaceStateSprite.RegionRect = FaceStateAtlas(1, 2);
				break;
			case "furious":
				StateSprite.RegionRect = StateAtlas(11);
				FaceStateSprite.RegionRect = FaceStateAtlas(2, 2);
				break;
			case "afraid":
				StateSprite.RegionRect = StateAtlas(12);
				FaceStateSprite.RegionRect = FaceStateAtlas(3, 2);
				break;

		}
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
