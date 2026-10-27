using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class Nanci : Enemy
{
    public override string Name => "???";
    public override Vector2 InfoBoxOffset => new(0, -140);
    public override Vector2 DamageNumberOffset => new(0, -100);
    public override bool InfoBoxCursorAboveBox => true;
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/nanci.tres");
    protected override Stats Stats => new(520, 250, 83, 90, 44, 10, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "afraid";
    }

    protected override string[] EquippedSkills => ["NANAttack", "NANLiar", "NANSelfAngry"];

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
            return new BattleCommand(this, observe, Skills["NANAttack"]);

        if (Roll() < 76)
            goto attack;
        if (Roll() < 46)
            goto liar;
        goto selfAngry;
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["NANAttack"]);
    liar:
        return new BattleCommand(this, SelectTarget(), Skills["NANLiar"]);
    selfAngry:
        return new BattleCommand(this, this, Skills["NANSelfAngry"]);
    }
}
