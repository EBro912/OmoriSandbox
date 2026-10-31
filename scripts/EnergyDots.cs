using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Menu;

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
    private readonly RandomNumberGenerator StressRandom = new();
    private int StressFrame;
    private bool ShowingBasilStress;

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

    internal static bool BasilStressed => StorySwitches.BasilTroop && BattleManager.Instance.GetSunnyStandIn().CurrentEmotion.Id is "stressed";

    internal Rect2 CurrentRow
    {
        get
        {
            int energy = BasilStressed ? 10 : BattleManager.Instance.Energy;
            int row = energy == 10 && StorySwitches.BasilTroop ? 11 : energy;
            return new(Origin.X, Origin.Y + row * LineHeight, SourceWidth, LineHeight);
        }
    }

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
        StressFrame = (StressFrame + 1) % 60;
        bool stressed = BasilStressed;
        if (stressed != ShowingBasilStress)
        {
            ShowingBasilStress = stressed;
            MenuManager.Instance.RefreshEnergyDisplay();
        }
        if (stressed && StressFrame != 0 && StressFrame % 3 == 0 && StorySwitches.BlackSpaceWorld)
            MenuManager.Instance.ShowEnergyNumber(StressRandom.RandiRange(0, 10));

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
