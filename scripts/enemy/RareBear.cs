using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class RareBear : Enemy
{
    public override string Name => "RARE BEAR";
    public override Vector2 InfoBoxOffset => new(0, -250);
    public override Vector2 DamageNumberOffset => new(0, -120);
    public override bool InfoBoxCursorAboveBox => true;
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/rare_bear.tres");
    protected override Stats Stats => new(700, 225, 15, 15, 10, 10, 95);

    private Stats GetStatsForEmotion()
    {
        return CurrentEmotion.Group?.Id == "angry"
            ? new Stats(700, 225, 45, 30, 41, 15, 100)
            : new Stats(700, 225, 15, 15, 10, 10, 95);
    }

    public override Stats GetBaseStats()
    {
        return GetStatsForEmotion() + AdjustedStats;
    }

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["RBAttack", "RBRoar"];

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["RBAttack"]);

        switch (CurrentEmotion.Group?.Id)
        {
            case "angry":
                goto attack;
            default:
                if (Roll() < 21)
                    goto attack;
                goto roar;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["RBAttack"]);
    roar:
        return new BattleCommand(this, this, Skills["RBRoar"]);
    }
}
