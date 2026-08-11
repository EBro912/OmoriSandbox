using Godot;

using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;
internal sealed class TheEarth : Enemy
{
    public override string Name => "THE EARTH";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/earth.tres");
    protected override Stats Stats => new(425, 210, 20, 15, 15, 10, 95);

    protected override string[] EquippedSkills => ["TEAttack", "TEDoNothing", "TECruel", "TEProtect"];

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "sad" or "happy" or "angry";
    }

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["TEAttack"]);
        
        if (IsBelowHP(0.2f))
            return new BattleCommand(this, SelectAllTargets(), Skills["TEProtect"]);

        switch (CurrentEmotion.Id)
        {
            case "sad":
                if (Roll() < 51)
                    goto attack;
                if (Roll() < 31)
                    goto nothing;
                goto cruel;
            case "happy":
                if (Roll() < 46)
                    goto attack;
                if (Roll() < 61)
                    goto nothing;
                goto cruel;
            case "angry":
                if (Roll() < 76)
                    goto attack;
                if (Roll() < 36)
                    goto nothing;
                goto cruel;
            default:
                if (Roll() < 61)
                    goto attack;
                if (Roll() < 36)
                    goto nothing;
                goto cruel;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["TEAttack"]);
    nothing:
        return new BattleCommand(this, this, Skills["TEDoNothing"]);
    cruel:
        return new BattleCommand(this, SelectTarget(), Skills["TECruel"]);
    }
}