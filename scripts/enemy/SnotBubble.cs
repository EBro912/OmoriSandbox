using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class SnotBubble : Enemy
{
    public override string Name => "SNOT BUBBLE";
    public override Vector2 InfoBoxOffset => new(0, -175);
    protected internal override bool ObserveHasMulti => true;
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/snot_bubble.tres");
    protected override Stats Stats => new(875, 250, 20, 1, 1, 10, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["SNTPop", "SNTPopOnDeath", "SNTInflate"];

    private bool PopPageRan;

    public override Task OnStartOfBattle()
    {
        AddImmortal();
        return Task.CompletedTask;
    }

    public override Task ProcessBattleConditions()
    {
        if (PopPageRan || CurrentHP != 0)
            return Task.CompletedTask;
        PopPageRan = true;
        if (BattleManager.Instance.GetCurrentCommand()?.Actor == this)
            return Task.CompletedTask;
        if (!IsImmortal)
            AddImmortal();
        BattleManager.Instance.ForceCommand(this, SelectAllTargets(), Skills["SNTPopOnDeath"]);
        RemoveImmortal(afterForcedCommands: true);
        return Task.CompletedTask;
    }

    public override BattleCommand ProcessAI()
    {
        if (HasMultiTargetObserve())
            return new BattleCommand(this, SelectAllTargets(), Skills["SNTPop"]);

        if (IsBelowHP(0.1f))
            goto popOnDeath;
        if (TrueTurnCount == 0)
            goto inflate;
        if (TrueTurnCount == 1)
            goto inflate;
        if (TrueTurnCount == 2)
            goto inflate;
        if (TrueTurnCount == 3)
            goto pop;
        return null;
    popOnDeath:
        return new BattleCommand(this, SelectAllTargets(), Skills["SNTPopOnDeath"]);
    inflate:
        return new BattleCommand(this, this, Skills["SNTInflate"]);
    pop:
        return new BattleCommand(this, SelectAllTargets(), Skills["SNTPop"]);
    }
}
