using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class HorseHead : Enemy
{
    public override string Name => "HORSE HEAD";
    public override Vector2 InfoBoxOffset => CurrentEmotion.Group == null ? new(0, -150) : new(0, -200);
    public override Vector2 DamageNumberOffset => new(0, 40);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/horse_head.tres");
    protected override Stats Stats => new(230, 95, 20, 25, 30, 10, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["HHDAttack", "HHDDoNothing", "HHDLick", "HHDWhinny"];

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["HHDAttack"]);

        switch (CurrentEmotion.Group?.Id)
        {
            case "happy":
                if (Roll() < 36)
                    goto attack;
                if (Roll() < 31)
                    goto doNothing;
                goto lick;
            case "sad":
                if (Roll() < 36)
                    goto attack;
                if (Roll() < 56)
                    goto doNothing;
                if (Roll() < 36)
                    goto lick;
                goto whinny;
            case "angry":
                if (Roll() < 61)
                    goto attack;
                if (Roll() < 26)
                    goto doNothing;
                if (Roll() < 61)
                    goto lick;
                goto whinny;
            default:
                if (Roll() < 56)
                    goto attack;
                if (Roll() < 31)
                    goto doNothing;
                if (Roll() < 36)
                    goto lick;
                goto whinny;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["HHDAttack"]);
    doNothing:
        return new BattleCommand(this, this, Skills["HHDDoNothing"]);
    lick:
        return new BattleCommand(this, SelectTarget(), Skills["HHDLick"]);
    whinny:
        return new BattleCommand(this, this, Skills["HHDWhinny"]);
    }
}
