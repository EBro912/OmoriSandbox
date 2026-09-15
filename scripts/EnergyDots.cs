using Godot;

namespace OmoriSandbox;

internal partial class EnergyDots : Sprite2D
{
    [Export]
    private double Speed = 20f;

    private ShaderMaterial Shader;

    public override void _Ready()
    {
        Shader = Material as ShaderMaterial;
    }

    public void Tick(double delta)
    {
        RegionRect = new Rect2(363, 1 + BattleManager.Instance.Energy * 28, 289, 28);

        double sliceX = Shader.GetShaderParameter("slice_x").AsDouble();
        sliceX += Speed * delta;

        if (sliceX > 200)
            sliceX = 90;

        Shader.SetShaderParameter("slice_x", sliceX);
    }
}