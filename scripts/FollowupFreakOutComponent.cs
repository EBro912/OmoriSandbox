using Godot;

namespace OmoriSandbox;

internal partial class FollowupFreakOutComponent : Node
{
    [Export] private FollowupDirection Target;

    private const float FPS = 30f;
    private float FrameDuration = 1f / FPS;
    private float FrameTimer = 0f;
    private int ColorCounter = 0;
    private Vector2 Origin;

    private int BlinkIndex = 0;
    private readonly int[] BlinkColors = [255, 200, 150];

    public override void _Ready()
    {
        Origin = Target.Position;
    }

    private bool WasActive = false;

    public override void _Process(double delta)
    {
        // avoid processing the freakout animation when the bubble isn't even visible
        bool active = Target.Modulate.A > 0 && Target.Available;
        if (active)
        {
            FrameTimer += (float)delta;

            if (FrameTimer >= FrameDuration)
            {
                FrameTimer -= FrameDuration;
                DoFreakOut();
            }
        }
        else if (WasActive)
        {
            // reset only after being active
            FrameTimer = 0;
            Target.Position = Origin;
            Target.SelfModulate = Colors.White;
        }
        WasActive = active;
    }

    private void DoFreakOut()
    {
        Target.Position = Origin + new Vector2(GameManager.Instance.Random.RandfRange(-2f, 2f), GameManager.Instance.Random.RandfRange(-2f, 2f));
        ColorCounter++;
        // change the color slower than the movement
        if (ColorCounter > 1)
        {
            float color = BlinkColors[BlinkIndex] / 255f;
            BlinkIndex++;
            if (BlinkIndex >= BlinkColors.Length)
                BlinkIndex = 0;
            Target.SelfModulate = new Color(1, color, color);
            ColorCounter = 0;
        }
    }
}
