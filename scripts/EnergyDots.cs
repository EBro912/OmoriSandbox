using Godot;

namespace OmoriSandbox;

internal partial class EnergyDots : Sprite2D
{
    private const int LineWidth = 290;
    private const int LineHeight = 28;
    private const int SourceWidth = 289;
    private const int TrailLength = 40;
    private const double StepTime = 1d / 60d;

    private static readonly Vector2 DreamworldOrigin = new(363, 1);

    private Image Atlas;
    private Image Trail;
    private ImageTexture TrailTexture;
    private double Elapsed;
    private Vector2 OriginValue = DreamworldOrigin;

    internal int Lx { get; private set; }

    internal Vector2 Origin
    {
        get => OriginValue;
        set
        {
            OriginValue = value;
            ResetTrail();
        }
    }

    internal Rect2 CurrentRow => new(Origin.X, Origin.Y + BattleManager.Instance.Energy * LineHeight, SourceWidth, LineHeight);

    public override void _Ready()
    {
        Atlas = Texture.GetImage();
        Atlas.Convert(Image.Format.Rgba8);
        Trail = Image.CreateEmpty(LineWidth, LineHeight, false, Image.Format.Rgba8);
        TrailTexture = ImageTexture.CreateFromImage(Trail);
        Texture = TrailTexture;
        RegionEnabled = false;
    }

    internal void ResetTrail()
    {
        Lx = 0;
        Elapsed = 0;
        if (Trail == null)
            return;
        Trail.Fill(Colors.Transparent);
        TrailTexture.Update(Trail);
    }

    public void Tick(double delta)
    {
        Elapsed += delta;
        int steps = 0;
        while (Elapsed >= StepTime && steps < LineWidth + TrailLength)
        {
            Elapsed -= StepTime;
            Step();
            steps++;
        }

        if (Elapsed >= StepTime)
            Elapsed = 0;
        if (steps > 0)
            TrailTexture.Update(Trail);
    }

    private void Step()
    {
        Lx = (Lx + 1) % (LineWidth + TrailLength);
        if (Lx < SourceWidth)
        {
            Rect2 row = CurrentRow;
            Trail.BlitRect(Atlas, new Rect2I((int)row.Position.X + Lx, (int)row.Position.Y, 1, LineHeight), new Vector2I(Lx, 0));
        }

        if (Lx >= TrailLength)
            Trail.FillRect(new Rect2I(Lx - TrailLength, 0, 1, LineHeight), Colors.Transparent);
    }
}
