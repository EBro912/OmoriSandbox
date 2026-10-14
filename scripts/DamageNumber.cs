using System.Collections.Generic;
using Godot;
using System.Linq;

namespace OmoriSandbox;

internal partial class DamageNumber : Node2D
{
    private static Dictionary<Vector2, DamageNumber> DamageNumbers = [];
    private static readonly List<DamageNumber> PartyNumbers = [];
    
    private int[] Digits;
    private DamageType DamageType;
    private bool Critical;
    internal readonly bool Party;
    private static Texture2D Texture;

    private const int CELL_WIDTH = 32;
    private const int CELL_HEIGHT = 45;
    private readonly List<Sprite2D> Cells = [];
    private int Duration = 128;
    private float FlashAlpha;
    private int FlashDuration;
    private bool Started;

    public DamageNumber(int damage, Vector2 position, DamageType type = DamageType.Damage, bool critical = false, bool party = false)
    {
        Digits = damage.ToString().Select(digit => (int)char.GetNumericValue(digit)).ToArray();
        DamageType = type;
        Critical = critical;
        Party = party;
        ZAsRelative = false;
        ZIndex = 10;
        Position = position;
        if (Party)
        {
            PartyNumbers.Add(this);
            return;
        }
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
        foreach (DamageNumber number in DamageNumbers.Values.Concat(PartyNumbers).Where(IsInstanceValid))
            number.Despawn();
        DamageNumbers.Clear();
        PartyNumbers.Clear();
    }

    public override void _Ready()
    {
        if (DamageType == DamageType.Miss)
            AddCell(new Rect2(0, 4 * CELL_HEIGHT, 48, CELL_HEIGHT), 0f);
        else
            for (int i = 0; i < Digits.Length; i++)
                AddCell(new Rect2(CELL_WIDTH * Digits[i], CELL_HEIGHT * (int)DamageType, CELL_WIDTH, CELL_HEIGHT),
                    (i - (Digits.Length - 1) / 2f) * (CELL_WIDTH - 8));
        if (Critical)
        {
            FlashAlpha = 160f / 255f;
            FlashDuration = 60;
        }
    }

    private void AddCell(Rect2 region, float x)
    {
        Sprite2D sprite = new()
        {
            Texture = Texture,
            RegionEnabled = true,
            RegionRect = region,
            Centered = false,
            Offset = new Vector2(-region.Size.X / 2f, -region.Size.Y),
            Position = new Vector2(x + 1f, 44f - 50f),
            Modulate = Colors.Transparent,
            UseParentMaterial = true
        };
        AddChild(sprite);
        Cells.Add(sprite);
    }

    public override void _PhysicsProcess(double delta)
    {
        Step();
        if (!Party && Started && !IsQueuedForDeletion())
            Step();
        Started = true;
    }

    private void Step()
    {
        if (Duration > 0)
        {
            bool previousDone = true;
            foreach (Sprite2D cell in Cells)
            {
                if (previousDone)
                {
                    float alpha = Duration > 10 ? Mathf.Min(cell.Modulate.A + 20f / 255f, 1f) : cell.Modulate.A;
                    cell.Position = new Vector2(cell.Position.X, Mathf.Min(cell.Position.Y + 6f, 44f));
                    cell.Modulate = new Color(1f, 1f - FlashAlpha, 1f - FlashAlpha, alpha);
                }
                previousDone = cell.Position.Y >= 44f - 20f;
            }
            if (previousDone)
                Duration--;
        }
        if (FlashDuration > 0)
        {
            int d = FlashDuration--;
            FlashAlpha *= (d - 1f) / d;
        }
        if (Duration < 10)
            Modulate = new Color(Modulate, Duration / 10f);
        if (Duration <= 0)
        {
            Forget();
            QueueFree();
        }
    }

    public void Despawn()
    {
        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(this, "modulate:a", 0f, 0.1f);
        tween.TweenCallback(Callable.From(() =>
        {
            Forget();
            QueueFree();
        }));
    }

    private void Forget()
    {
        if (Party)
            PartyNumbers.Remove(this);
        else
            DamageNumbers.Remove(Position);
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
