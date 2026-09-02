using System;
using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;
internal sealed class UnbreadTwinsAlt : Enemy
{
    public override string Name => "UNBREAD TWINS";
    public override Vector2 InfoBoxOffset => new(-30, -175);
    public override bool InfoBoxCursorAboveBox => true;
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/unbread_twins.tres");
    protected override Stats Stats => new(10000, 5000, 90, 1, 80, 10, 95);
    protected override string[] EquippedSkills => ["UBTAttack", "UBTDoNothing", "UBTCheerUp", "UBTCook", "UBTBakeBread"];

    private static readonly string[] SpawnPool = ["Slice (Epilogue)", "Sourdough (Epilogue)", "Sesame (Epilogue)"];
    private int Stage = 0;

    private readonly EnemyComponent[] Breads = new EnemyComponent[2];
    // Unbread Twins only ever bake bread twice
    private int Bakes = 0;
    private readonly int[] Offsets = [-270, 200];

    public override bool IsEmotionValid(Emotion emotion)
    {
        if (IsEmotionLocked)
            return false;

        return emotion.Id is "neutral" or "sad" or "happy" or "angry";
    }

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["UBTAttack"]);
        
        // when Unbread Twins are emotion locked to sad, their AI uses depressed to prevent trying to cleanse sad
        string state = CurrentEmotion.Id == "sad" && IsEmotionLocked ? "depressed" : CurrentEmotion.Id;
        switch (state) {
            case "miserable":
                if (Roll() < 46)
                    goto attack;
                if (Roll() < 36)
                    goto cook;
                if (Bakes < 2 && Breads.Any(x => !GodotObject.IsInstanceValid(x) || x.Actor.IsToast))
                    goto bake;
                goto nothing;
            case "depressed":
                if (Roll() < 46)
                    goto attack;
                if (Roll() < 36)
                    goto cook;
                if (Bakes < 2 && Breads.Any(x => !GodotObject.IsInstanceValid(x) || x.Actor.IsToast))
                    goto bake;
                goto nothing;
            case "sad":
                if (Roll() < 51)
                    goto attack;
                goto cheerup;
            default:
                if (Roll() < 51)
                    goto attack;
                if (Bakes < 2 && Breads.Any(x => !GodotObject.IsInstanceValid(x) || x.Actor.IsToast))
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
        if (CurrentHP <= 0)
            return;

        if (Stage > 3)
            return;

        if (IsBelowHP(0.8f) && Stage == 0)
        {
            DialogueManager.Instance.QueueMessage("DOUGHIE", CenterPoint, @"[wave freq=10][br]Fresh bread...\! Fresh bread...\! Every day, it's fresh bread...");
            DialogueManager.Instance.QueueMessage("BISCUIT", CenterPoint, "[wave freq=10][br]Ohooooooooo...");
            await DialogueManager.Instance.WaitForDialogue();
            SetEmotionForced("sad");
            DialogueManager.Instance.QueueMessage("UNBREAD TWINS became SAD...");
            DialogueManager.Instance.QueueMessage("UNBREAD TWINS can no longer become HAPPY or ANGRY!");
            await DialogueManager.Instance.WaitForDialogue();
            LockEmotion("sad");
            Stage = 1;
        }

        if (IsBelowHP(0.65f) && Stage <= 1)
        {
            DialogueManager.Instance.QueueMessage("DOUGHIE", CenterPoint, @"We're doomed to bake bread for all eternity...\! aren't we, BISCUIT?");
            DialogueManager.Instance.QueueMessage("BISCUIT", CenterPoint, "[wave freq=10]Ohooo...");
            await DialogueManager.Instance.WaitForDialogue();
            Stage = 2;
        }

        if (IsBelowHP(0.5f) && Stage <= 2)
        {
            DialogueManager.Instance.QueueMessage("DOUGHIE", CenterPoint, "We're running out of supplies! What do we do, BISCUIT!?");
            DialogueManager.Instance.QueueMessage("BISCUIT", CenterPoint, "[wave freq=10]Ohooooooo!");
            await DialogueManager.Instance.WaitForDialogue();
            SetEmotionForced("depressed");
            DialogueManager.Instance.QueueMessage("UNBREAD TWINS became DEPRESSED...");
            await DialogueManager.Instance.WaitForDialogue();
            Stage = 3;
        } 
        
        if (IsBelowHP(0.25f) && Stage <= 3)
        {
            DialogueManager.Instance.QueueMessage("DOUGHIE", CenterPoint, @"We're running low on everything!\! We have almost nothing left...");
            DialogueManager.Instance.QueueMessage("BISCUIT", CenterPoint, "[wave freq=10]Ohooo...");
            await DialogueManager.Instance.WaitForDialogue();
            SetEmotionForced("miserable");
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
            DialogueManager.Instance.QueueMessage("DOUGHIE", CenterPoint, @"BISCUIT! It's a miracle!\![br]We've been saved by the gods!");
            DialogueManager.Instance.QueueMessage("BISCUIT", CenterPoint, "[wave freq=10]Ohooooo!");
            DialogueManager.Instance.QueueMessage("DOUGHIE", CenterPoint, @"Now I guess it's back to making... [wave freq=10]fresh bread...\! fresh bread...\! fresh bread...");
            DialogueManager.Instance.QueueMessage("BISCUIT", CenterPoint, "[wave freq=10]Ohoo...");
            await DialogueManager.Instance.WaitForDialogue();
        }
    }

    public void SpawnBread()
    {
        for (int i = 0; i < 2; i++)
        {
            if (!GodotObject.IsInstanceValid(Breads[i]) || Breads[i].Actor.IsToast)
            {
                Breads[i] = BattleManager.Instance.SummonEnemy(SpawnPool[GameManager.Instance.Random.RandiRange(0, SpawnPool.Length - 1)], new Vector2(CenterPoint.X + Offsets[i], CenterPoint.Y), layer: Math.Max(0, Layer - 1));
                Bakes++;
                return;
            }
        }
        GD.PushWarning("Tried to summon more than 2 breads!");
    }
}