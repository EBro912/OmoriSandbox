using System.Linq;
using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class Abbi : Enemy
{
    public override string Name => "ABBI";
    public override Vector2 InfoBoxOffset => new(0, -325);
    public override Vector2 DamageNumberOffset => new(0, -150);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/abbi.tres");
    protected override Stats Stats => new(8000, 2500, 63, 76, 90, 20, 95);
    protected override string[] EquippedSkills => ["AbbiAttack", "AbbiAttackOrder", "AbbiSummon"];
    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "sad" or "happy" or "angry" or "afraid";
    }

    private readonly Enemy[] Tentacles = new Enemy[4];

    public override Task OnStartOfBattle()
    {
        for (int i = 0; i < 4; i++)
            ReviveTentacle();
        foreach (Enemy enemy in BattleManager.Instance.GetAllEnemies())
        {
            enemy.RemoveStatModifier("CallForFriendDelay");
            enemy.ClearResult();
        }
        return Task.CompletedTask;
    }

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["AbbiAttack"]);
        
        if (Roll() < 71 && Tentacles.Any(x => x?.Component == null || x.IsToast))
            return new BattleCommand(this, this, Skills["AbbiSummon"]);

        if (Roll() < 36 && SelectAllEnemies().Count > 1)
            return new BattleCommand(this, SelectAllEnemies(), Skills["AbbiAttackOrder"]);
        return new BattleCommand(this, SelectTarget(), Skills["AbbiAttack"]);
    }

    internal void ReviveTentacle()
    {
        // VANILLA BUG: the delay state is added to the slot's first, already-dead tentacle, so a revived tentacle can act right away
        Enemy tentacle = ReinforcementSlots.Summon(this, ReinforcementSlots.Abbi);
        if (tentacle != null)
            Tentacles[ReinforcementSlots.SlotOf(tentacle, ReinforcementSlots.Abbi) - 2] = tentacle;
    }

    private bool HasSpoken;
    public override async Task ProcessBattleConditions()
    {
        if (IsBelowHP(0.5f) && !HasSpoken)
        {
            DialogueManager.Instance.QueueMessage(this, "[shake rate=20]Ngh...", font: DialogueManager.FontType.Jagged);
            await DialogueManager.Instance.WaitForDialogue();
            HasSpoken = true;
        }
    }

    public override Task OnDefeat()
    {
        foreach (Enemy e in Tentacles)
            e.CurrentHP = 0;
        return Task.CompletedTask;
    }

    public override async Task OnEndOfBattle(BattleOutcome outcome)
    {
        if (outcome == BattleOutcome.Defeat)
        {
            DialogueManager.Instance.QueueMessage(this, "[shake rate=20]Goodbye...", font: DialogueManager.FontType.Jagged);
            await DialogueManager.Instance.WaitForDialogue();
        }
    }
}