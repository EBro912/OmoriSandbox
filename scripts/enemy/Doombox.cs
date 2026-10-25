using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class Doombox : Enemy
{
    public override string Name => "DOOMBOX";
    public override Vector2 InfoBoxOffset => new(0, -200);
    public override Vector2 DamageNumberOffset => new(0, 30);
    protected internal override bool ObserveHasMulti => true;
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/doombox.tres");
    protected override Stats Stats => new(135, 65, 12, 10, 6, 10, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["DMBBlastMusic", "DMBAttack", "DMBDoNothing"];

    public override BattleCommand ProcessAI()
    {
        if (HasMultiTargetObserve())
            return new BattleCommand(this, SelectAllTargets(), Skills["DMBBlastMusic"]);

        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["DMBAttack"]);

        switch (CurrentEmotion.Group?.Id)
        {
            case "happy":
                if (Roll() < 46)
                    goto attack;
                if (Roll() < 41)
                    goto doNothing;
                goto blastMusic;
            case "sad":
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 71)
                    goto doNothing;
                goto blastMusic;
            case "angry":
                if (Roll() < 61)
                    goto attack;
                if (Roll() < 11)
                    goto doNothing;
                goto blastMusic;
            default:
                if (Roll() < 56)
                    goto attack;
                if (Roll() < 36)
                    goto doNothing;
                goto blastMusic;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["DMBAttack"]);
    doNothing:
        return new BattleCommand(this, this, Skills["DMBDoNothing"]);
    blastMusic:
        return new BattleCommand(this, SelectAllTargets(), Skills["DMBBlastMusic"]);
    }

    public override Task ProcessEndOfTurn()
    {
        if (BattleManager.Instance.TurnCount < 1)
            return Task.CompletedTask;
        if (!IsToast && CurrentEmotion.Id != "angry")
            BattleManager.Instance.ForceCommand(this, this, Database.Skills["DMBSelfAngry"]);
        return Task.CompletedTask;
    }
}
