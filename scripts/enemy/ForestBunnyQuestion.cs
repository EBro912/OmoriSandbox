using Godot;
using OmoriSandbox.Battle;

namespace OmoriSandbox.Actors;
internal sealed class ForestBunnyQuestion : Enemy
{
    public override string Name => "FOREST BUNNY?";

    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/forest_bunny_alt.tres");

    protected override Stats Stats => new(110, 55, 13, 6, 11, 10, 95);

    public override bool IsStateValid(string state)
    {
        return state == "neutral" || state == "sad" || state == "happy" || state == "angry" || state == "hurt" || state == "toast";
    }

    protected override string[] EquippedSkills => ["FBQAttack", "FBQDoNothing", "FBQBeCute", "FBQSadEyes"];

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["FBQAttack"]);
        
        Actor target = SelectTarget();
        switch (CurrentState)
        {
            case "happy":
                if (Roll() < 46)
                    goto attack;
                if (Roll() < 31)
                    goto nothing;
                goto cute;
            case "sad":
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 31)
                    goto nothing;
                goto sad;
            case "angry":
                if (Roll() < 71)
                    goto attack;
                if (Roll() < 21)
                    goto nothing;
                goto cute;
            default:
                if (Roll() < 61)
                    goto attack;
                if (Roll() < 41)
                    goto nothing;
                goto cute;
        }
    attack:
        return new BattleCommand(this, target, Skills["FBQAttack"]);
    nothing:
        return new BattleCommand(this, this, Skills["FBQDoNothing"]);
    cute:
        return new BattleCommand(this, target, Skills["FBQBeCute"]);
    sad:
        return new BattleCommand(this, target, Skills["FBQSadEyes"]);
    }
}