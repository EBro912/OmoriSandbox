using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class Salli : Enemy
{
    public override string Name => "???";
    public override Vector2 InfoBoxOffset => new(0, -220);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/salli.tres");
    protected override Stats Stats => new(500, 250, 70, 30, 100, 10, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "afraid";
    }

    protected override string[] EquippedSkills => ["SALAttack", "SALLiar", "SALSpeedUp"];

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
            return new BattleCommand(this, observe, Skills["SALAttack"]);

        if (Roll() < 66)
            goto attack;
        if (Roll() < 31)
            goto liar;
        goto speedUp;
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["SALAttack"]);
    liar:
        return new BattleCommand(this, SelectTarget(), Skills["SALLiar"]);
    speedUp:
        return new BattleCommand(this, this, Skills["SALSpeedUp"]);
    }
}
