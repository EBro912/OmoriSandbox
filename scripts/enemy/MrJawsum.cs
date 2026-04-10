using System;
using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OmoriSandbox.Animation;
using OmoriSandbox.Battle;
using OmoriSandbox.Extensions;

namespace OmoriSandbox.Actors;
internal sealed class MrJawsum : Enemy
{
    public override string Name => "MR. JAWSUM";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/mr_jawsum.tres");
    protected override Stats Stats => new(500, 250, 999, 20, 1, 10, 95);
    protected override string[] EquippedSkills => ["MJSummonGator", "MJAttackOrder"];

    public override bool IsStateValid(string state)
    {
        return state == "neutral" || state == "happy" || state == "sad"
               || state == "angry" || state == "hurt" || state == "toast";
    }

    public readonly List<EnemyComponent> GatorGuys = [];
    private int Stage = 0;

    public override BattleCommand ProcessAI()
    {
        if (GatorGuys.Count == 0)
            return new BattleCommand(this, this, Skills["MJSummonGator"]);
        if (Roll() < 21)
            return new BattleCommand(this, SelectAllEnemies(), Skills["MJAttackOrder"]);
        if (GatorGuys.Count < 2)
            return new BattleCommand(this, this, Skills["MJSummonGator"]);
        return new BattleCommand(this, SelectAllEnemies(), Skills["MJAttackOrder"]);
    }

    internal void SpawnGatorGuy()
    {
        GatorGuys.RemoveAll(x => x.Actor.CurrentHP <= 0);
        
        if (GatorGuys.Count == 0)
           GatorGuys.Add(BattleManager.Instance.SummonEnemy("GatorGuyJawsum", new Vector2(CenterPoint.X - 145, CenterPoint.Y + 65), layer: Math.Max(0, Layer - 1)));
        else if (GatorGuys.Count == 1)
            GatorGuys.Add(BattleManager.Instance.SummonEnemy("GatorGuyJawsum", new Vector2(CenterPoint.X + 145, CenterPoint.Y + 65), layer: Math.Max(0, Layer - 1)));
        else
        {
            GD.PushWarning("Tried to summon more than 2 gator guys!");
        }
    }

    public override async Task ProcessBattleConditions()
    {
        GatorGuys.RemoveAll(x => x.Actor.CurrentHP <= 0);

        if (CurrentHP <= 0) return;

        if (Stage > 2) 
            return;
        
        if (CurrentHP < 495 && Stage == 0)
        {
            DialogueManager.Instance.QueueMessage(this, "I WANT THESE KIDS GONE YOU UNDERSTAND!?");
            await DialogueManager.Instance.WaitForDialogue();
            Stage = 1;
        }
        
        if (CurrentHP < 375 && Stage <= 1)
        {
            DialogueManager.Instance.QueueMessage(this, @"The GATOR GUY who runs them out gets free pizza...\! [shake rate=20]on me!");
            await DialogueManager.Instance.WaitForDialogue();
            Stage = 2;
        }
        
        if (CurrentHP < 250 && Stage <= 2)
        {
            AudioManager.Instance.PlaySFX("se_thunder_bolt", volume: 0.9f);
            AudioManager.Instance.PlaySFX("se_fire_whoosh", volume: 0.7f);
            AnimationManager.Instance.InitShake(new Shake(29, 100, 15));
            await AnimationManager.Instance.WaitForTintScreen(new Color(1, 0, 0, 0.5f), 0.25f);
            AnimationManager.Instance.TintScreen(ColorsExtension.TransparentBlack, 0.25f);
            SetState("angry", true);
            DialogueManager.Instance.QueueMessage(this, @"What do you mean we're running low on henchmen!?\! That's impossible!");
            await DialogueManager.Instance.WaitForDialogue();
            Stage = 3;
        }
    }

    public override async Task OnDefeat()
    {
        DialogueManager.Instance.QueueMessage(this, "You let yourselves be foiled by a bunch of children!?");
        DialogueManager.Instance.QueueMessage(this, "WHAT DID I EVEN HIRE YOU FOR!?");
        await DialogueManager.Instance.WaitForDialogue();
    }

    public override async Task OnStartOfBattle()
    {
        AddStatModifier("MinionBarrier");
        SpawnGatorGuy();
        SpawnGatorGuy();
        DialogueManager.Instance.QueueMessage(this, @"Boys...\! would you be so kind as to show these kids the way out?");
        await DialogueManager.Instance.WaitForDialogue();
    }
    public override async Task OnEndOfBattle(bool victory)
    {
        if (!victory) {
            DialogueManager.Instance.QueueMessage("[shake amp=50.0][font_size=40]JAWHAW[font_size=52]HAW[font_size=64]HAW!!!");
            DialogueManager.Instance.QueueMessage(this, "That's what happens when you mess with MR. JAWSUM!");
            await DialogueManager.Instance.WaitForDialogue();
        }
    }
}
