using Godot;
using System.Linq;

public partial class DamageNumber : Node2D
{
    private int[] Digits;
    private DamageType DamageType;
    private Texture2D Texture;

    private const int WIDTH = 30;
    private const int HEIGHT = 42;
    private const float SPACING = 25f;
    private const float SCALE = 1.2f;

    public DamageNumber(int damage, DamageType type = DamageType.Damage)
    {
        Digits = damage.ToString().Select(digit => (int)char.GetNumericValue(digit)).ToArray();
        DamageType = type;
        // TODO: cache
        Texture = GD.Load<Texture2D>("res://assets/system/Damage.png");
    }

    public override void _Ready()
    {
        if (DamageType == DamageType.Miss)
        {
            Sprite2D sprite = new()
            {
                Texture = Texture,
                RegionEnabled = true,
                RegionRect = new Rect2(0, 182, 62, HEIGHT)
            };
            AddChild(sprite);
            return;
        }

        float scaledSpacing = SPACING * SCALE;
        float totalWidth = (Digits.Length - 1) * scaledSpacing;
        for (int i = 0; i < Digits.Length; i++)
        {
            Sprite2D sprite = new()
            {
                Texture = Texture,
                RegionEnabled = true,
                RegionRect = new Rect2(32 * Digits[i], 48 * (int)DamageType, WIDTH, HEIGHT)
            };
            AddChild(sprite);

            sprite.Scale = new Vector2(SCALE, SCALE);
            float offset = i * scaledSpacing - totalWidth / 2f;
            sprite.Position = new Vector2(offset, 20);
        }
    }

    private int TypeOffset
    {
        // the sprites aren't lined up evenly in the sprite sheet...
        get
        {
            return DamageType switch
            {
                DamageType.Heal => 48,
                DamageType.JuiceLoss => 90,
                DamageType.JuiceGain => 138,
                _ => 0
            };
        }
    }
}

public enum DamageType
{
    Damage,
    Heal,
    JuiceLoss,
    JuiceGain,
    Miss
}
