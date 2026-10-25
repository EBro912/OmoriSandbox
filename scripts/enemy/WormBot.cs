using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class WormBot : Enemy
{
    public override string Name => "WORM-BOT";
    public override Vector2 InfoBoxOffset => new(0, -300);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/worm_bot.tres");
    protected override Stats Stats => new(410, 205, 53, 46, 30, 15, 95);

    private Stats GetStatsForEmotion()
    {
        return CurrentEmotion.Group?.Id switch
        {
            "happy" => new Stats(410, 205, 53, 46, 30, 10, 95),
            "sad" => new Stats(410, 205, 53, 46, 30, 10, 95),
            "angry" => new Stats(410, 205, 53, 46, 30, 10, 95),
            _ => new Stats(410, 205, 53, 46, 30, 15, 95)
        };
    }

    public override Stats GetBaseStats()
    {
        return GetStatsForEmotion() + AdjustedStats;
    }

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["WBTLaser", "WBTAttack", "WBTDoNothing"];

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["WBTLaser"]);

        switch (CurrentEmotion.Group?.Id)
        {
            case "happy":
                if (Roll() < 51)
                    goto attack;
                if (Roll() < 31)
                    goto doNothing;
                goto laser;
            case "sad":
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 56)
                    goto doNothing;
                goto laser;
            case "angry":
                if (Roll() < 61)
                    goto attack;
                if (Roll() < 11)
                    goto doNothing;
                goto laser;
            default:
                if (Roll() < 61)
                    goto attack;
                if (Roll() < 41)
                    goto doNothing;
                goto laser;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["WBTAttack"]);
    doNothing:
        return new BattleCommand(this, this, Skills["WBTDoNothing"]);
    laser:
        return new BattleCommand(this, SelectTarget(), Skills["WBTLaser"]);
    }
}
