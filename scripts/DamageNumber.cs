using System.Collections.Generic;
using Godot;
using System.Linq;

namespace OmoriSandbox;

internal partial class DamageNumber : Node2D
{
    private static Dictionary<Vector2, DamageNumber> DamageNumbers = [];
    
    private int[] Digits;
    private DamageType DamageType;
    private bool Critical;
    private static Texture2D Texture;

    private const int WIDTH = 30;
    private const int HEIGHT = 42;
    private const float SPACING = 25f;
    private const float SCALE = 1f;

    public DamageNumber(int damage, Vector2 position, DamageType type = DamageType.Damage, bool critical = false)
    {
        Digits = damage.ToString().Select(digit => (int)char.GetNumericValue(digit)).ToArray();
        DamageType = type;
        Critical = critical;
        ZAsRelative = false;
        ZIndex = 5;
        Position = position;
        if (DamageNumbers.TryGetValue(Position, out DamageNumber number))
            ShiftUp(number);
        DamageNumbers[Position] = this;
    }

    private void ShiftUp(DamageNumber number)
    {
        DamageNumbers.Remove(number.Position);
        Vector2 newPos = number.Position + new Vector2(0, -40);
        if (DamageNumbers.TryGetValue(newPos, out DamageNumber otherNumber))
            ShiftUp(otherNumber);
        number.Position = newPos;
        DamageNumbers[newPos] = number;
    }

    // since we spawn in damage numbers we need to cache this texture from elsewhere
    public static void CacheTexture(Texture2D texture)
    {
        Texture ??= texture;
    }

    public static void DespawnAll()
    {
        foreach (DamageNumber number in DamageNumbers.Values.Where(IsInstanceValid))
            number.Despawn();
    }

    public override void _Ready()
    {
        Tween tween = GetTree().CreateTween().SetParallel();
        const float stagger = 0.05f;
        if (DamageType == DamageType.Miss)
        {
            Sprite2D sprite = new()
            {
                Texture = Texture,
                RegionEnabled = true,
                RegionRect = new Rect2(0, 182, 62, HEIGHT)
            };
            AddChild(sprite);
            tween.TweenProperty(sprite, "position:y", 20, 0.1f)
                .SetDelay(stagger)
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Cubic);
            tween.TweenProperty(sprite, "modulate:a", 1f, 0.1f).SetDelay(stagger);
            return;
        }
        
        const float scaledSpacing = SPACING * SCALE;
        float totalWidth = (Digits.Length - 1) * scaledSpacing;
        for (int i = 0; i < Digits.Length; i++)
        {
            Sprite2D sprite = new()
            {
                Texture = Texture,
                Modulate = Critical ? new Color(1f, 0f, 0f, 0f) : Colors.Transparent,
                RegionEnabled = true,
                RegionRect = new Rect2(32 * Digits[i], 48 * (int)DamageType, WIDTH, HEIGHT)
            };
            AddChild(sprite);

            sprite.Scale = new Vector2(SCALE, SCALE);
            float offset = i * scaledSpacing - totalWidth / 2f;
            sprite.Position = new Vector2(offset, -20);
            float delay = i * stagger;
            tween.TweenProperty(sprite, "position:y", 20, 0.1f)
                .SetDelay(delay)
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Cubic);
            tween.TweenProperty(sprite, "modulate:a", 1f, 0.1f).SetDelay(delay);
            if (Critical)
            {
                tween.TweenProperty(sprite, "modulate:g", 1f, 0.5f).SetDelay(delay);
                tween.TweenProperty(sprite, "modulate:b", 1f, 0.5f).SetDelay(delay);
            }
        }
    }

    public void Despawn()
    {
        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(this, "modulate:a", 0f, 0.1f);
        tween.TweenCallback(Callable.From(() =>
        {
            DamageNumbers.Remove(Position);
            QueueFree();
        }));
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
