using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class Celery : Enemy
{
    public override string Name => "CELERY";
    public override Vector2 InfoBoxOffset => new(0, -150);
    public override Vector2 DamageNumberOffset => new(0, 20);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/celery.tres");
    protected override Stats Stats => new(935, 467, 80, 20, 65, 10, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["CELAttack", "CELDoNothing"];

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["CELAttack"]);

        switch (CurrentEmotion.Group?.Id)
        {
            case "sad":
                if (Roll() < 41)
                    goto attack;
                goto doNothing;
            case "angry":
                if (Roll() < 61)
                    goto attack;
                goto doNothing;
            default:
                if (Roll() < 51)
                    goto attack;
                goto doNothing;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["CELAttack"]);
    doNothing:
        return new BattleCommand(this, this, Skills["CELDoNothing"]);
    }
}
