using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class Cilantro : Enemy
{
    public override string Name => "CILANTRO";
    public override Vector2 InfoBoxOffset => new(0, -150);
    public override Vector2 DamageNumberOffset => new(0, 40);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/cilantro.tres");
    protected override Stats Stats => new(715, 357, 70, 5, 90, 30, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["CILAttack", "CILDoNothing"];

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["CILAttack"]);

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
        return new BattleCommand(this, SelectTarget(), Skills["CILAttack"]);
    doNothing:
        return new BattleCommand(this, this, Skills["CILDoNothing"]);
    }
}
