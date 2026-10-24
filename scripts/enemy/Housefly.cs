using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class Housefly : Enemy
{
    public override string Name => "HOUSEFLY";
    // VANILLA BUG: HOUSEFLY's emotion entries have no StatusYOffset, so its info box drops to the sprite's origin while it feels an emotion
    public override Vector2 InfoBoxOffset => CurrentEmotion.Group == null ? new(0, -100) : Vector2.Zero;
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/housefly.tres");
    protected override Stats Stats => new(1, 1, 1, 1, 50, 10, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["HFAttack", "HFDoNothing", "HFAnnoy"];

    public override BattleCommand ProcessAI()
    {
        switch (CurrentEmotion.Group?.Id)
        {
            case "happy":
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 31)
                    goto doNothing;
                goto annoy;
            case "sad":
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 56)
                    goto doNothing;
                goto annoy;
            case "angry":
                if (Roll() < 61)
                    goto attack;
                if (Roll() < 31)
                    goto doNothing;
                goto annoy;
            default:
                if (Roll() < 46)
                    goto attack;
                if (Roll() < 41)
                    goto doNothing;
                goto annoy;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["HFAttack"]);
    doNothing:
        return new BattleCommand(this, this, Skills["HFDoNothing"]);
    annoy:
        return new BattleCommand(this, SelectTarget(), Skills["HFAnnoy"]);
    }
}
