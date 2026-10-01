using Godot;

namespace OmoriSandbox;

internal partial class BattlebackDisplayComponent : TextureRect
{
    // Total horizontal tiles; the middle tile retains its original phase.
    [Export] private int Tiles = 1;

    private const float ScreenWidth = 640f;
    private const float ScreenHeight = 480f;

    private IBattleback CurrentBattleback;
    private int CurrentFrame;
    private double Elapsed;
    private Texture2D DefaultTexture;
    private Rect2? DisplayBounds;
    private Vector2 ScrollOffset;

    internal void ConfigureDisplay(Rect2 bounds)
    {
        DisplayBounds = bounds;
        ApplyLayout();
    }

    public void SetBattleback(string name)
    {
        if (BattlebackManager.Instance.TryGetBattleback(name, out IBattleback battleback))
        {
            CurrentBattleback = battleback;
            CurrentFrame = 0;
            Elapsed = 0;
            ScrollOffset = Vector2.Zero;
            Texture = battleback.GetFrame(0);
            ApplyLayout();
        }
        else
        {
            GD.PushWarning($"Failed to load battleback {name}, falling back to default.");
            SetDefaultBattleback();
        }
    }

    public void SetDefaultBattleback()
    {
        Texture = DefaultTexture;
        CurrentBattleback = null;
        CurrentFrame = 0;
        Elapsed = 0;
        ScrollOffset = Vector2.Zero;
        ApplyLayout();
    }

    // center the middle tile on the screen so battlebacks with non-640x480 dimensions display centered
    private void ApplyLayout()
    {
        if (Texture == null)
            return;

        Vector2 tex = Texture.GetSize();
        if (CurrentBattleback is ScrollingBattleback scrolling)
        {
            ApplyScrollingLayout(scrolling, tex);
            return;
        }
        if (DisplayBounds is { } bounds)
        {
            float left = Mathf.Round(ScreenWidth / 2f - Tiles * tex.X / 2f);
            float width = Tiles * tex.X;
            float extra = Mathf.Max(left - (bounds.Position.X - 32),
                bounds.End.X + 32 - (left + width));
            int extraTiles = Mathf.Max(0, Mathf.CeilToInt(extra / tex.X));
            left -= extraTiles * tex.X;
            width += extraTiles * 2 * tex.X;
            Position = new Vector2(left, Mathf.Round((ScreenHeight - tex.Y) / 2f));
            Size = new Vector2(width, tex.Y);
            return;
        }
        Size = new Vector2(Tiles * tex.X, tex.Y);
        // round so odd-dimension textures keep pixel alignment
        Position = new Vector2(Mathf.Round(ScreenWidth / 2f - Tiles * tex.X / 2f), Mathf.Round((ScreenHeight - tex.Y) / 2f));
    }

    private void ApplyScrollingLayout(ScrollingBattleback scrolling, Vector2 tex)
    {
        Rect2 coverage = (DisplayBounds ?? new Rect2(0, 0, ScreenWidth, ScreenHeight)).Grow(32);
        float left = Mathf.Round(ScreenWidth / 2f - Tiles * tex.X / 2f);
        float top = Mathf.Round((ScreenHeight - tex.Y) / 2f);
        int columns = Mathf.Max(0, Mathf.CeilToInt(Mathf.Max(left - coverage.Position.X, coverage.End.X - left - Tiles * tex.X) / tex.X)) + 1;
        int rows = scrolling.TileVertically
            ? Mathf.Max(0, Mathf.CeilToInt(Mathf.Max(top - coverage.Position.Y, coverage.End.Y - top - tex.Y) / tex.Y)) + 1
            : 0;
        float scrollY = scrolling.TileVertically ? Mathf.Floor(ScrollOffset.Y + 0.5f) : 0f;
        Position = new Vector2(left - columns * tex.X + Mathf.Floor(ScrollOffset.X + 0.5f), top - rows * tex.Y + scrollY);
        Size = new Vector2((Tiles + 2 * columns) * tex.X, (1 + 2 * rows) * tex.Y);
    }

    public override void _Ready()
    {
        DefaultTexture = ResourceLoader.Load<Texture2D>("res://assets/battlebacks/battleback_vf_default.png");
    }

    public override void _Process(double delta)
    {
        if (CurrentBattleback is ScrollingBattleback scrolling && Texture != null && scrolling.ScrollSpeed != Vector2.Zero)
        {
            Vector2 tex = Texture.GetSize();
            ScrollOffset = new Vector2(Mathf.PosMod(ScrollOffset.X + scrolling.ScrollSpeed.X * (float)delta, tex.X),
                Mathf.PosMod(ScrollOffset.Y + scrolling.ScrollSpeed.Y * (float)delta, tex.Y));
            ApplyLayout();
        }

        if (CurrentBattleback != null && CurrentBattleback.FrameCount > 1)
        {
            Elapsed += delta;

            // track accumulated time across as many frames as it covers,
            // so fast-forwards don't leave leftover time
            bool frameChanged = false;
            for (int i = 0; i < CurrentBattleback.FrameCount; i++)
            {
                double delay = CurrentBattleback.GetFrameDelay(CurrentFrame);
                if (delay <= 0d)
                {
                    Elapsed = 0d;
                    break;
                }
                if (Elapsed < delay)
                    break;
                Elapsed -= delay;
                CurrentFrame = (CurrentFrame + 1) % CurrentBattleback.FrameCount;
                frameChanged = true;
            }

            if (frameChanged)
                Texture = CurrentBattleback.GetFrame(CurrentFrame);
        }
    }
}
