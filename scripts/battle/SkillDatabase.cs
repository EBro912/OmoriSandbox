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