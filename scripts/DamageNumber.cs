using Godot;
using System.Linq;
public partial class DamageNumber : Node2D
{
    private int[] Digits;
    private DamageType DamageType;
    private static Texture2D Texture;

    private const int WIDTH = 30;
    private const int HEIGHT = 42;
    private const float SPACING = 25f;
    private const float SCALE = 1f;

    public DamageNumber(int damage, DamageType type = DamageType.Damage)
    {
        Digits = damage.ToString().Select(digit => (int)char.GetNumericValue(digit)).ToArray();
        DamageType = type;
        Modulate = Colors.Transparent;
    }

    // since we spawn in damage numbers we need to cache this texture from elsewhere
    public static void CacheTexture(Texture2D texture)
    {
        Texture ??= texture;
    }

    public override void _Ready()
    {
        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(this, "modulate:a", 1f, 0.1f);

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

    public void Despawn()
    {
        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(this, "modulate:a", 0f, 0.1f);
        tween.TweenCallback(Callable.From(QueueFree));
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
