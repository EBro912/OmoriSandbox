using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class GatorGuyHero : Enemy
{
    public override string Name => "GATOR GUY";
    public override Vector2 InfoBoxOffset => new(0, -250);
    public override bool InfoBoxCursorAboveBox => true;
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/gator_guy.tres");
    protected override Stats Stats => new(6000, 3000, 80, 65, 70, 10, 95);
    protected override string[] EquippedSkills => ["GGAttack", "GGDoNothing", "GGRoughUp"];
    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id == "neutral" || emotion.Id == "happy" || emotion.Id == "sad" || emotion.Id == "angry";
    }
    public override BattleCommand ProcessAI()
    {
        switch (CurrentEmotion.Id)
        {
            case "happy":
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 31)
                    goto nothing;
                goto rough;
            case "sad":
                if (Roll() < 26)
                    goto attack;
                if (Roll() < 41)
                    goto nothing;
                goto rough;
            case "angry":
                if (Roll() < 46)
                    goto attack;
                if (Roll() < 26)
                    goto nothing;
                goto rough;
            default:
                if (Roll() < 36)
                    goto attack;
                if (Roll() < 26)
                    goto nothing;
                goto rough;
        }
        attack:
        return new BattleCommand(this, SelectTarget(), Skills["GGAttack"]);
        nothing:
        return new BattleCommand(this, this, Skills["GGDoNothing"]);
        rough:
        return new BattleCommand(this, SelectTarget(), Skills["GGRoughUp"]);
    }

    public override Stats GetBaseStats()
    {
        if (CurrentEmotion.Id is "sad" or "angry")
            return new Stats(6000, 3000, 80, 65, 80, 10, 95) + AdjustedStats;
        return new Stats(6000, 3000, 80, 65, 70, 10, 95) + AdjustedStats;
    }
}