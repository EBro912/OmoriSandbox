using Godot;

namespace OmoriSandbox;

internal partial class EnergyDots : Sprite2D
{
    [Export]
    private double Speed = 20f;

    private static readonly Vector2 DreamworldOrigin = new(363, 1);

    private ShaderMaterial Shader;

    internal Vector2 Origin { get; set; } = DreamworldOrigin;

    public override void _Ready()
    {
        Shader = Material as ShaderMaterial;
    }

    public void Tick(double delta)
    {
        RegionRect = new Rect2(Origin.X, Origin.Y + BattleManager.Instance.Energy * 28, 289, 28);
        double shift = (DreamworldOrigin.X - Origin.X) * 289 / Texture.GetWidth();

        double sliceX = Shader.GetShaderParameter("slice_x").AsDouble() + shift;
        sliceX += Speed * delta;

        if (sliceX is > 200 or < 90)
            sliceX = 90;

        Shader.SetShaderParameter("slice_x", sliceX - shift);
    }
}