using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;
using OmoriSandbox.Battle.Modifier;

namespace OmoriSandbox.Actors;

internal sealed class Charlene : Enemy
{
    // base game calls her CHARLIE
    public override string Name => "CHARLIE";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/charlene.tres");
    protected override Stats Stats => new(300, 100, 10, 40, 10, 10, 95);
    public override Vector2 InfoBoxOffset => new(0, -375);
    protected override string[] EquippedSkills => ["CHAttack", "CHDoNothing"];

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "ecstatic" or "manic" or "sad" or "depressed" or "miserable" or "angry" or "enraged" or "furious" or "afraid";
    }

    protected override PartyMember SelectTarget()
    {
        if (HasStatModifier("Charm"))
            return (StatModifiers["Charm"] as CharmStatModifier).CharmedBy;
        return SelectTargetWithHighest(StatType.SPD);
    }

    public override Task OnStartOfBattle()
    {
        AddStatModifier("ResistPhysical");
        return Task.CompletedTask;
    }

    public override async Task ProcessBattleConditions()
    {
        if (SelectAllEnemies().Count == 1)
        {
            DialogueManager.Instance.QueueMessage("CHARLIE", CenterPoint, "...");
            await DialogueManager.Instance.WaitForDialogue();
            BattleLogManager.Instance.QueueMessage(this, "[actor] stops fighting.");
            CurrentHP = 0;
        }
    }

    public override BattleCommand ProcessAI()
    {
        int chance = CurrentEmotion.Group?.Id switch
        {
            "sad" => 6,
            "angry" => 36,
            _ => 16,
        };
        if (Roll() < chance)
            return new BattleCommand(this, SelectTarget(), Skills["CHAttack"]);
        return new BattleCommand(this, this, Skills["CHDoNothing"]);
    }
}