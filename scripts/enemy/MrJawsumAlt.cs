using System;
using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OmoriSandbox.Animation;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;
using OmoriSandbox.Extensions;

namespace OmoriSandbox.Actors;
internal sealed class MrJawsumAlt : Enemy
{
    public override string Name => "MR. JAWSUM";
    public override Vector2 InfoBoxOffset => new(0, -225);
    public override bool InfoBoxCursorAboveBox => true;
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/mr_jawsum.tres");
    protected override Stats Stats => new(3000, 1000, 999, 60, 1, 10, 95);
    protected override string[] EquippedSkills => ["MJSummonGator", "MJAttackOrder"];

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry";
    }

    private readonly EnemyComponent[] Gators = new EnemyComponent[2];
    private readonly int[] Offsets = [-145, 145];
    private int Stage = 0;

    public override BattleCommand ProcessAI()
    {
        if (Gators.All(x => !GodotObject.IsInstanceValid(x) || x.Actor.IsToast))
            return new BattleCommand(this, this, Skills["MJSummonGator"]);
        if (Roll() < 21)
            return new BattleCommand(this, SelectAllEnemies(), Skills["MJAttackOrder"]);
        if (Gators.Any(x => !GodotObject.IsInstanceValid(x) || x.Actor.IsToast))
            return new BattleCommand(this, this, Skills["MJSummonGator"]);
        return new BattleCommand(this, SelectAllEnemies(), Skills["MJAttackOrder"]);
    }

    internal void SpawnGatorGuy()
    {
        for (int i = 0; i < 2; i++)
        {
            if (!GodotObject.IsInstanceValid(Gators[i]) || Gators[i].Actor.IsToast)
            {
                Gators[i] = BattleManager.Instance.SummonEnemy("GatorGuyJawsum (Boss Rush)",
                    new Vector2(CenterPoint.X + Offsets[i], CenterPoint.Y + 65), layer: Math.Max(0, Layer - 1));
                return;
            }
        }
        GD.PushWarning("Tried to summon more than 2 gator guys!");
    }

    public override async Task ProcessBattleConditions()
    {

        if (Stage > 2) 
            return;
        
        if (IsBelowHP(0.99f) && Stage == 0)
        {
            DialogueManager.Instance.QueueMessage(this, "I WANT THESE KIDS GONE YOU UNDERSTAND!?");
            await DialogueManager.Instance.WaitForDialogue();
            Stage = 1;
        }
        
        if (IsBelowHP(0.75f) && Stage <= 1)
        {
            DialogueManager.Instance.QueueMessage(this, @"The GATOR GUY who runs them out gets free pizza...\! [shake rate=20]on me!");
            await DialogueManager.Instance.WaitForDialogue();
            Stage = 2;
        }
        
        if (IsBelowHP(0.5f) && Stage <= 2)
        {
            AudioManager.Instance.PlaySFX("se_thunder_bolt", volume: 0.9f);
            AudioManager.Instance.PlaySFX("se_fire_whoosh", pitch: 0.7f, volume: 0.9f);
            AnimationManager.Instance.InitShake(new Shake(1, 8, 15));
            await AnimationManager.Instance.WaitForTintScreen(new Color(1, 0, 0, 0.5f), 0.25f);
            AnimationManager.Instance.TintScreen(ColorsExtension.TransparentBlack, 0.25f);
            SetEmotion("angry", true);
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
