using System.Linq;
using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;

namespace OmoriSandbox.Actors;

internal sealed class Abbi : Enemy
{
    public override string Name => "ABBI";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/abbi.tres");
    protected override Stats Stats => new(8000, 2500, 63, 76, 90, 20, 95);
    protected override string[] EquippedSkills => ["AbbiAttack", "AbbiAttackOrder", "AbbiSummon"];
    public override bool IsStateValid(string state)
    {
        return state == "neutral" || state == "sad" || state == "happy" || state == "angry" || state == "hurt" || state == "toast";
    }

    private readonly EnemyComponent[] Tentacles = new EnemyComponent[4];
    private readonly int[] Offsets = [-200, -80, 40, 180];

    public override Task OnStartOfBattle()
    {
        for (int i = 0; i < 4; i++)
        {
            Tentacles[i] = BattleManager.Instance.SummonEnemy("Tentacle", CenterPoint + new Vector2(Offsets[i], -80),
                layer: Layer + 1);
        }
        return Task.CompletedTask;
    }

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["AbbiAttack"]);
        
        if (Roll() < 71)
        {
            for (int i = 0; i < 4; i++)
            {
                if (!GodotObject.IsInstanceValid(Tentacles[i]) || Tentacles[i].Actor.CurrentState == "toast")
                {
                    Tentacles[i] = BattleManager.Instance.SummonEnemy("Tentacle", CenterPoint + new Vector2(Offsets[i], -80),
                        layer: Layer + 1);
                    return new BattleCommand(this, this, Skills["AbbiSummon"]);
                }
            }
        }

        if (Roll() < 36)
            return new BattleCommand(this, SelectAllEnemies(), Skills["AbbiAttackOrder"]);
        return new BattleCommand(this, SelectTarget(), Skills["AbbiAttack"]);
    }

    private bool HasSpoken = false;
    public override async Task ProcessBattleConditions()
    {
        if (CurrentHP <= 0) return;

        if (CurrentHP < 4000 && !HasSpoken)
        {
            DialogueManager.Instance.QueueMessage(this, "[shake rate=20]Ngh...", font: DialogueManager.FontType.Jagged);
            await DialogueManager.Instance.WaitForDialogue();
            HasSpoken = true;
        }
    }

    public override Task OnDefeat()
    {
        foreach (EnemyComponent e in Tentacles)
            e.Actor.CurrentHP = 0;
        return Task.CompletedTask;
    }

    public override async Task OnEndOfBattle(bool victory)
    {
        if (!victory)
        {
            DialogueManager.Instance.QueueMessage(this, "[shake rate=20]Goodbye...", font: DialogueManager.FontType.Jagged);
            await DialogueManager.Instance.WaitForDialogue();
        }
    }
}