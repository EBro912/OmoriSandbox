using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class SnowAngel : Enemy
{
    public override string Name => "SNOW ANGEL";
    public override Vector2 InfoBoxOffset => new(0, -260);
    public override Vector2 DamageNumberOffset => new(0, -160);
    public override bool InfoBoxCursorAboveBox => true;
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/snow_angel.tres");
    protected override Stats Stats => new(1000, 225, 86, 22, 80, 25, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["SNAUpliftingHymn", "SNAAttack", "SNAPierceHeart"];

    public override BattleCommand ProcessAI()
    {
        if (HasMultiTargetObserve())
            return new BattleCommand(this, SelectAllTargets(), Skills["SNAUpliftingHymn"]);

        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["SNAAttack"]);

        if (HasTargetWithEmotion("happy", out PartyMember happy))
            return new BattleCommand(this, happy, Skills["SNAPierceHeart"]);

        switch (CurrentEmotion.Group?.Id)
        {
            case "happy":
                if (Roll() < 31)
                    goto attack;
                goto upliftingHymn;
            case "angry":
                if (Roll() < 71)
                    goto attack;
                goto upliftingHymn;
            default:
                if (Roll() < 41)
                    goto attack;
                goto upliftingHymn;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["SNAAttack"]);
    upliftingHymn:
        return new BattleCommand(this, SelectAllTargets(), Skills["SNAUpliftingHymn"]);
    }
}
