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
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/abbi.tres");
    protected override Stats Stats => new(8000, 2500, 63, 76, 90, 20, 95);
    protected override string[] EquippedSkills => ["AbbiAttack", "AbbiAttackOrder", "AbbiSummon"];
    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "sad" or "happy" or "angry";
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
                if (!GodotObject.IsInstanceValid(Tentacles[i]) || Tentacles[i].Actor.IsToast)
                {
                    if (!PreRolling)
                        Tentacles[i] = BattleManager.Instance.SummonEnemy("Tentacle", CenterPoint + new Vector2(Offsets[i], -80),
                            layer: Layer + 1);
                    return new BattleCommand(this, this, Skills["AbbiSummon"]);
                }
            }
        }

        if (Roll() < 36 && SelectAllEnemies().Count > 1)
            return new BattleCommand(this, SelectAllEnemies(), Skills["AbbiAttackOrder"]);
        return new BattleCommand(this, SelectTarget(), Skills["AbbiAttack"]);
    }

    private bool HasSpoken = false;
    public override async Task ProcessBattleConditions()
    {
        if (CurrentHP <= 0) return;

        if (IsBelowHP(0.5f) && !HasSpoken)
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