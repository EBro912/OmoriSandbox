using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class DustBunny : Enemy
{
    public override string Name => "DUST BUNNY";
    public override Vector2 InfoBoxOffset => CurrentEmotion.Group == null ? new(0, -150) : new(0, -180);
    protected internal override bool ObserveHasMulti => true;
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/dust_bunny.tres");
    protected override Stats Stats => new(20, 10, 10, 1, 15, 1, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["DBDoNothing", "DBScatter"];

    public override BattleCommand ProcessAI()
    {
        switch (CurrentEmotion.Group?.Id)
        {
            case "sad":
                if (Roll() < 76)
                    goto doNothing;
                goto scatter;
            default:
                if (Roll() < 51)
                    goto doNothing;
                goto scatter;
        }
    doNothing:
        return new BattleCommand(this, this, Skills["DBDoNothing"]);
    scatter:
        return new BattleCommand(this, SelectAllTargets(), Skills["DBScatter"]);
    }
}
