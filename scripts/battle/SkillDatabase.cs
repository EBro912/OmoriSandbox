using System;
using System.Collections.Generic;
using System.Linq;
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
            Animation = "o_attack",
            Effect = async (self, target, skill) =>
            {
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.Animation, target);
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
            Animation = "o_sad_story",
            Effect = async (self, target, skill) =>
            {
                GameManager.Instance.ClearAndMessageBattleLog(self.Name.ToUpper() + " reads a sad poem.");
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.Animation, self);
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
            Animation = "o_quick_attack",
            Effect = async (self, target, skill) =>
            {
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.Animation, target);
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
            Animation = "o_stab",
            Effect = async (self, target, skill) =>
            {
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.Animation, target);
                GameManager.Instance.ClearAndMessageBattleLog(self, target, "[actor] stabs [target].");
                if (self.CurrentState == "sad" || self.CurrentState == "depressed" || self.CurrentState == "miserable")
                    GameManager.Instance.BattleManager.Damage(self, target, () => { return self.CurrentStats.ATK * 2f; }, guaranteeCrit: true);
                else
                    GameManager.Instance.BattleManager.Damage(self, target, () => { return self.CurrentStats.ATK * 1.5f - target.CurrentStats.DEF; }, guaranteeCrit: true);
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
            Animation = "a_attack",
            Effect = async (self, target, skill) =>
            {
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.Animation, target);
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
            Animation = "a_peptalk",
            Effect = async (self, target, skill) =>
            {
                GameManager.Instance.ClearAndMessageBattleLog(self.Name.ToUpper() + " cheers on " + target.Name.ToUpper() + "!");
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.Animation);
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
            Animation = "a_headbutt_edit",
            Effect = async (self, target, skill) =>
            {
                double neededHp = Math.Floor(self.CurrentStats.MaxHP * 0.2);
                if (self.CurrentHP < neededHp)
                {
                    GameManager.Instance.ClearAndMessageBattleLog(self, target, "[actor] does not have enough HP!");
                    return;
                }
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.Animation, target);
                GameManager.Instance.ClearAndMessageBattleLog(self, target, "[actor] headbutts [target]!");
                if (self.CurrentState == "angry" || self.CurrentState == "enraged")
                    GameManager.Instance.BattleManager.Damage(self, target, () => { return self.CurrentStats.ATK * 3f - target.CurrentStats.DEF; }, false);
                else
                    GameManager.Instance.BattleManager.Damage(self, target, () => { return self.CurrentStats.ATK * 2.5f - target.CurrentStats.DEF; }, false);
                self.CurrentHP = (int)Math.Max(1f, self.CurrentHP - Math.Floor(self.CurrentStats.MaxHP * 0.2));
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
            Animation = "k_attack",
            Effect = async (self, target, skill) =>
            {
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.Animation, target);
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
            Animation = "k_annoy",
            Effect = async (self, target, skill) =>
            {
                GameManager.Instance.ClearAndMessageBattleLog(self.Name.ToUpper() + " annoys " + target.Name.ToUpper() + "!");
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.Animation);
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
            Animation = "k_rebound",
            Effect = async (self, target, skill) =>
            {
                GameManager.Instance.ClearAndMessageBattleLog(self, target, "[actor]'s ball bounces everywhere!");
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.Animation);
                foreach (Enemy enemy in GameManager.Instance.BattleManager.GetAllEnemies())
                    GameManager.Instance.BattleManager.Damage(self, enemy, () => { return self.CurrentStats.ATK * 2.5f - enemy.CurrentStats.DEF; }, false);
            }
        };
        Skills["Ricochet"] = new Skill
        {
            Name = "RICOCHET",
            Description = "Deals damage to a foe 3 times.\nCost : 30",
            Cost = 30,
            Hidden = false,
            GoesFirst = false,
            Target = SkillTarget.Enemy,
            Animation = "k_riccochet_big",
            Effect = async (self, target, skill) =>
            {
                GameManager.Instance.ClearAndMessageBattleLog(self, target, "[actor] does a fancy ball trick!");
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.Animation);
                GameManager.Instance.BattleManager.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false, 0.3f);
                await Task.Delay(1000);
                GameManager.Instance.BattleManager.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false, 0.3f);
                await Task.Delay(1000);
                GameManager.Instance.BattleManager.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false, 0.3f);
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
            Animation = "h_attack",
            Effect = async (self, target, skill) =>
            {
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.Animation, target);
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
            Animation = "h_massage",
            Effect = async (self, target, skill) =>
            {
                GameManager.Instance.ClearAndMessageBattleLog(self.Name.ToUpper() + " massages " + target.Name.ToUpper() + "!");
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.Animation);
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
            Animation = "h_snack_time",
            Effect = async (self, target, skill) =>
            {
                GameManager.Instance.ClearAndMessageBattleLog(self, target, "[actor] makes a cookie just for [target]!");
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.Animation, target);
                int heal = (int)Math.Round(target.CurrentStats.MaxHP * 0.75f, MidpointRounding.AwayFromZero);
                target.Heal(heal);
                GameManager.Instance.BattleManager.SpawnDamageNumber(heal, target.CenterPoint, DamageType.Heal);
                GameManager.Instance.AnimationManager.PlayAnimation("u_healheart", target);
                await Task.Delay(TimeSpan.FromSeconds(1d));
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
            Animation = "u_healjuice",
            Effect = async (self, target, skill) =>
            {
                GameManager.Instance.ClearAndMessageBattleLog(self, target, "[actor] makes a refreshment for [target].");
                GameManager.Instance.AnimationManager.PlayAnimation(skill.Animation, target);
                int heal = (int)Math.Round(target.CurrentStats.MaxJuice * 0.5f, MidpointRounding.AwayFromZero);
                target.HealJuice(heal);
                GameManager.Instance.BattleManager.SpawnDamageNumber(heal, target.CenterPoint, DamageType.JuiceGain);
                await Task.Delay(TimeSpan.FromSeconds(1d));
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
            Animation = "e_attacksolid",
            Effect = async (self, target, skill) =>
            {
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.Animation, target);
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
            Animation = string.Empty,
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
            Animation = "e_sproutmole_running",
            Effect = async (self, target, skill) =>
            {
                GameManager.Instance.AnimationManager.PlayAnimation(skill.Animation);
                GameManager.Instance.ClearAndMessageBattleLog(self, target, "[actor] runs around!");
                await Task.Delay(167);
                target = GameManager.Instance.BattleManager.GetRandomAlivePartyMember();
                GameManager.Instance.BattleManager.Damage(self, target, () => { return self.CurrentStats.ATK * 1.5f - target.CurrentStats.DEF; }, false);
                await Task.Delay(916);
                target = GameManager.Instance.BattleManager.GetRandomAlivePartyMember();
                GameManager.Instance.BattleManager.Damage(self, target, () => { return self.CurrentStats.ATK * 1.5f - target.CurrentStats.DEF; }, false);
            }
        };
    }
}