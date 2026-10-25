using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class ToastGhost : Enemy
{
    public override string Name => "TOAST GHOST";
    public override Vector2 InfoBoxOffset => new(0, -175);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/toast_ghost.tres");
    protected override Stats Stats => new(200, 999, 15, 3, 10, 10, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["TGAttack", "TGDoNothing"];

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["TGAttack"]);

        switch (CurrentEmotion.Group?.Id)
        {
            case "sad":
                if (Roll() < 61)
                    goto attack;
                goto doNothing;
            case "angry":
                if (Roll() < 91)
                    goto attack;
                goto doNothing;
            default:
                if (Roll() < 71)
                    goto attack;
                goto doNothing;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["TGAttack"]);
    doNothing:
        return new BattleCommand(this, this, Skills["TGDoNothing"]);
    }
}
