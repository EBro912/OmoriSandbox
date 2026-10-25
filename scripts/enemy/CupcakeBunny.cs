using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class CupcakeBunny : Enemy
{
    public override string Name => "CUPCAKE BUNNY";
    public override Vector2 InfoBoxOffset => new(0, -175);
    public override Vector2 DamageNumberOffset => new(0, 40);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/cupcake_bunny.tres");
    protected override Stats Stats => new(150, 100, 15, 15, 5, 10, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["CCBAttack", "CCBDoNothing", "CCBSprinkles"];

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["CCBAttack"]);

        switch (CurrentEmotion.Group?.Id)
        {
            case "sad":
                if (Roll() < 26)
                    goto attack;
                if (Roll() < 46)
                    goto doNothing;
                goto sprinkles;
            case "angry":
                if (Roll() < 56)
                    goto attack;
                if (Roll() < 26)
                    goto doNothing;
                goto sprinkles;
            default:
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 21)
                    goto doNothing;
                goto sprinkles;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["CCBAttack"]);
    doNothing:
        return new BattleCommand(this, this, Skills["CCBDoNothing"]);
    sprinkles:
        return new BattleCommand(this, SelectEnemy(), Skills["CCBSprinkles"]);
    }
}
