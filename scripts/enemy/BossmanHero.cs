using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;

namespace OmoriSandbox.Actors;

internal sealed class BossmanHero : Enemy
{
    public override string Name => "BOSSMAN HERO";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/bossman_hero.tres");
    protected override Stats Stats => new(8000, 8000, 70, 80, 10, 45, 95);
    protected override string[] EquippedSkills 
        => ["BMHAttack", "BMHThrowMoney", "BMHFlingMoney", "BMHHealFriends", 
            "BMHHealFoes", "BMHBuffFriends", "BMHBuffFoes", "BMHDebuffFriends", 
            "BMHDebuffFoes", "BMHHappyFriends", "BMHHappyFoes", "BMHSadFriends",
            "BMHSadFoes", "BMHAngryFriends", "BMHAngryFoes", "BMHCritFriends",
            "BMHCritFoes", "BMHDamageFriends", "BMHDamageFoes", "GGPizzaParty", "BMHGivePizzaFriends"];

    private readonly Dictionary<int, string> Buffs = new()
    {
        { 0, "BMHHealFriends" }, // fully heal friends
        { 1, "BMHBuffFriends" }, // increase stats of friends
        { 2, "BMHDebuffFoes" }, // reduce stats of foes
        { 3, "BMHHappyFriends" }, // make all friends happy
        { 4, "BMHSadFriends" }, // make all friends sad
        { 5, "BMHAngryFriends" }, // make all friends angry
        { 6, "BMHGivePizzaFriends" }, // give you 10 whole pizzas
        { 7, "BMHCritFriends" } // grant crits to friends
    };
    
    private readonly Dictionary<int, string> Debuffs = new()
    {
        { 0, "BMHHealFoes" }, // fully heal foes
        { 1, "BMHBuffFoes" }, // increase stats of foes
        { 2, "BMHDebuffFriends" }, // reduce stats of friends
        { 3, "BMHHappyFoes" }, // make all foes happy
        { 4, "BMHSadFoes" }, // make all foes sad
        { 5, "BMHAngryFoes" }, // make all foes angry
        { 6, "BMHDamageFriends" }, // deal damage to friends
        { 7, "BMHCritFoes" } // grant crits to foes
    };
    
    private readonly Dictionary<int, int[]> Compatibility = new()
    {
        { 0, [0, 1, 2, 7] },
        { 1, [6, 1, 7] },
        { 2, [0, 6, 2] },
        { 3, [4] },
        { 4, [5] },
        { 5, [3] },
        { 6, [0, 6, 1, 2, 7] },
        { 7, [6, 1, 7] }
    };
    
    /* RULES
       Conflicting offers will not be combined.
       If and only if the contract offers an EMOTION to the friends, the advantageous EMOTION to the friends' one will be offered to the foes.
       If the contract offers buff stats to the friends, healing will not be offered to the foes.
       If the contract offers debuff stats to the foes, 100% crit will not be offered to the foes.
       If the contract offers 100% crit to the friends, damage and debuff stats will not be offered to the friends.
    */
    
    public override bool IsStateValid(string state)
    {
        return state is "neutral" or "toast";
    }

    private readonly EnemyComponent[] GatorGuys = new EnemyComponent[2];

