using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class GhostBunny : Enemy
{
    public override string Name => "GHOST BUNNY";
    public override Vector2 InfoBoxOffset => new(0, -175);
    public override Vector2 DamageNumberOffset => new(0, 25);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/ghost_bunny.tres");
    protected override Stats Stats => new(150, 999, 20, 10, 100, 10, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["GHBAttack", "GHBDoNothing"];

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["GHBAttack"]);

        switch (CurrentEmotion.Group?.Id)
        {
            case "happy":
                if (Roll() < 51)
                    goto attack;
                goto doNothing;
            case "sad":
                if (Roll() < 46)
                    goto attack;
                goto doNothing;
            case "angry":
                if (Roll() < 61)
                    goto attack;
                if (Roll() < 41)
                    goto doNothing;
                return null;
            default:
                if (Roll() < 56)
                    goto attack;
                goto doNothing;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["GHBAttack"]);
    doNothing:
        return new BattleCommand(this, this, Skills["GHBDoNothing"]);
    }
}
