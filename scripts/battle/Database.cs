using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class Database
{
    private static readonly Dictionary<string, Skill> Skills = [];
    private static readonly Dictionary<string, Item> Items = [];

    public static bool TryGetSkill(string name, out Skill skill)
    {
        return Skills.TryGetValue(name, out skill);
    }

    public static bool TryGetItem(string name, out Item item)
    {
        return Items.TryGetValue(name, out item);
    }

    public static void Init()
    {
        #region SKILLS
        // OMORI //
        Skills["OAttack"] = new Skill
        {
            Name = "Attack",
            Description = "Basic Attack",
            Cost = 0,
            Hidden = true,
            GoesFirst = false,
            Target = SkillTarget.Enemy,
            AnimationId = 3,
            Effect = async (self, target, skill) =>
            {
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
                GameManager.Instance.BattleManager.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false);
            }
        };
        Skills["SadPoem"] = new Skill
        {
            Name = "SAD POEM",
            Description = "Inflicts SAD on a friend or foe.\nCost: 5",
            Cost = 5,
            Hidden = false,
            GoesFirst = false,
            Target = SkillTarget.AllyOrEnemy,
            AnimationId = 5,
            Effect = async (self, target, skill) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] reads a sad poem.");
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, self);
                string state = "sad";
                switch (target.CurrentState)
                {
                    case "miserable":
                        BattleLogManager.Instance.QueueMessage(self, target, "[target] cannot be any sadder!");
                        return;
                    case "depressed":
                        state = "miserable";
                        break;
                    case "sad":
                        state = "depressed";
                        break;
                }
                if (target.IsStateValid(state))
                    target.SetState(state);
                else
                    BattleLogManager.Instance.QueueMessage(self, target, "[target] cannot be any sadder!");
            }
        };
        Skills["LuckySlice"] = new Skill
        {
            Name = "LUCKY SLICE",
            Description = "Acts first. An attack that's stronger\nwhen OMORI is HAPPY. Cost: 15",
            Cost = 15,
            Hidden = false,
            GoesFirst = true,
            Target = SkillTarget.Enemy,
            AnimationId = 8,
            Effect = async (self, target, skill) =>
            {
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] lunges at [target]!");
                if (self.CurrentState == "happy" || self.CurrentState == "ecstatic" || self.CurrentState == "manic")
                    GameManager.Instance.BattleManager.Damage(self, target, () => { return (self.CurrentStats.ATK + self.CurrentStats.LCK) * 2f - target.CurrentStats.DEF; }, false);
                else
                    GameManager.Instance.BattleManager.Damage(self, target, () => { return (self.CurrentStats.ATK + self.CurrentStats.LCK) * 1.5f - target.CurrentStats.DEF; }, false);
            }
        };
        Skills["Stab"] = new Skill
        {
            Name = "STAB",
            Description = "Always deals a critical hit.\nIgnores DEFENSE when OMORI is sad. Cost: 13",
            Cost = 13,
            Hidden = false,
            GoesFirst = true,
            Target = SkillTarget.Enemy,
            AnimationId = 9,
            Effect = async (self, target, skill) =>
            {
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] stabs [target].");
                if (self.CurrentState == "sad" || self.CurrentState == "depressed" || self.CurrentState == "miserable")
                    GameManager.Instance.BattleManager.Damage(self, target, () => { return self.CurrentStats.ATK * 2f; }, false, guaranteeCrit: true);
                else
                    GameManager.Instance.BattleManager.Damage(self, target, () => { return self.CurrentStats.ATK * 1.5f - target.CurrentStats.DEF; }, false, guaranteeCrit: true);
            }
        };
        Skills["Trick"] = new Skill
        {
            Name = "TRICK",
            Description = "Deals damage. If the foe is HAPPY, greatly\nreduce it's SPEED. Cost: 20",
            Cost = 20,
            Hidden = false,
            GoesFirst = false,
            Target = SkillTarget.Enemy,
            AnimationId = 10,
            Effect = async (self, target, skill) =>
            {
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] tricks [target].");
                if (target.CurrentState == "happy" || target.CurrentState == "ecstatic" || target.CurrentState == "manic")
                {
                    GameManager.Instance.AnimationManager.PlayAnimation(219, target);
                    target.AddStatModifier(Modifier.SpeedDown, 3);
                }
                GameManager.Instance.BattleManager.Damage(self, target, () => { return self.CurrentStats.ATK * 3f - target.CurrentStats.DEF; }, false);
                await Task.Delay(334);
            }
        };

        // SUNNY

        Skills["SAttack"] = new Skill
        {
            Name = "Attack",
            Description = "Basic Attack",
            Cost = 0,
            Hidden = true,
            GoesFirst = false,
            Target = SkillTarget.Enemy,
            AnimationId = 108,
            Effect = async (self, target, skill) =>
            {
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
                GameManager.Instance.BattleManager.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false);
            }
        };

        Skills["CalmDown"] = new Skill
        {
            Name = "CALM DOWN",
            Description = "Removes EMOTIONS and heals some HEART.\nCost: 0",
            Cost = 0,
            Hidden = false,
            GoesFirst = true,
            Target = SkillTarget.Self,
            AnimationId = 104,
            Effect = async (self, target, skill) =>
            {
                AudioManager.Instance.FadeBGMTo(10f);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] calms down.");
                GameManager.Instance.AnimationManager.PlayAnimation(skill.AnimationId);
                await Task.Delay(2500);
                self.Heal((int)Math.Round(self.BaseStats.MaxHP * 0.5, MidpointRounding.AwayFromZero));
                self.SetState("neutral");
                AudioManager.Instance.FadeBGMTo(100f);
            }
        };


        // AUBREY //
        Skills["AAttack"] = new Skill
        {
            Name = "Attack",
            Description = "Basic Attack",
            Cost = 0,
            Hidden = true,
            GoesFirst = false,
            Target = SkillTarget.Enemy,
            AnimationId = 28,
            Effect = async (self, target, skill) =>
            {
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
                GameManager.Instance.BattleManager.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false);
            }
        };
        Skills["PepTalk"] = new Skill
        {
            Name = "PEP TALK",
            Description = "Makes a friend or foe HAPPY.\nCost: 5",
            Cost = 5,
            Hidden = false,
            GoesFirst = false,
            Target = SkillTarget.AllyOrEnemy,
            AnimationId = 29,
            Effect = async (self, target, skill) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] cheers on [target]!");
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId);
                string state = "happy";
                switch (target.CurrentState)
                {
                    case "manic":
                        BattleLogManager.Instance.QueueMessage(self, target, "[target] cannot be any happier!");
                        return;
                    case "ecstatic":
                        state = "manic";
                        break;
                    case "happy":
                        state = "ecstatic";
                        break;
                }
                if (target.IsStateValid(state))
                    target.SetState(state);
                else
                    BattleLogManager.Instance.QueueMessage(self, target, "[target] cannot be any happier!");
            }
        };
        Skills["Headbutt"] = new Skill
        {
            Name = "HEADBUTT",
            Description = "Deals big damage, but AUBREY also takes damage.\nStronger when AUBREY is ANGRY. Cost: 5",
            Cost = 5,
            Hidden = false,
            GoesFirst = false,
            Target = SkillTarget.Enemy,
            AnimationId = 30,
            Effect = async (self, target, skill) =>
            {
                double neededHp = Math.Floor(self.CurrentStats.MaxHP * 0.2);
                if (self.CurrentHP < neededHp)
                {
                    BattleLogManager.Instance.QueueMessage(self, target, "[actor] does not have enough HP!");
                    // refund juice
                    self.CurrentJuice += skill.Cost;
                    return;
                }
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] headbutts [target]!");
                if (self.CurrentState == "angry" || self.CurrentState == "enraged")
                    GameManager.Instance.BattleManager.Damage(self, target, () => { return self.CurrentStats.ATK * 3f - target.CurrentStats.DEF; }, false);
                else
                    GameManager.Instance.BattleManager.Damage(self, target, () => { return self.CurrentStats.ATK * 2.5f - target.CurrentStats.DEF; }, false);
                self.CurrentHP = (int)Math.Max(1f, self.CurrentHP - Math.Floor(self.CurrentStats.MaxHP * 0.2));
            }
        };

        Skills["PowerHit"] = new Skill
        {
            Name = "POWER HIT",
            Description = "An attack that ignore's a foe's DEFENSE,\nthen reduces the foe's DEFENSE. Cost: 20",
            Cost = 20,
            Hidden = false,
            GoesFirst = false,
            Target = SkillTarget.Enemy,
            AnimationId = 31,
            Effect = async (self, target, skill) =>
            {
                GameManager.Instance.AnimationManager.PlayAnimation(skill.AnimationId, target);
                await Task.Delay(1000);
                await GameManager.Instance.AnimationManager.WaitForAnimation(219, target);
                target.AddStatModifier(Modifier.DefenseDown);
                GameManager.Instance.BattleManager.Damage(self, target, () => { return self.CurrentStats.ATK * 2f; }, false);
            }
        };

        Skills["Twirl"] = new Skill
        {
            Name = "TWIRL",
            Description = "AUBREY attacks a foe and becomes HAPPY.\nCost: 10",
            Cost = 10,
            Hidden = false,
            GoesFirst = false,
            Target = SkillTarget.Enemy,
            AnimationId = 45,
            Effect = async (self, target, skill) =>
            {
                GameManager.Instance.AnimationManager.PlayAnimation(skill.AnimationId, target);
                await Task.Delay(500);
                GameManager.Instance.AnimationManager.PlayAnimation(28, target);
                await Task.Delay(500);
                GameManager.Instance.BattleManager.Damage(self, target, () => { return (self.CurrentStats.ATK * 2f + self.CurrentStats.LCK) - target.CurrentStats.DEF; }, false);
                string state = "happy";
                switch (self.CurrentState)
                {
                    case "manic":
                        BattleLogManager.Instance.QueueMessage(self, target, "[target] cannot be any happier!");
                        return;
                    case "ecstatic":
                        state = "manic";
                        break;
                    case "happy":
                        state = "ecstatic";
                        break;
                }
                if (self.IsStateValid(state))
                    self.SetState(state);
                else
                    BattleLogManager.Instance.QueueMessage(self, target, "[target] cannot be any happier!");

            }
        };

        Skills["ARWAttack"] = new Skill
        {
            Name = "Attack",
            Description = "Basic Attack",
            Cost = 0,
            Hidden = true,
            GoesFirst = false,
            Target = SkillTarget.Enemy,
            AnimationId = 48,
            Effect = async (self, target, skill) =>
            {
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
                GameManager.Instance.BattleManager.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false);
            }
        };

        Skills["Homerun"] = new Skill
        {
            Name = "HOMERUN",
            Description = "Has a chance to instantly defeat a\nfoe. AUBREY also takes damage. Cost: 25",
            Cost = 25,
            Hidden = false,
            GoesFirst = false,
            Target = SkillTarget.Enemy,
            AnimationId = 32,
            Effect = async (self, target, skill) =>
            {
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] hits a home run!");
                GameManager.Instance.BattleManager.Damage(self, target, () => { return self.CurrentStats.ATK * 4f - target.CurrentStats.DEF; });
                int roll = GameManager.Instance.Random.RandiRange(0, 100);
                if (roll < 11)
                {
                    target.CurrentHP = 0;
                }
                self.CurrentHP = Math.Max(0, (int)Math.Round(self.CurrentHP - self.BaseStats.MaxHP * 0.2f, MidpointRounding.AwayFromZero));
            }
        };

        // KEL //
        Skills["KAttack"] = new Skill
        {
            Name = "Attack",
            Description = "Basic Attack",
            Cost = 0,
            Hidden = true,
            GoesFirst = false,
            Target = SkillTarget.Enemy,
            AnimationId = 54,
            Effect = async (self, target, skill) =>
            {
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
                GameManager.Instance.BattleManager.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false);
            }
        };
        Skills["Annoy"] = new Skill
        {
            Name = "ANNOY",
            Description = "Makes a friend or foe ANGRY.\nCost: 5",
            Cost = 5,
            Hidden = false,
            GoesFirst = false,
            Target = SkillTarget.AllyOrEnemy,
            AnimationId = 55,
            Effect = async (self, target, skill) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] annoys [target]!");
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId);
                string state = "angry";
                switch (target.CurrentState)
                {
                    case "furious":
                        BattleLogManager.Instance.QueueMessage(self, target, "[target] cannot be any angrier!");
                        return;
                    case "enraged":
                        state = "furious";
                        break;
                    case "angry":
                        state = "enraged";
                        break;
                }
                if (target.IsStateValid(state))
                    target.SetState(state);
                else
                    BattleLogManager.Instance.QueueMessage(self, target, "[target] cannot be any angrier!");
            }
        };
        Skills["Rebound"] = new Skill
        {
            Name = "REBOUND",
            Description = "Deals damage to all foes.\nCost: 15",
            Cost = 15,
            Hidden = false,
            GoesFirst = false,
            Target = SkillTarget.AllEnemies,
            AnimationId = 56,
            Effect = async (self, target, skill) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor]'s ball bounces everywhere!");
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId);
                foreach (Enemy enemy in GameManager.Instance.BattleManager.GetAllEnemies())
                    GameManager.Instance.BattleManager.Damage(self, enemy, () => { return self.CurrentStats.ATK * 2.5f - enemy.CurrentStats.DEF; }, false);
            }
        };
        Skills["Ricochet"] = new Skill
        {
            Name = "RICOCHET",
            Description = "Deals damage to a foe 3 times.\nCost: 30",
            Cost = 30,
            Hidden = false,
            GoesFirst = false,
            Target = SkillTarget.Enemy,
            AnimationId = 58,
            Effect = async (self, target, skill) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] does a fancy ball trick!");
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId);
                GameManager.Instance.BattleManager.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false, 0.3f);
                await Task.Delay(1000);
                GameManager.Instance.BattleManager.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false, 0.3f);
                await Task.Delay(1000);
                GameManager.Instance.BattleManager.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false, 0.3f);
            }
        };
        Skills["Flex"] = new Skill
        {
            Name = "FLEX",
            Description = "KEL deals more damage next turn and increases\nHIT RATE for his next attack. Cost: 10",
            Cost = 10,
            Hidden = false,
            GoesFirst = false,
            Target = SkillTarget.Self,
            AnimationId = 57,
            Effect = async (self, target, skill) =>
            {
                GameManager.Instance.AnimationManager.PlayAnimation(skill.AnimationId);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] flexes and feels his best!");
                BattleLogManager.Instance.QueueMessage(self, target, "[actor]'s HIT RATE rose!");
                self.AddStatModifier(Modifier.Flex, turns: int.MaxValue);
                await Task.CompletedTask;
            }
        };

        Skills["KRWAttack"] = new Skill
        {
            Name = "Attack",
            Description = "Basic Attack",
            Cost = 0,
            Hidden = true,
            GoesFirst = false,
            Target = SkillTarget.Enemy,
            AnimationId = 77,
            Effect = async (self, target, skill) =>
            {
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
                GameManager.Instance.BattleManager.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false);
            }
        };

        Skills["Encourage"] = new Skill
        {
            Name = "ENCOURAGE",
            Description = "KEL encourages a friend.\nRaises their attack. No cost.",
            Cost = 0,
            Hidden = false,
            GoesFirst = false,
            Target = SkillTarget.Ally,
            AnimationId = 214,
            Effect = async (self, target, skill) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] gives some encouragement!");
                GameManager.Instance.AnimationManager.PlayAnimation(skill.AnimationId, target);
                await Task.Delay(1000);
                target.AddStatModifier(Modifier.AttackUp);
            }
        };

        // HERO //
        Skills["HAttack"] = new Skill
        {
            Name = "Attack",
            Description = "Basic Attack",
            Cost = 0,
            Hidden = true,
            GoesFirst = false,
            Target = SkillTarget.Enemy,
            AnimationId = 83,
            Effect = async (self, target, skill) =>
            {
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
                GameManager.Instance.BattleManager.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false);
            }
        };
        Skills["Massage"] = new Skill
        {
            Name = "MASSAGE",
            Description = "Removes a friend or foe's EMOTION.\nCost: 5",
            Cost = 5,
            Hidden = false,
            GoesFirst = false,
            Target = SkillTarget.AllyOrEnemy,
            AnimationId = 86,
            Effect = async (self, target, skill) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] massages [target]!");
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId);
                target.SetState("neutral");
                BattleLogManager.Instance.QueueMessage(target.Name.ToUpper() + " calms down...");
            }
        };
        Skills["Cook"] = new Skill
        {
            Name = "COOK",
            Description = "Heals a friend for 75% of their HEART.\nCost: 10",
            Cost = 10,
            Hidden = false,
            GoesFirst = false,
            Target = SkillTarget.Ally,
            AnimationId = 85,
            Effect = async (self, target, skill) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] makes a cookie just for [target]!");
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, target);
                float heal = target.CurrentStats.MaxHP * 0.75f;
                float variance = GameManager.Instance.Random.RandfRange(0.8f, 1.2f);
                int finalHeal = (int)Math.Round(heal * variance, MidpointRounding.AwayFromZero);
                target.Heal(finalHeal);
                GameManager.Instance.BattleManager.SpawnDamageNumber(finalHeal, target.CenterPoint, DamageType.Heal);
                GameManager.Instance.AnimationManager.PlayAnimation(212, target);
                BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovered {finalHeal} HEART!");
                await Task.Delay(1000);
            }
        };
        Skills["Refresh"] = new Skill
        {
            Name = "REFRESH",
            Description = "Heals 50% of a friend's JUICE.\nCost: 40",
            Cost = 40,
            Hidden = false,
            GoesFirst = false,
            Target = SkillTarget.Ally,
            AnimationId = 213,
            Effect = async (self, target, skill) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] makes a refreshment for [target].");
                GameManager.Instance.AnimationManager.PlayAnimation(skill.AnimationId, target);
                int heal = (int)Math.Round(target.CurrentStats.MaxJuice * 0.5f, MidpointRounding.AwayFromZero);
                target.HealJuice(heal);
                GameManager.Instance.BattleManager.SpawnDamageNumber(heal, target.CenterPoint, DamageType.JuiceGain);
                BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovered {heal} JUICE!");
                await Task.Delay(1000);
            }
        };
        Skills["HomemadeJam"] = new Skill
        {
            Name = "HOMEMADE JAM",
            Description = "Brings back a friend that is TOAST.\nCost: 40",
            Cost = 40,
            Hidden = false,
            GoesFirst = false,
            Target = SkillTarget.DeadAlly,
            AnimationId = 269,
            Effect = async (self, target, skill) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] makes HOMEMADE JAM!");
                if (target.CurrentState != "toast")
                {
                    target = GameManager.Instance.BattleManager.GetRandomDeadPartyMember();
                    if (target == null)
                    {
                        BattleLogManager.Instance.QueueMessage("It had no effect.");
                        return;
                    }
                }
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, target);
                target.SetState("neutral");
                int heal = (int)Math.Round(target.CurrentStats.MaxHP * 0.7f, MidpointRounding.AwayFromZero);
                target.Heal(heal);
                GameManager.Instance.BattleManager.SpawnDamageNumber(heal, target.CenterPoint, DamageType.Heal);
                BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovered {heal} HEART!");
                BattleLogManager.Instance.QueueMessage(self, target, "[target] rose again!");
                await Task.Delay(1000);
            }
        };

        Skills["HRWAttack"] = new Skill
        {
            Name = "Attack",
            Description = "Basic Attack",
            Cost = 0,
            Hidden = true,
            GoesFirst = false,
            Target = SkillTarget.Enemy,
            AnimationId = 99,
            Effect = async (self, target, skill) =>
            {
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
                GameManager.Instance.BattleManager.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false);
            }
        };

        Skills["FirstAid"] = new Skill
        {
            Name = "FIRST AID",
            Description = "Heals a friend for 25% of their HEART.\nCost: 10",
            Cost = 10,
            Hidden = false,
            GoesFirst = false,
            Target = SkillTarget.Ally,
            AnimationId = 114,
            Effect = async (self, target, skill) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] provides first aid!");
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, target);
                float heal = target.CurrentStats.MaxHP * 0.25f;
                float variance = GameManager.Instance.Random.RandfRange(0.8f, 1.2f);
                int finalHeal = (int)Math.Round(heal * variance, MidpointRounding.AwayFromZero);
                target.Heal(finalHeal);
                GameManager.Instance.BattleManager.SpawnDamageNumber(finalHeal, target.CenterPoint, DamageType.Heal);
                GameManager.Instance.AnimationManager.PlayAnimation(212, target);
                BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovered {finalHeal} HEART!");
                await Task.Delay(1000);
            }
        };

        // LOST SPROUT MOLE //
        Skills["LSMAttack"] = new Skill
        {
            Name = "Attack",
            Description = "Basic Attack",
            Cost = 0,
            Hidden = true,
            GoesFirst = false,
            Target = SkillTarget.Enemy,
            AnimationId = 123,
            Effect = async (self, target, skill) =>
            {
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] bumps into [target]!");
                GameManager.Instance.BattleManager.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false);
            }
        };

        Skills["LSMDoNothing"] = new Skill
        {
            Name = "Do Nothing",
            Description = "Does nothing",
            Cost = 0,
            Hidden = true,
            GoesFirst = false,
            Target = SkillTarget.Enemy,
            AnimationId = -1,
            Effect = async (self, target, skill) =>
            {
                AudioManager.Instance.PlaySFX("BA_do_nothing_dance");
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] is rolling around.");
                await Task.CompletedTask;
            }
        };

        Skills["LSMRunAround"] = new Skill
        {
            Name = "Run Around",
            Description = "Run Around",
            Cost = 0,
            Hidden = true,
            GoesFirst = false,
            Target = SkillTarget.Enemy,
            AnimationId = 200,
            Effect = async (self, target, skill) =>
            {
                GameManager.Instance.AnimationManager.PlayAnimation(skill.AnimationId);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] runs around!");
                await Task.Delay(100);
                target = GameManager.Instance.BattleManager.GetRandomAlivePartyMember();
                GameManager.Instance.BattleManager.Damage(self, target, () => { return self.CurrentStats.ATK * 1.5f - target.CurrentStats.DEF; }, false);
                await Task.Delay(917);
                target = GameManager.Instance.BattleManager.GetRandomAlivePartyMember();
                GameManager.Instance.BattleManager.Damage(self, target, () => { return self.CurrentStats.ATK * 1.5f - target.CurrentStats.DEF; }, false);
            }
        };

        // FOREST BUNNY? //
        Skills["FBQAttack"] = new Skill
        {
            Name = "Attack",
            Description = "Basic Attack",
            Cost = 0,
            Hidden = true,
            GoesFirst = false,
            Target = SkillTarget.Enemy,
            AnimationId = 123,
            Effect = async (self, target, skill) =>
            {
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] nibbles at [target]?");
                GameManager.Instance.BattleManager.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false);
            }
        };

        Skills["FBQDoNothing"] = new Skill
        {
            Name = "Do Nothing",
            Description = "Does nothing",
            Cost = 0,
            Hidden = true,
            GoesFirst = false,
            Target = SkillTarget.Enemy,
            AnimationId = -1,
            Effect = async (self, target, skill) =>
            {
                AudioManager.Instance.PlaySFX("BA_do_nothing_falls_over");
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] is hopping around?");
                await Task.CompletedTask;
            }
        };

        Skills["FBQBeCute"] = new Skill
        {
            Name = "Be Cute",
            Description = "Be Cute",
            Cost = 0,
            Hidden = true,
            GoesFirst = false,
            Target = SkillTarget.Enemy,
            AnimationId = 148,
            Effect = async (self, target, skill) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] winks at [target]?");
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, self);
                await GameManager.Instance.AnimationManager.WaitForAnimation(215, target);
                target.AddStatModifier(Modifier.AttackDown);
            }
        };

        Skills["FBQSadEyes"] = new Skill
        {
            Name = "Sad Eyes",
            Description = "Sad Eyes",
            Cost = 0,
            Hidden = true,
            GoesFirst = false,
            Target = SkillTarget.Enemy,
            AnimationId = 149,
            Effect = async (self, target, skill) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] looks sadly at [target]?");
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, self);
                string state = "sad";
                switch (target.CurrentState)
                {
                    case "miserable":
                        BattleLogManager.Instance.QueueMessage(target.Name.ToUpper() + " cannot be any sadder!");
                        return;
                    case "depressed":
                        state = "miserable";
                        break;
                    case "sad":
                        state = "depressed";
                        break;
                }
                if (target.IsStateValid(state))
                    target.SetState(state);
                else
                    BattleLogManager.Instance.QueueMessage(target.Name.ToUpper() + " cannot be any sadder!");
            }
        };

        // SWEETHEART //
        Skills["SHAttack"] = new Skill
        {
            Name = "Attack",
            Description = "Basic Attack",
            Cost = 0,
            Hidden = true,
            GoesFirst = false,
            Target = SkillTarget.Enemy,
            AnimationId = 132,
            Effect = async (self, target, skill) =>
            {
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] slaps [target].");
                GameManager.Instance.BattleManager.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false);
            }
        };

        Skills["SharpInsult"] = new Skill
        {
            Name = "Sharp Insult",
            Description = "Sharp Insult",
            Cost = 0,
            Hidden = true,
            GoesFirst = false,
            Target = SkillTarget.AllEnemies,
            AnimationId = 183,
            Effect = async (self, target, skill) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] insults everyone!");
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId);
                foreach (PartyMemberComponent member in GameManager.Instance.BattleManager.GetAlivePartyMembers()) {
                    GameManager.Instance.BattleManager.Damage(self, member.Actor, () => { return self.CurrentStats.ATK; }, false, 0.1f, neverCrit: true);
                    string state = "angry";
                    switch (member.Actor.CurrentState)
                    {
                        case "furious":
                            BattleLogManager.Instance.QueueMessage(member.Actor.Name.ToUpper() + " cannot be any angrier!");
                            continue;
                        case "enraged":
                            state = "furious";
                            break;
                        case "angry":
                            state = "enraged";
                            break;
                    }
                    if (member.Actor.IsStateValid(state))
                        member.Actor.SetState(state);
                    else
                        BattleLogManager.Instance.QueueMessage(member.Actor.Name.ToUpper() + " cannot be any angrier!");
                }
            }
        };

        Skills["SwingMace"] = new Skill
        {
            Name = "Swing Mace",
            Description = "Swing Mace",
            Cost = 0,
            Hidden = true,
            GoesFirst = false,
            Target = SkillTarget.AllEnemies,
            AnimationId = 206,
            Effect = async (self, target, skill) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] swings her mace!");
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId);
                foreach (PartyMemberComponent member in GameManager.Instance.BattleManager.GetAlivePartyMembers())
                {
                    GameManager.Instance.BattleManager.Damage(self, member.Actor, () => { return self.CurrentStats.ATK * 2.5f - member.Actor.CurrentStats.DEF; }, false);
                }
            }
        };

        Skills["Brag"] = new Skill
        {
            Name = "Brag",
            Description = "Brag",
            Cost = 0,
            Hidden = true,
            GoesFirst = false,
            Target = SkillTarget.Self,
            AnimationId = 162,
            Effect = async (self, target, skill) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] boasts about one of her\nmany, many talents!");
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId);
                string state = "happy";
                switch (self.CurrentState)
                {
                    case "manic":
                        BattleLogManager.Instance.QueueMessage(self.Name.ToUpper() + " cannot be any happier!");
                        return;
                    case "ecstatic":
                        state = "manic";
                        break;
                    case "happy":
                        state = "ecstatic";
                        break;
                }
                if (self.IsStateValid(state))
                    self.SetState(state);
                else
                    BattleLogManager.Instance.QueueMessage(self.Name.ToUpper() + " cannot be any happier!");
            }
        };
        #endregion

        #region SNACKS
        Items["HOT DOG"] = new Item
        {
            Name = "HOT DOG",
            Description = "Better than a cold dog.\nHeals 100 HEART.",
            Target = SkillTarget.Ally,
            AnimationId = 212,
            Effect = async (self, target, item) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] uses HOT DOG!");
                GameManager.Instance.AnimationManager.PlayAnimation(212, target);
                target.Heal(100);
                GameManager.Instance.BattleManager.SpawnDamageNumber(100, target.CenterPoint, DamageType.Heal);
                BattleLogManager.Instance.QueueMessage(self, target, "[target] recovered 100 HEART!");
                await Task.CompletedTask;
            }
        };

        Items["CHOCOLATE"] = new Item
        {
            Name = "CHOCOLATE",
            Description = "Chocolate!? Oh, it's baking chocolate...\nHeals 40% of HEART.",
            Target = SkillTarget.Ally,
            AnimationId = 212,
            Effect = async (self, target, item) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] uses CHOCOLATE!");
                GameManager.Instance.AnimationManager.PlayAnimation(212, target);
                float heal = target.CurrentStats.MaxHP * 0.4f;
                int finalHeal = (int)Math.Round(heal, MidpointRounding.AwayFromZero);
                target.Heal(finalHeal);
                GameManager.Instance.BattleManager.SpawnDamageNumber(finalHeal, target.CenterPoint, DamageType.Heal);
                BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovered {finalHeal} HEART!");
                await Task.CompletedTask;
            }
        };

        Items["LIFE JAM"] = new Item
        {
            Name = "LIFE JAM",
            Description = "Infused with the spirit of life.\nRevives a friend that is TOAST.",
            Target = SkillTarget.DeadAlly,
            AnimationId = 269,
            Effect = async (self, target, item) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] uses LIFE JAM!");
                if (target.CurrentState != "toast")
                {
                    target = GameManager.Instance.BattleManager.GetRandomDeadPartyMember();
                    if (target == null)
                    {
                        BattleLogManager.Instance.QueueMessage("It had no effect.");
                        return;
                    }
                }
                await GameManager.Instance.AnimationManager.WaitForAnimation(item.AnimationId, target);
                target.CurrentHP = target.CurrentStats.MaxHP / 2;
                target.SetState("neutral");
                BattleLogManager.Instance.QueueMessage(self, target, "[target] rose again!");
            }
        };

        Items["LEMONADE"] = new Item
        {
            Name = "LEMONADE",
            Description = "When life gives you lemons, you make this!\nHeals 75 JUICE.",
            Target = SkillTarget.Ally,
            AnimationId = 212,
            Effect = async (self, target, item) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] uses LEMONADE!");
                GameManager.Instance.AnimationManager.PlayAnimation(213, target);
                target.HealJuice(75);
                GameManager.Instance.BattleManager.SpawnDamageNumber(75, target.CenterPoint, DamageType.JuiceGain);
                BattleLogManager.Instance.QueueMessage(self, target, "[target] recovered 75 JUICE!");
                await Task.CompletedTask;
            }
        };
        #endregion
        #region TOYS
        Items["RUBBER BAND"] = new Item
        {
            Name = "RUBBER BAND",
            Description = "Deals damage to a for and reduces\ntheir DEFENSE.",
            IsToy = true,
            Target = SkillTarget.Enemy,
            AnimationId = 219,
            Effect = async (self, target, item) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] uses RUBBER BAND!");
                GameManager.Instance.BattleManager.Damage(self, target, () => { return 50; }, true, 0, neverCrit: true);
                await GameManager.Instance.AnimationManager.WaitForAnimation(item.AnimationId, target);
                target.AddStatModifier(Modifier.DefenseDown);
            }
        };

        Items["AIR HORN"] = new Item
        {
            Name = "AIR HORN",
            Description = "Who would invent this!?\nInflicts ANGER on all friends.",
            IsToy = true,
            Target = SkillTarget.AllAllies,
            AnimationId = -1,
            Effect = async (self, target, item) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] uses AIR HORN!");
                AudioManager.Instance.PlaySFX("SE_airhorn", 1, 0.9f);
                foreach (PartyMemberComponent member in GameManager.Instance.BattleManager.GetAlivePartyMembers())
                {
                    string state = "angry";
                    switch (member.Actor.CurrentState)
                    {
                        case "furious":
                            BattleLogManager.Instance.QueueMessage(self, member.Actor, "[target] cannot be any angrier!");
                            continue;
                        case "enraged":
                            state = "furious";
                            break;
                        case "angry":
                            state = "enraged";
                            break;
                    }
                    if (member.Actor.IsStateValid(state))
                        member.Actor.SetState(state);
                    else
                        BattleLogManager.Instance.QueueMessage(self, member.Actor, "[target] cannot be any angrier!");
                }
                await Task.CompletedTask;
            }
        };
        #endregion
    }
}