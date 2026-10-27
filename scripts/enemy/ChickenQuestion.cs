using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class ChickenQuestion : Enemy
{
    public override string Name => "CHICKEN?";
    public override Vector2 InfoBoxOffset => new(0, -250);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/chicken.tres");
    protected override Stats Stats => new(150, 300, 10, 10, 95, 10, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["CHKRunAway", "CHKDoNothing"];

    public override BattleCommand ProcessAI()
    {
        switch (CurrentEmotion.Group?.Id)
        {
            case "sad":
                goto doNothing;
            default:
                if (Roll() < 91)
                    goto runAway;
                goto doNothing;
        }
    runAway:
        return new BattleCommand(this, this, Skills["CHKRunAway"]);
    doNothing:
        return new BattleCommand(this, this, Skills["CHKDoNothing"]);
    }
}
