using Godot;

namespace OmoriSandbox;

internal partial class BattlebackDisplayComponent : TextureRect
{
    // the number of horizontal repeats on each side
    [Export] private int Tiles = 1;

    private const float ScreenWidth = 640f;
    private const float ScreenHeight = 480f;

    private IBattleback CurrentBattleback;
    private int CurrentFrame;
    private double Elapsed;
    private Texture2D DefaultTexture;

    public void SetBattleback(string name)
    {
        if (BattlebackManager.Instance.TryGetBattleback(name, out IBattleback battleback))
        {
            CurrentBattleback = battleback;
            CurrentFrame = 0;
            Elapsed = 0;
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
        ApplyLayout();
    }

    // center the middle tile on the screen so battlebacks with non-640x480 dimensions display centered
    private void ApplyLayout()
    {
        if (Texture == null)
            return;

        Vector2 tex = Texture.GetSize();
        Size = new Vector2(Tiles * tex.X, tex.Y);
        // round so odd-dimension textures keep pixel alignment
        Position = new Vector2(Mathf.Round(ScreenWidth / 2f - Tiles * tex.X / 2f), Mathf.Round((ScreenHeight - tex.Y) / 2f));
    }

    public override void _Ready()
    {
        DefaultTexture = ResourceLoader.Load<Texture2D>("res://assets/battlebacks/battleback_vf_default.png");
    }

    public override void _Process(double delta)
    {
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
