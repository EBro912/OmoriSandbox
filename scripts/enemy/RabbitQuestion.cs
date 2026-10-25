using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class RabbitQuestion : Enemy
{
    public override string Name => "RABBIT?";
    public override Vector2 InfoBoxOffset => new(0, -225);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/rabbit_question.tres");
    protected override Stats Stats => new(1244, 622, 95, 70, 80, 15, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["RBQAttack", "RBQDoNothing", "RBQBeCute", "RBQSadEyes"];

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["RBQAttack"]);

        switch (CurrentEmotion.Group?.Id)
        {
            case "happy":
                if (Roll() < 41)
                    goto attack;
                if (Roll() < 26)
                    goto doNothing;
                goto beCute;
            case "sad":
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 56)
                    goto doNothing;
                goto sadEyes;
            case "angry":
                if (Roll() < 61)
                    goto attack;
                if (Roll() < 21)
                    goto doNothing;
                goto beCute;
            default:
                if (Roll() < 56)
                    goto attack;
                if (Roll() < 36)
                    goto doNothing;
                goto beCute;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["RBQAttack"]);
    doNothing:
        return new BattleCommand(this, this, Skills["RBQDoNothing"]);
    beCute:
        return new BattleCommand(this, SelectTarget(), Skills["RBQBeCute"]);
    sadEyes:
        return new BattleCommand(this, SelectTarget(), Skills["RBQSadEyes"]);
    }
}
