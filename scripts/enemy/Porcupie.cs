using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class Porcupie : Enemy
{
    public override string Name => "PORCUPIE";
    public override Vector2 InfoBoxOffset => new(0, -100);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/porcupie.tres");
    protected override Stats Stats => new(370, 740, 50, 75, 5, 10, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["PCPPierce", "PCPDoNothing"];

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["PCPPierce"]);

        switch (CurrentEmotion.Group?.Id)
        {
            case "happy":
                if (Roll() < 31)
                    goto doNothing;
                goto pierce;
            case "sad":
                if (Roll() < 46)
                    goto doNothing;
                goto pierce;
            case "angry":
                if (Roll() < 16)
                    goto doNothing;
                goto pierce;
            default:
                if (Roll() < 36)
                    goto doNothing;
                goto pierce;
        }
    doNothing:
        return new BattleCommand(this, this, Skills["PCPDoNothing"]);
    pierce:
        return new BattleCommand(this, SelectTarget(), Skills["PCPPierce"]);
    }
}
