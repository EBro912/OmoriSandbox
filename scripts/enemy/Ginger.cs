using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class Ginger : Enemy
{
    public override string Name => "GINGER";
    public override Vector2 InfoBoxOffset => new(0, -150);
    public override Vector2 DamageNumberOffset => new(0, 40);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/ginger.tres");
    protected override Stats Stats => new(590, 1180, 67, 86, 5, 10, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["GINAttack", "GINDoNothing", "GINSoothe"];

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["GINAttack"]);

        switch (CurrentEmotion.Group?.Id)
        {
            case "happy":
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 26)
                    goto doNothing;
                goto soothe;
            case "sad":
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 56)
                    goto doNothing;
                goto soothe;
            case "angry":
                if (Roll() < 56)
                    goto attack;
                if (Roll() < 31)
                    goto doNothing;
                goto soothe;
            default:
                if (Roll() < 56)
                    goto attack;
                if (Roll() < 36)
                    goto doNothing;
                goto soothe;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["GINAttack"]);
    doNothing:
        return new BattleCommand(this, this, Skills["GINDoNothing"]);
    soothe:
        return new BattleCommand(this, SelectTarget(), Skills["GINSoothe"]);
    }
}
