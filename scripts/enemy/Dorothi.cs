using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class Dorothi : Enemy
{
    public override string Name => "???";
    public override Vector2 InfoBoxOffset => new(0, -135);
    public override Vector2 DamageNumberOffset => new(0, -140);
    public override bool InfoBoxCursorAboveBox => true;
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/dorothi.tres");
    protected override Stats Stats => new(1000, 500, 75, 13, 91, 15, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "afraid";
    }

    protected override string[] EquippedSkills => ["DORAttack", "CINLiar", "DORAttackTwice", "DORSelfHappy"];

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
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["DORAttack"]);

        if (Roll() < 66)
            goto attack;
        if (Roll() < 41)
            goto liar;
        if (Roll() < 36)
            goto attackTwice;
        goto selfHappy;
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["DORAttack"]);
    liar:
        return new BattleCommand(this, SelectTarget(), Skills["CINLiar"]);
    attackTwice:
        return new BattleCommand(this, SelectTargets(2), Skills["DORAttackTwice"]);
    selfHappy:
        return new BattleCommand(this, this, Skills["DORSelfHappy"]);
    }
}
