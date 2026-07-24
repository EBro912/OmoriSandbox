using Godot;

using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;
internal sealed class Slice : Enemy
{
    public override string Name => "SLICE";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/slice.tres");
    protected override Stats Stats => new(344, 151, 35, 54, 40, 10, 95);
    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id == "neutral" || emotion.Id == "sad" || emotion.Id == "happy" || emotion.Id == "angry";
    }
    protected override string[] EquippedSkills => ["SLAttack", "SLDoNothing", "SLRile"];
  
    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["SLAttack"]);
        
        switch (CurrentEmotion.Id)
        {
            case "happy":
                if (Roll() < 46)
                    goto attack;
                if (Roll() < 31)
                    goto nothing;
                goto rile;
            case "sad":
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 61)
                    goto nothing;
                goto rile;
            case "angry":
                if (Roll() < 86)
                    goto attack;
                goto nothing;
            default:
                if (Roll() < 66)
                    goto attack;
                if (Roll() < 41)
                    goto nothing;
                goto rile;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["SLAttack"]);
    nothing:
        return new BattleCommand(this, this, Skills["SLDoNothing"]);
    rile:
        return new BattleCommand(this, SelectAllEnemies(), Skills["SLRile"]);
    }
}