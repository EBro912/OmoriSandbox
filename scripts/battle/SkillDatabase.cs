using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class SkillDatabase
{
    private static readonly Dictionary<string, Skill> Skills = [];

    public static bool TryGetSkill(string name, out Skill skill)
    {
        return Skills.TryGetValue(name, out skill);
    }

    public static void Init()
    {
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
                GameManager.Instance.ClearAndMessageBattleLog(self, target, "[actor] attacks [target]!");
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
                GameManager.Instance.ClearAndMessageBattleLog(self.Name.ToUpper() + " reads a sad poem.");
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, self);
                string state = "sad";
                switch (target.CurrentState)
                {
                    case "miserable":
                        GameManager.Instance.MessageBattleLog(target.Name.ToUpper() + " cannot be any sadder!");
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
                    GameManager.Instance.MessageBattleLog(target.Name.ToUpper() + " cannot be any sadder!");
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
                GameManager.Instance.ClearAndMessageBattleLog(self, target, "[actor] lunges at [target]!");
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
                GameManager.Instance.ClearAndMessageBattleLog(self, target, "[actor] stabs [target].");
                if (self.CurrentState == "sad" || self.CurrentState == "depressed" || self.CurrentState == "miserable")
                    GameManager.Instance.BattleManager.Damage(self, target, () => { return self.CurrentStats.ATK * 2f; }, false, guaranteeCrit: true);
                else
                    GameManager.Instance.BattleManager.Damage(self, target, () => { return self.CurrentStats.ATK * 1.5f - target.CurrentStats.DEF; }, false, guaranteeCrit: true);
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
                GameManager.Instance.ClearAndMessageBattleLog(self, target, "[actor] attacks [target]!");
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
                GameManager.Instance.ClearAndMessageBattleLog(self, target, "[actor] calms down.");
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
                GameManager.Instance.ClearAndMessageBattleLog(self, target, "[actor] attacks [target]!");
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
                GameManager.Instance.ClearAndMessageBattleLog(self.Name.ToUpper() + " cheers on " + target.Name.ToUpper() + "!");
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId);
                string state = "happy";
                switch (target.CurrentState)
                {
                    case "manic":
                        GameManager.Instance.MessageBattleLog(target.Name.ToUpper() + " cannot be any happier!");
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
                    GameManager.Instance.MessageBattleLog(target.Name.ToUpper() + " cannot be any happier!");
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
                    GameManager.Instance.ClearAndMessageBattleLog(self, target, "[actor] does not have enough HP!");
                    // refund juice
                    self.CurrentJuice += skill.Cost;
                    return;
                }
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId);
                GameManager.Instance.ClearAndMessageBattleLog(self, target, "[actor] headbutts [target]!");
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
                GameManager.Instance.ClearAndMessageBattleLog(self, target, "[actor] attacks [target]!");
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
                GameManager.Instance.ClearAndMessageBattleLog(self, target, "[actor] hits a home run!");
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
                GameManager.Instance.ClearAndMessageBattleLog(self, target, "[actor] attacks [target]!");
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
                GameManager.Instance.ClearAndMessageBattleLog(self.Name.ToUpper() + " annoys " + target.Name.ToUpper() + "!");
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId);
                string state = "angry";
                switch (target.CurrentState)
                {
                    case "furious":
                        GameManager.Instance.MessageBattleLog(target.Name.ToUpper() + " cannot be any angrier!");
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
                    GameManager.Instance.MessageBattleLog(target.Name.ToUpper() + " cannot be any angrier!");
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
                GameManager.Instance.ClearAndMessageBattleLog(self, target, "[actor]'s ball bounces everywhere!");
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
                GameManager.Instance.ClearAndMessageBattleLog(self, target, "[actor] does a fancy ball trick!");
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId);
                GameManager.Instance.BattleManager.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false, 0.3f);
                await Task.Delay(1000);
                GameManager.Instance.BattleManager.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false, 0.3f);
                await Task.Delay(1000);
                GameManager.Instance.BattleManager.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false, 0.3f);
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
                GameManager.Instance.ClearAndMessageBattleLog(self, target, "[actor] attacks [target]!");
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
                GameManager.Instance.ClearAndMessageBattleLog(self, target, "[actor] gives some encouragement!");
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
                GameManager.Instance.ClearAndMessageBattleLog(self, target, "[actor] attacks [target]!");
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
                GameManager.Instance.ClearAndMessageBattleLog(self.Name.ToUpper() + " massages " + target.Name.ToUpper() + "!");
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId);
                target.SetState("neutral");
                GameManager.Instance.MessageBattleLog(target.Name.ToUpper() + " calms down...");
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
                GameManager.Instance.ClearAndMessageBattleLog(self, target, "[actor] makes a cookie just for [target]!");
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, target);
                float heal = target.CurrentStats.MaxHP * 0.75f;
                float variance = GameManager.Instance.Random.RandfRange(0.8f, 1.2f);
                int finalHeal = (int)Math.Round(heal * variance, MidpointRounding.AwayFromZero);
                target.Heal(finalHeal);
                GameManager.Instance.BattleManager.SpawnDamageNumber(finalHeal, target.CenterPoint, DamageType.Heal);
                GameManager.Instance.AnimationManager.PlayAnimation(212, target);
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
                GameManager.Instance.ClearAndMessageBattleLog(self, target, "[actor] makes a refreshment for [target].");
                GameManager.Instance.AnimationManager.PlayAnimation(skill.AnimationId, target);
                int heal = (int)Math.Round(target.CurrentStats.MaxJuice * 0.5f, MidpointRounding.AwayFromZero);
                target.HealJuice(heal);
                GameManager.Instance.BattleManager.SpawnDamageNumber(heal, target.CenterPoint, DamageType.JuiceGain);
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
                GameManager.Instance.ClearAndMessageBattleLog(self, target, "[actor] attacks [target]!");
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
                GameManager.Instance.ClearAndMessageBattleLog(self, target, "[actor] provides first aid!");
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, target);
                float heal = target.CurrentStats.MaxHP * 0.25f;
                float variance = GameManager.Instance.Random.RandfRange(0.8f, 1.2f);
                int finalHeal = (int)Math.Round(heal * variance, MidpointRounding.AwayFromZero);
                target.Heal(finalHeal);
                GameManager.Instance.BattleManager.SpawnDamageNumber(finalHeal, target.CenterPoint, DamageType.Heal);
                GameManager.Instance.AnimationManager.PlayAnimation(212, target);
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
                GameManager.Instance.ClearAndMessageBattleLog(self, target, "[actor] bumps into [target]!");
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
                GameManager.Instance.ClearAndMessageBattleLog(self, target, "[actor] is rolling around.");
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
                GameManager.Instance.ClearAndMessageBattleLog(self, target, "[actor] runs around!");
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
                GameManager.Instance.ClearAndMessageBattleLog(self, target, "[actor] nibbles at [target]?");
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
                GameManager.Instance.ClearAndMessageBattleLog(self, target, "[actor] is hopping around?");
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
                GameManager.Instance.ClearAndMessageBattleLog(self, target, "[actor] winks at [target]?");
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
                GameManager.Instance.ClearAndMessageBattleLog(self, target, "[actor] looks sadly at [target]?");
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, self);
                string state = "sad";
                switch (target.CurrentState)
                {
                    case "miserable":
                        GameManager.Instance.MessageBattleLog(target.Name.ToUpper() + " cannot be any sadder!");
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
                    GameManager.Instance.MessageBattleLog(target.Name.ToUpper() + " cannot be any sadder!");
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
                GameManager.Instance.ClearAndMessageBattleLog(self, target, "[actor] slaps [target].");
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
                GameManager.Instance.ClearAndMessageBattleLog(self, target, "[actor] insults everyone!");
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId);
                foreach (PartyMemberComponent member in GameManager.Instance.BattleManager.GetAlivePartyMembers()) {
                    GameManager.Instance.BattleManager.Damage(self, member.Actor, () => { return self.CurrentStats.ATK; }, false, 0.1f, neverCrit: true);
                    string state = "angry";
                    switch (member.Actor.CurrentState)
                    {
                        case "furious":
                            GameManager.Instance.MessageBattleLog(member.Actor.Name.ToUpper() + " cannot be any angrier!");
                            return;
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
                        GameManager.Instance.MessageBattleLog(member.Actor.Name.ToUpper() + " cannot be any angrier!");
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
                GameManager.Instance.ClearAndMessageBattleLog(self, target, "[actor] swings her mace!");
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
                GameManager.Instance.ClearAndMessageBattleLog(self, target, "[actor] boasts about one of her\nmany, many talents!");
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId);
                string state = "happy";
                switch (self.CurrentState)
                {
                    case "manic":
                        GameManager.Instance.MessageBattleLog(self.Name.ToUpper() + " cannot be any happier!");
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
                    GameManager.Instance.MessageBattleLog(self.Name.ToUpper() + " cannot be any happier!");
            }
        };
    }
}