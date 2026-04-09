using System;
using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

using OmoriSandbox.Battle;

namespace OmoriSandbox.Actors;
internal sealed class UnbreadTwins : Enemy
{
    public override string Name => "UNBREAD TWINS";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/unbread_twins.tres");
    protected override Stats Stats => new(7500, 5000, 65, 1, 57, 10, 95);
    protected override string[] EquippedSkills => ["UBTAttack", "UBTDoNothing", "UBTCheerUp", "UBTCook", "UBTBakeBread"];

    private static readonly string[] SpawnPool = ["BunBunny", "Creepypasta", "Slice", "Sourdough", "Sesame", "LivingBread"];
    private bool EmotionLocked = false;
    private int Stage = 0;

    private List<EnemyComponent> SpawnedBread = [];

    public override bool IsStateValid(string state)
    {
        if (state == "toast")
            return true;

        if (EmotionLocked)
            return false;

        return state is "neutral" or "sad" or "happy" or "angry" or "hurt";
    }

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["UBTAttack"]);
        
        // when Unbread Twins are emotion locked to sad, their AI uses depressed to prevent trying to cleanse sad
        string state = CurrentState == "sad" && EmotionLocked ? "depressed" : CurrentState;
        switch (state) {
            case "miserable":
                if (Roll() < 51)
                    goto attack;
                if (Roll() < 36)
                    goto cook;
                if (SpawnedBread.Count < 2)
                    goto bake;
                goto nothing;
            case "depressed":
                if (Roll() < 41)
                    goto attack;
                if (Roll() < 36)
                    goto cook;
                if (SpawnedBread.Count < 2)
                    goto bake;
                goto nothing;
            case "sad":
                if (Roll() < 51)
                    goto attack;
                goto cheerup;
            default:
                if (Roll() < 51)
                    goto attack;
                if (SpawnedBread.Count < 2)
                    goto bake;
                goto nothing;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["UBTAttack"]);
    nothing:
        return new BattleCommand(this, this, Skills["UBTDoNothing"]);
    bake:
        return new BattleCommand(this, this, Skills["UBTBakeBread"]);
    cheerup:
        return new BattleCommand(this, this, Skills["UBTCheerUp"]);
    cook:
        return new BattleCommand(this, SelectEnemy(), Skills["UBTCook"]);
    }

    public override async Task ProcessBattleConditions()
    {
        SpawnedBread.RemoveAll(x => x == null || x.Actor.CurrentHP <= 0);

        if (CurrentHP <= 0)
            return;

        if (Stage > 3)
            return;

        if (CurrentHP < 6000 && Stage == 0)
        {
            DialogueManager.Instance.QueueMessage("DOUGHIE", CenterPoint, @"[wave freq=10][br]Fresh bread...\! fresh bread...\! Every day, it's fresh bread...");
            DialogueManager.Instance.QueueMessage("BISCUIT", CenterPoint, "[wave freq=10][br]Ohooooooooo...");
            await DialogueManager.Instance.WaitForDialogue();
            ForceState("UnbreadTwinsSad", "sad");
            DialogueManager.Instance.QueueMessage("UNBREAD TWINS became SAD...");
            DialogueManager.Instance.QueueMessage("UNBREAD TWINS can no longer become HAPPY or ANGRY!");
            await DialogueManager.Instance.WaitForDialogue();
            EmotionLocked = true;
            Stage = 1;
        }

        if (CurrentHP < 4875 && Stage <= 1)
        {
            DialogueManager.Instance.QueueMessage("DOUGHIE", CenterPoint, @"We're doomed to bake bread for all enternity...\! aren't we, BISCUIT?");
            DialogueManager.Instance.QueueMessage("BISCUIT", CenterPoint, "[wave freq=10]Ohooo...");
            await DialogueManager.Instance.WaitForDialogue();
            Stage = 2;
        }

        if (CurrentHP < 3750 && Stage <= 2)
        {
            DialogueManager.Instance.QueueMessage("DOUGHIE", CenterPoint, "We're running out of supplies! What do we do, BISCUIT!?");
            DialogueManager.Instance.QueueMessage("BISCUIT", CenterPoint, "[wave freq=10]Ohooooooo!");
            await DialogueManager.Instance.WaitForDialogue();
            ForceState("UnbreadTwinsDepressed", "depressed");
            DialogueManager.Instance.QueueMessage("UNBREAD TWINS became DEPRESSED...");
            await DialogueManager.Instance.WaitForDialogue();
            Stage = 3;
        } 
        
        if (CurrentHP < 1875 && Stage <= 3)
        {
            DialogueManager.Instance.QueueMessage("DOUGHIE", CenterPoint, @"We're running low on everything!\! We have almost nothing left...");
            DialogueManager.Instance.QueueMessage("BISCUIT", CenterPoint, "[wave freq=10]Ohooo...");
            await DialogueManager.Instance.WaitForDialogue();
            ForceState("UnbreadTwinsMiserable", "miserable");
            DialogueManager.Instance.QueueMessage("UNBREAD TWINS became MISERABLE...");
            await DialogueManager.Instance.WaitForDialogue();
            Stage = 4;
        }
    }

    public override async Task OnDefeat()
    {
        DialogueManager.Instance.QueueMessage("DOUGHIE", CenterPoint, @"Our resources have been depleted...\! What will we do without ingredients?");
        DialogueManager.Instance.QueueMessage("BISCUIT", CenterPoint, "[wave freq=10]Ohooooo...");
        await DialogueManager.Instance.WaitForDialogue();
    }

    public override async Task OnEndOfBattle(bool victory)
    {
        if (!victory)
        {
            DialogueManager.Instance.QueueMessage("DOUGHIE", CenterPoint, @"BISCUIT!\! It's a miracle!\![br]We've been saved by the gods!");
            DialogueManager.Instance.QueueMessage("BISCUIT", CenterPoint, "[wave freq=10]Ohooooo!");
            DialogueManager.Instance.QueueMessage("DOUGHIE", CenterPoint, @"Now I guess it's back to making... [wave freq=10]fresh bread...\! fresh bread...\! fresh bread...");
            DialogueManager.Instance.QueueMessage("BISCUIT", CenterPoint, "[wave freq=10]Ohoo...");
            await DialogueManager.Instance.WaitForDialogue();
        }
    }

    public void SpawnBread()
    {
        EnemyComponent enemy;
        if (SpawnedBread.Count == 0)
            enemy = BattleManager.Instance.SummonEnemy(SpawnPool[GameManager.Instance.Random.RandiRange(0, SpawnPool.Length - 1)], new Vector2(CenterPoint.X - 270, CenterPoint.Y), layer: Math.Max(0, Layer - 1));
        else if (SpawnedBread.Count == 1)
            enemy = BattleManager.Instance.SummonEnemy(SpawnPool[GameManager.Instance.Random.RandiRange(0, SpawnPool.Length - 1)], new Vector2(CenterPoint.X + 200, CenterPoint.Y), layer: Math.Max(0, Layer - 1));
        else
        {
            GD.PushWarning("Tried to summon more than 2 breads!");
            return;
        }
        SpawnedBread.Add(enemy);
    }
}