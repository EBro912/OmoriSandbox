using Godot;

using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;
internal sealed class Sourdough : Enemy
{
    public override string Name => "SOURDOUGH";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/sourdough.tres");
    protected override Stats Stats => new(363, 93, 55, 33, 49, 10, 95);
    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id == "neutral" || emotion.Id == "sad" || emotion.Id == "happy" || emotion.Id == "angry";
    }
    protected override string[] EquippedSkills => ["SDAttack", "SDDoNothing", "SDBadWord"];
  
    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["SDBadWord"]);
        
        switch (CurrentEmotion.Id)
        {
            case "happy":
                if (Roll() < 46)
                    goto attack;
                if (Roll() < 31)
                    goto nothing;
                goto badword;
            case "sad":
                if (Roll() < 21)
                    goto attack;
                if (Roll() < 56)
                    goto nothing;
                goto badword;
            case "angry":
                if (Roll() < 76)
                    goto attack;
                if (Roll() < 26)
                    goto nothing;
                goto badword;
            default:
                if (Roll() < 66)
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