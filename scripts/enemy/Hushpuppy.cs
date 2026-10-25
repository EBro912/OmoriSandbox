using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class Hushpuppy : Enemy
{
    public override string Name => "HUSHPUPPY";
    public override Vector2 InfoBoxOffset => new(0, -175);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/hushpuppy.tres");
    protected override Stats Stats => new(430, 215, 40, 33, 49, 10, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["HSPAttack", "HSPDoNothing", "HSPMuffledScreams"];

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["HSPAttack"]);

        switch (CurrentEmotion.Group?.Id)
        {
            case "happy":
                if (Roll() < 36)
                    goto attack;
                if (Roll() < 26)
                    goto doNothing;
                goto muffledScreams;
            case "sad":
                if (Roll() < 26)
                    goto attack;
                if (Roll() < 66)
                    goto doNothing;
                goto muffledScreams;
            case "angry":
                if (Roll() < 81)
                    goto attack;
                if (Roll() < 26)
                    goto doNothing;
                goto muffledScreams;
            default:
                if (Roll() < 66)
                    goto attack;
                if (Roll() < 36)
                    goto doNothing;
                goto muffledScreams;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["HSPAttack"]);
    doNothing:
        return new BattleCommand(this, this, Skills["HSPDoNothing"]);
    muffledScreams:
        return new BattleCommand(this, SelectTarget(), Skills["HSPMuffledScreams"]);
    }
}
