using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class SharkPlane : Enemy
{
    public override string Name => "SHARK PLANE";
    public override Vector2 InfoBoxOffset => new(0, -175);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/shark_plane.tres");
    protected override Stats Stats => new(300, 150, 16, 7, 20, 10, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["SHPChomp", "SHPAttack", "SHPDoNothing", "SHPOverclockEngines"];

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["SHPChomp"]);

        switch (CurrentEmotion.Group?.Id)
        {
            case "happy":
                if (Roll() < 46)
                    goto attack;
                if (Roll() < 26)
                    goto doNothing;
                if (Roll() < 51)
                    goto overclockEngines;
                goto chomp;
            case "sad":
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 51)
                    goto doNothing;
                if (Roll() < 26)
                    goto overclockEngines;
                goto chomp;
            case "angry":
                if (Roll() < 61)
                    goto attack;
                if (Roll() < 21)
                    goto doNothing;
                goto chomp;
            default:
                if (Roll() < 66)
                    goto attack;
                if (Roll() < 31)
                    goto doNothing;
                if (Roll() < 41)
                    goto overclockEngines;
                goto chomp;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["SHPAttack"]);
    doNothing:
        return new BattleCommand(this, this, Skills["SHPDoNothing"]);
    overclockEngines:
        return new BattleCommand(this, this, Skills["SHPOverclockEngines"]);
    chomp:
        return new BattleCommand(this, SelectTarget(), Skills["SHPChomp"]);
    }
}
