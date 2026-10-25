using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class Watermimic : Enemy
{
    public override string Name => "WATERMIMIC";
    public override Vector2 InfoBoxOffset => CurrentEmotion.Group == null ? new(0, -240) : new(0, -250);
    protected internal override bool ObserveHasMulti => true;
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/watermimic.tres");
    protected override Stats Stats => new(600, 250, 10, 25, 150, 10, 200);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["WMMDynamite", "WMMRubberBand", "WMMJacks", "WMMTomato", "WMMGrapes", "WMMFrenchFries", "WMMConfetti", "WMMRainCloud", "WMMAirHorn"];

    public override BattleCommand ProcessAI()
    {
        if (HasMultiTargetObserve())
            return new BattleCommand(this, SelectAllTargets(), Skills["WMMDynamite"]);

        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["WMMRubberBand"]);

        switch (CurrentEmotion.Group?.Id)
        {
            case "happy":
                if (Roll() < 21)
                    goto rubberBand;
                if (Roll() < 21)
                    goto jacks;
                if (Roll() < 21)
                    goto dynamite;
                if (Roll() < 21)
                    goto tomato;
                if (Roll() < 21)
                    goto grapes;
                if (Roll() < 21)
                    goto frenchFries;
                if (Roll() < 21)
                    goto rainCloud;
                goto airHorn;
            case "sad":
                if (Roll() < 21)
                    goto rubberBand;
                if (Roll() < 21)
                    goto jacks;
                if (Roll() < 21)
                    goto dynamite;
                if (Roll() < 21)
                    goto tomato;
                if (Roll() < 21)
                    goto grapes;
                if (Roll() < 21)
                    goto frenchFries;
                if (Roll() < 21)
                    goto confetti;
                goto airHorn;
            case "angry":
                if (Roll() < 21)
                    goto rubberBand;
                if (Roll() < 21)
                    goto jacks;
                if (Roll() < 21)
                    goto dynamite;
                if (Roll() < 21)
                    goto tomato;
                if (Roll() < 21)
                    goto grapes;
                if (Roll() < 21)
                    goto frenchFries;
                if (Roll() < 21)
                    goto confetti;
                goto rainCloud;
            default:
                if (Roll() < 21)
                    goto rubberBand;
                if (Roll() < 21)
                    goto jacks;
                if (Roll() < 21)
                    goto dynamite;
                if (Roll() < 21)
                    goto tomato;
                if (Roll() < 21)
                    goto grapes;
                if (Roll() < 21)
                    goto frenchFries;
                if (Roll() < 21)
                    goto confetti;
                if (Roll() < 21)
                    goto rainCloud;
                goto airHorn;
        }
    rubberBand:
        return new BattleCommand(this, SelectTarget(), Skills["WMMRubberBand"]);
    jacks:
        return new BattleCommand(this, SelectAllTargets(), Skills["WMMJacks"]);
    dynamite:
        return new BattleCommand(this, SelectAllTargets(), Skills["WMMDynamite"]);
    tomato:
        return new BattleCommand(this, SelectTarget(), Skills["WMMTomato"]);
    grapes:
        return new BattleCommand(this, SelectTarget(), Skills["WMMGrapes"]);
    frenchFries:
        return new BattleCommand(this, SelectAllTargets(), Skills["WMMFrenchFries"]);
    confetti:
        return new BattleCommand(this, SelectAllTargets(), Skills["WMMConfetti"]);
    rainCloud:
        return new BattleCommand(this, SelectAllTargets(), Skills["WMMRainCloud"]);
    airHorn:
        return new BattleCommand(this, SelectAllTargets(), Skills["WMMAirHorn"]);
    }
}
