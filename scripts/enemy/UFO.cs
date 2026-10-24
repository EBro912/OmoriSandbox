using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class UFO : Enemy
{
    public override string Name => "U.F.O.";
    public override Vector2 InfoBoxOffset => new(0, -105);
    public override Vector2 DamageNumberOffset => new(0, 40);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/ufo.tres");
    protected override Stats Stats => new(20, 10, 10, 2, 15, 5, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["UFOOrangeBeam", "UFOAttack", "UFODoNothing", "UFOStrangeBeam"];

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["UFOOrangeBeam"]);

        switch (CurrentEmotion.Group?.Id)
        {
            case "happy":
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 11)
                    goto doNothing;
                if (Roll() < 31)
                    goto orangeBeam;
                goto strangeBeam;
            case "sad":
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 51)
                    goto doNothing;
                if (Roll() < 21)
                    goto orangeBeam;
                goto strangeBeam;
            case "angry":
                if (Roll() < 56)
                    goto attack;
                if (Roll() < 11)
                    goto doNothing;
                if (Roll() < 56)
                    goto orangeBeam;
                goto strangeBeam;
            default:
                if (Roll() < 46)
                    goto attack;
                if (Roll() < 31)
                    goto doNothing;
                if (Roll() < 46)
                    goto orangeBeam;
                goto strangeBeam;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["UFOAttack"]);
    doNothing:
        return new BattleCommand(this, this, Skills["UFODoNothing"]);
    orangeBeam:
        return new BattleCommand(this, SelectTargetWithLowest(member => (float)member.CurrentJuice / member.CurrentStats.MaxJuice), Skills["UFOOrangeBeam"]);
    strangeBeam:
        return new BattleCommand(this, SelectTarget(), Skills["UFOStrangeBeam"]);
    }
}