    public override Task OnStartOfBattle()
    {
        GatorGuys[0] = BattleManager.Instance.SummonEnemy("GatorGuyHero", new Vector2(CenterPoint.X - 160, CenterPoint.Y + 65), layer: Math.Max(0, Layer - 1));
        GatorGuys[1] = BattleManager.Instance.SummonEnemy("GatorGuyHero", new Vector2(CenterPoint.X + 180, CenterPoint.Y + 65), layer: Math.Max(0, Layer - 1));
        return Task.CompletedTask;
    }

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["BMHThrowMoney"]);
        
        if (Roll() < 41)
            return new BattleCommand(this, SelectTarget(), Skills["BMHAttack"]);
        if (Roll() < 36)
            return new BattleCommand(this, SelectTarget(), Skills["BMHThrowMoney"]);
        return new BattleCommand(this, SelectAllTargets(), Skills["BMHFlingMoney"]);
    }

    public override async Task OnDefeat()
    {
        foreach (EnemyComponent enemy in GatorGuys)
            enemy.Actor.CurrentHP = 0;
        DialogueManager.Instance.QueueMessage("HERO", CenterPoint, @"Friends...\! Let's...\! make a de...\! Huff...\| Huff...\| Wheeze...");
        await DialogueManager.Instance.WaitForDialogue();
    }

    public override async Task OnEndOfBattle(bool victory)
    {
        if (!victory)
        {
            DialogueManager.Instance.QueueMessage("HERO", CenterPoint, @"Sorry friends...\| You should have taken my offer when you had the chance!");
            await DialogueManager.Instance.WaitForDialogue();
        }
    }

    public override async Task ProcessEndOfTurn()
    {
        BattleLogManager.Instance.ClearBattleLog();
        DialogueManager.Instance.QueueMessage("HERO", CenterPoint, "Friends! Let's make a deal...");
        int buff = GameManager.Instance.Random.RandiRange(0, 7);
        QueueBuffMessage(buff);
        await DialogueManager.Instance.WaitForDialogue();
        int[] options = Compatibility[buff];
        int debuff = options[GameManager.Instance.Random.RandiRange(0, options.Length - 1)];
        QueueDebuffMessage(debuff);
        await DialogueManager.Instance.WaitForDialogue();
        DialogueManager.Instance.QueueMessage("Will you sign HERO's contract?", ["YES", "NO"]);
        string choice = await DialogueManager.Instance.WaitForUserChoice();
        if (choice == "YES")
        {
            DialogueManager.Instance.QueueMessage("HERO", CenterPoint, "Attaboy! It's a deal!");
            await DialogueManager.Instance.WaitForDialogue();
            string buffSkill = Buffs[buff];
            string debuffSkill = Debuffs[debuff];
            BattleManager.Instance.ForceCommand(this, buffSkill.EndsWith("Friends") ? SelectAllTargets() : SelectAllEnemies(), Skills[buffSkill]);
            BattleManager.Instance.ForceCommand(this, debuffSkill.EndsWith("Friends") ? SelectAllTargets() : SelectAllEnemies(), Skills[debuffSkill]);
        }
        else
        {
            DialogueManager.Instance.QueueMessage("HERO", CenterPoint, "That's a real shame...");
            await DialogueManager.Instance.WaitForDialogue();

            if (Roll() < 26)
            {
                EnemyComponent gatorGuy = GatorGuys.FirstOrDefault(x => x.Actor.CurrentState != "toast");
                if (gatorGuy == null)
                    return;
                AudioManager.Instance.PlaySFX("SE_dinosaur", 1.4f);
                DialogueManager.Instance.QueueMessage(gatorGuy.Actor, "Hey BOSS! I'll take your deal!");
                await DialogueManager.Instance.WaitForDialogue();
                
                // when a Gator Guy takes the deal, the buff needs to be swapped to the debuff version
                string gatorBuffSkill = Buffs[debuff];
                string debuffSkill = Debuffs[buff];
                if (buff == 6)
                    BattleManager.Instance.ForceCommand(gatorGuy.Actor, SelectAllEnemies(), Skills["GGPizzaParty"]);
                else
                    BattleManager.Instance.ForceCommand(this, debuffSkill.EndsWith("Friends") ? SelectAllTargets() : SelectAllEnemies(), Skills[debuffSkill]);
                if (debuff == 6) 
                    BattleManager.Instance.ForceCommand(this, SelectAllEnemies(), Skills["BMHDamageFoes"]);
                else
                    BattleManager.Instance.ForceCommand(this, gatorBuffSkill.EndsWith("Friends") ? SelectAllTargets() : SelectAllEnemies(), Skills[gatorBuffSkill]);
            }
        }
    }

    private void QueueBuffMessage(int roll)
    {
        string message = roll switch
        {
            0 => "I will heal your HEART, but...",
            1 => "I will increase your STATS, but...",
            2 => "I will reduce your foes' STATS, but...",
            3 => "I will make you HAPPY, but...",
            4 => "I will make you SAD, but...",
            5 => "I will make you ANGRY, but...",
            6 => "I will give you 10 WHOLE PIZZAS, but...",
            7 => "Your attacks will hit right in the HEART this turn, but...",
            _ => "If you're reading this, Toast messed up badly."
        };
        DialogueManager.Instance.QueueMessage("HERO", CenterPoint, message);
    }
    
    private void QueueDebuffMessage(int roll)
    {
        string message = roll switch
        {
            0 => "I will recover your foes to full HEART!",
            1 => "I will increase your foes' STATS!",
            2 => "I will reduce your STATS!",
            3 => "I will make your foes HAPPY!",
            4 => "I will make your foes SAD!",
            5 => "I will make your foes ANGRY!",
            6 => "I will reduce your HEART and JUICE to 50% of your max HEART and JUICE!",
            7 => "Your foes' attacks will hit right in the HEART this turn!",
            _ => "If you're reading this, Toast messed up badly."
        };
        DialogueManager.Instance.QueueMessage("HERO", CenterPoint, message);
    }
    
}