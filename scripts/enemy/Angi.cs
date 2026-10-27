using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class Angi : Enemy
{
    public override string Name => "???";
    public override Vector2 InfoBoxOffset => new(-45, -370);
    public override Vector2 DamageNumberOffset => new(-40, -165);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/angi.tres");
    protected override Stats Stats => new(960, 480, 75, 74, 50, 10, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "afraid";
    }

    protected override string[] EquippedSkills => ["ANGCrunch", "ANGAttack", "ANGDoNothing"];

    public override Task OnStartOfBattle()
    {
        AddImmortal();
        return Task.CompletedTask;
    }

    public override async Task ProcessBattleConditions()
    {
        if (CurrentHP == 0 && IsImmortal)
            await GhostKids.Fall(this, null);
    }

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["ANGCrunch"]);

        if (Roll() < 66)
            goto attack;
        if (Roll() < 46)
            goto doNothing;
        goto crunch;
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["ANGAttack"]);
    doNothing:
        return new BattleCommand(this, this, Skills["ANGDoNothing"]);
    crunch:
        return new BattleCommand(this, SelectTarget(), Skills["ANGCrunch"]);
    }
}
