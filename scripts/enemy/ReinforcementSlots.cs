using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Godot;

namespace OmoriSandbox.Actors;

// a wrapper that mirrors the very consistent (sarcasm) reinforcement summoning various omori enemies do
// groups enemies and their summons into "families" and handles summon slots, offsets relative to the summoner, and other similar behavior 
internal static class ReinforcementSlots
{
    internal sealed record Member(string Id, float CenterDy);

    internal sealed record Slot(float Dx, Member[] Pool);

    internal sealed class Family
    {
        internal Family(params Slot[] slots)
        {
            Slots = slots;
        }

        internal Slot[] Slots { get; }

        internal int FirstSlot { get; init; } = 1;

        internal int Blanks { get; init; } = 1;

        internal int LayerDelta { get; init; }

        internal bool FallsOffScreen { get; init; } = true;

        internal bool PerSummoner { get; init; }
    }

    private sealed record Claim(Family Family, int Number, Vector2 Anchor, Enemy Owner = null);

    private static readonly ConditionalWeakTable<Enemy, Claim> Claims = new();

    private static readonly Member[] WormholeSwap = [new("SpaceBunny", -15f), new("UFO", 23f)];

    private static readonly Member[] PastaSwap =
        [new("Copypasta", 0f), new("Copypasta", 0f), new("Copypasta", 0f), new("Copypasta", 0f), new("Creepypasta", -17.5f)];

    internal static readonly Family Wormhole = new(
        new Slot(0f, [new Member("Wormhole", 0f)]),
        new Slot(-120f, WormholeSwap),
        new Slot(120f, WormholeSwap));

    internal static readonly Family Copypasta = new(
        new Slot(0f, [new Member("Copypasta", 0f)]),
        new Slot(-104f, PastaSwap),
        new Slot(104f, PastaSwap),
        new Slot(-200f, PastaSwap),
        new Slot(216f, [new Member("Creepypasta", -17.5f)]));

    internal static readonly Family FishBunny = new(
        new Slot(0f, [new Member("FishBunny", 0f)]),
        new Slot(-168f, [new Member("FishBunny", 0f)]),
        new Slot(168f, [new Member("FishBunny", 0f)]));

    private static readonly Member[] Tentacles = [new("Tentacle", -67f)];

    private static readonly Member[] BreadSwap =
        [new("BunBunny", -7f), new("Creepypasta", 7f), new("Slice", -18f), new("Sourdough", -12f), new("Sesame", -12f), new("LivingBread", -3f)];

    private static readonly Member[] EpilogueBreadSwap =
        [new("Slice (Epilogue)", -18f), new("Sourdough (Epilogue)", -12f), new("Sesame (Epilogue)", -12f)];

    internal static readonly Family KingCrawler = new(
        new Slot(-134f, [new Member("LostSproutMole", 17.5f)]),
        new Slot(186f, [new Member("LostSproutMole", 17.5f)]))
    {
        FirstSlot = 2, Blanks = 0, LayerDelta = 1, PerSummoner = true
    };

    internal static readonly Family KingCrawlerBossRush = new(
        new Slot(-166f, [new Member("LostSproutMole (King Crawler)", 17.5f)]),
        new Slot(138f, [new Member("LostSproutMole (King Crawler)", 13.5f)]))
    {
        FirstSlot = 2, Blanks = 0, LayerDelta = 1, PerSummoner = true
    };

    internal static readonly Family MrJawsum = new(
        new Slot(-136f, [new Member("GatorGuyJawsum", 71.5f)]),
        new Slot(168f, [new Member("GatorGuyJawsum", 71.5f)]))
    {
        FirstSlot = 2, Blanks = 0, LayerDelta = -1, PerSummoner = true
    };

    internal static readonly Family MrJawsumBossRush = new(
        new Slot(-120f, [new Member("GatorGuyJawsum (Boss Rush)", 59.5f)]),
        new Slot(152f, [new Member("GatorGuyJawsum (Boss Rush)", 59.5f)]))
    {
        FirstSlot = 2, Blanks = 0, LayerDelta = -1, PerSummoner = true
    };

    internal static readonly Family Abbi = new(
        new Slot(-247f, Tentacles),
        new Slot(-103f, Tentacles),
        new Slot(41f, Tentacles),
        new Slot(201f, Tentacles))
    {
        FirstSlot = 2, Blanks = 0, LayerDelta = 1, PerSummoner = true
    };

