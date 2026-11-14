using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class Cindi : Enemy
{
    public override string Name => "???";
    public override Vector2 InfoBoxOffset => new(0, -90);
    public override Vector2 DamageNumberOffset => new(0, -50);
    public override bool InfoBoxCursorAboveBox => true;
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/cindi.tres");
    protected override Stats Stats => new(700, 400, 90, 10, 30, 12, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "afraid";
    }

    protected override string[] EquippedSkills => ["CINSlam", "CINAttack", "CINLiar"];

    public override Task OnStartOfBattle()
    {
        AddImmortal();
        return Task.CompletedTask;
    }

    public override async Task ProcessBattleConditions()
    {
        if (CurrentHP == 0 && IsImmortal)
            await GhostKids.Fall(this, @"SUNNY...\. I'm... sorry...");
    }

    public override BattleCommand ProcessAI()
    {
        if (CanUse("CINSlam") && HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["CINSlam"]);

        if (Roll() < 66)
            return new BattleCommand(this, SelectTarget(), Skills["CINAttack"]);
        if (Roll() < 41)
            return new BattleCommand(this, SelectTarget(), Skills["CINLiar"]);
        if (CanUse("CINSlam"))
            return new BattleCommand(this, SelectTarget(), Skills["CINSlam"]);
        return null;
    }
}
