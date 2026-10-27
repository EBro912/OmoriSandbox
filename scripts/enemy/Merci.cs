using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class Merci : Enemy
{
    public override string Name => "???";
    public override Vector2 InfoBoxOffset => new(20, -240);
    public override Vector2 DamageNumberOffset => new(0, -300);
    public override bool InfoBoxCursorAboveBox => true;
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/merci.tres");
    protected override Stats Stats => new(660, 330, 82, 13, 87, 10, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "afraid";
    }

    protected override string[] EquippedSkills => ["MERAttack", "MERLiar", "MERMelody", "MERScream"];

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
            return new BattleCommand(this, observe, Skills["MERAttack"]);

        if (Roll() < 66)
            goto attack;
        if (Roll() < 36)
            goto liar;
        if (Roll() < 36)
            goto melody;
        goto scream;
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["MERAttack"]);
    liar:
        return new BattleCommand(this, SelectTarget(), Skills["MERLiar"]);
    melody:
        return new BattleCommand(this, SelectTarget(), Skills["MERMelody"]);
    scream:
        return new BattleCommand(this, SelectTarget(), Skills["MERScream"]);
    }
}