    internal static readonly Family Recyclepath = new(
        new Slot(-247f, [new Member("RecycultistRight", 52f)]),
        new Slot(233f, [new Member("RecycultistLeft", 52f)]))
    {
        FirstSlot = 2, Blanks = 0, LayerDelta = -1, FallsOffScreen = false, PerSummoner = true
    };

    internal static readonly Family UnbreadTwins = new(
        new Slot(-276f, BreadSwap),
        new Slot(188f, BreadSwap))
    {
        Blanks = 0, LayerDelta = -1, PerSummoner = true
    };

    internal static readonly Family UnbreadTwinsEpilogue = new(
        new Slot(-276f, EpilogueBreadSwap),
        new Slot(204f, EpilogueBreadSwap))
    {
        FirstSlot = 2, Blanks = 0, LayerDelta = -1, PerSummoner = true
    };

    private static Claim ClaimOf(Enemy enemy, Family family) =>
        Claims.TryGetValue(enemy, out Claim claim) && claim?.Family == family ? claim : null;

    private static Claim ClaimOf(Enemy enemy, Family family, Enemy owner) =>
        ClaimOf(enemy, family) is { } claim && claim.Owner == owner ? claim : null;

    private static bool InPool(Slot slot, Enemy enemy) => slot.Pool.Any(member => member.Id == enemy.GetType().Name);

    internal static int SlotOf(Enemy enemy, Family family) => ClaimOf(enemy, family)?.Number ?? 0;

    internal static void AddTroopMember(Enemy owner, Family family, Enemy member)
    {
        if (member != null)
            Claims.AddOrUpdate(member, new Claim(family, 0, owner.CenterPoint, family.PerSummoner ? owner : null));
    }

    private static List<Enemy> OwnTroop(Family family, Enemy owner) =>
        BattleManager.Instance.GetAllEnemies().Where(enemy => (owner != null && enemy == owner) || ClaimOf(enemy, family, owner) != null).ToList();

    internal static void ClaimPresetSlots(Family family)
    {
        List<Enemy> enemies = BattleManager.Instance.GetAllEnemies();
        if (enemies.Any(enemy => ClaimOf(enemy, family) != null))
            return;
        int first = enemies.FindIndex(enemy => InPool(family.Slots[0], enemy));
        if (first == -1)
            return;
        Vector2 anchor = enemies[first].CenterPoint;
        Claims.AddOrUpdate(enemies[first], new Claim(family, 1, anchor));
        foreach (Enemy enemy in enemies.Skip(first + 1))
        {
            int number = Enumerable.Range(2, family.Slots.Length - 1)
                .FirstOrDefault(k => InPool(family.Slots[k - 1], enemy) && enemies.All(other => SlotOf(other, family) != k));
            if (number != 0)
                Claims.AddOrUpdate(enemy, new Claim(family, number, anchor));
        }
    }

    internal static Enemy Summon(Enemy user, Family family)
    {
        List<Enemy> enemies = BattleManager.Instance.GetAllEnemies();
        Enemy owner = family.PerSummoner ? user : null;
        int number = Enumerable.Range(family.FirstSlot, family.Slots.Length)
            .FirstOrDefault(k => !enemies.Any(enemy => ClaimOf(enemy, family, owner)?.Number == k && !enemy.IsToast && !enemy.HasEscaped));
        if (number == 0)
            return null;
        Vector2 anchor = enemies.Select(enemy => ClaimOf(enemy, family, owner)).FirstOrDefault(claim => claim != null)?.Anchor ?? user.CenterPoint;
        Slot slot = family.Slots[number - family.FirstSlot];
        Member member = slot.Pool.Length == 1 ? slot.Pool[0] : slot.Pool[GameManager.Instance.Random.RandiRange(0, slot.Pool.Length - 1)];
        int layer = family.LayerDelta < 0 ? Math.Max(0, user.Layer + family.LayerDelta) : user.Layer + family.LayerDelta;
        Enemy summoned = BattleManager.Instance.SummonEnemy(member.Id, anchor + new Vector2(slot.Dx, member.CenterDy),
            fallsOffScreen: family.FallsOffScreen, layer: layer);
        if (summoned == null)
            return null;
        Claims.AddOrUpdate(summoned, new Claim(family, number, anchor, owner));
        List<Enemy> troop = OwnTroop(family, owner);
        int index = number - 1 - family.Blanks;
        if (index >= 0 && index < troop.Count)
        {
            Enemy stunned = troop[index];
            if (!stunned.IsToast && !stunned.HasEscaped)
            {
                stunned.AddStatModifier("CallForFriendDelay", silent: true);
                stunned.ClearResult();
            }
        }
        return summoned;
    }
}
