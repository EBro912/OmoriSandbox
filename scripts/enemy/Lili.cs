using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class Lili : Enemy
{
    public override string Name => "???";
    public override Vector2 InfoBoxOffset => new(0, -200);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/lili.tres");
    protected override Stats Stats => new(540, 270, 70, 70, 70, 15, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "afraid";
    }

    protected override string[] EquippedSkills => ["LILAttack", "LILLiar", "LILSadEyes"];

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
            return new BattleCommand(this, observe, Skills["LILAttack"]);

        if (Roll() < 41)
            goto attack;
        if (Roll() < 31)
            goto liar;
        goto sadEyes;
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["LILAttack"]);
    liar:
        return new BattleCommand(this, SelectTarget(), Skills["LILLiar"]);
    sadEyes:
        return new BattleCommand(this, SelectTarget(), Skills["LILSadEyes"]);
    }
}
