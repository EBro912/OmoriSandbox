using Godot;

using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;
internal sealed class SourdoughAlt : Enemy
{
    public override string Name => "SOURDOUGH";
    public override Vector2 InfoBoxOffset => new(0, -175);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/sourdough.tres");
    protected override Stats Stats => new(2000, 2000, 88, 65, 95, 10, 95);
    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id == "neutral" || emotion.Id == "sad" || emotion.Id == "happy" || emotion.Id == "angry";
    }
    protected override string[] EquippedSkills => ["SDAttack", "SDDoNothing", "SDBadWord"];
  
    public override BattleCommand ProcessAI()
    {
        switch (CurrentEmotion.Id)
        {
            case "happy":
                if (Roll() < 36)
                    goto attack;
                if (Roll() < 31)
                    goto nothing;
                goto badword;
            case "sad":
                if (Roll() < 21)
                    goto attack;
                if (Roll() < 36)
                    goto nothing;
                goto badword;
            case "angry":
                if (Roll() < 61)
                    goto attack;
                if (Roll() < 26)
                    goto nothing;
                goto badword;
            default:
                if (Roll() < 36)
                    goto attack;
                if (Roll() < 26)
                    goto nothing;
                goto badword;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["SDAttack"]);
    nothing:
        return new BattleCommand(this, this, Skills["SDDoNothing"]);
    badword:
        return new BattleCommand(this, SelectTarget(), Skills["SDBadWord"]);
    }
}