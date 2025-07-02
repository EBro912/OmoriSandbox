using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class Database
{
    private static readonly Dictionary<string, Skill> Skills = [];
    private static readonly Dictionary<string, Item> Items = [];
    private static readonly Dictionary<string, Weapon> Weapons = [];

    public static bool TryGetSkill(string name, out Skill skill)
    {
        return Skills.TryGetValue(name, out skill);
    }

    public static bool TryGetItem(string name, out Item item)
    {
        return Items.TryGetValue(name, out item);
    }

    public static bool TryGetWeapon(string name, out Weapon weapon)
    {
        return Weapons.TryGetValue(name, out weapon);
    }

    public static void Init()
    {
        #region SKILLS
        Skills["Guard"] = new Skill
        {
            Name = "GUARD",
            Description = "Acts first, reducing damage taken for 1 turn.\nCost: 0",
            Hidden = false,
            GoesFirst = true,
            Target = SkillTarget.Self,
            AnimationId = 115,
            Effect = async (self, target, skill) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] guards.");
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, self, false);
                self.AddStatModifier(Modifier.Guard, 1, 1);
            }
        };

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
                await Task.Delay(1000);
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false);
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
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, self, false);
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
                    BattleManager.Instance.Damage(self, target, () => { return (self.CurrentStats.ATK + self.CurrentStats.LCK) * 2f - target.CurrentStats.DEF; }, false);
                else
                    BattleManager.Instance.Damage(self, target, () => { return (self.CurrentStats.ATK + self.CurrentStats.LCK) * 1.5f - target.CurrentStats.DEF; }, false);
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
                    BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2f; }, false, guaranteeCrit: true);
                else
                    BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 1.5f - target.CurrentStats.DEF; }, false, guaranteeCrit: true);
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
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 3f - target.CurrentStats.DEF; }, false);
                await Task.Delay(334);
            }
        };

        Skills["HackAway"] = new Skill
        {
            Name = "HACK AWAY",
            Description = "Attacks 3 times, hitting random foes.\nCost: 30",
            Cost = 30,
            Hidden = false,
            GoesFirst = false,
            Target = SkillTarget.AllEnemies,
            AnimationId = 6,
            Effect = async (self, target, skill) =>
            {
                await GameManager.Instance.AnimationManager.WaitForScreenAnimation(skill.AnimationId, true);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] slashes wildly!");
                List<Enemy> allEnemies = BattleManager.Instance.GetAllEnemies();
                List<Enemy> targets = [];
                for (int i = 0; i < 3; i++)
                {
                    targets.Add(allEnemies[GameManager.Instance.Random.RandiRange(0, allEnemies.Count - 1)]);
                }
                foreach (Enemy enemy in allEnemies)
                {
                    BattleManager.Instance.Damage(self, target, () =>
                    {
                        if (self.CurrentState == "angry" || self.CurrentState == "enraged" || self.CurrentState == "furious")
                        {
                            return self.CurrentStats.ATK * 2.25f - target.CurrentStats.DEF;
                        }
                        return self.CurrentStats.ATK * 2f - target.CurrentStats.DEF;
                    }, false);
                }
            }
        };

        Skills["PainfulTruth"] = new Skill
        {
            Name = "PAINFUL TRUTH",
            Description = "Deals damage to a foe. OMORI and the foe\nbecome SAD. Cost: 10",
            Cost = 10,
            Hidden = false,
            GoesFirst = false,
            Target = SkillTarget.Enemy,
            AnimationId = 5,
            Effect = async (self, target, skill) =>
            {
                GameManager.Instance.AnimationManager.PlayAnimation(skill.AnimationId, self, false);
                GameManager.Instance.AnimationManager.PlayAnimation(19, target);

                string state = "sad";
                switch (self.CurrentState)
                {
                    case "miserable":
                        BattleLogManager.Instance.QueueMessage(self, target, "[actor] cannot be any sadder!");
                        return;
                    case "depressed":
                        state = "miserable";
                        break;
                    case "sad":
                        state = "depressed";
                        break;
                }
                if (self.IsStateValid(state))
                    self.SetState(state);
                else
                    BattleLogManager.Instance.QueueMessage(self, target, "[actor] cannot be any sadder!");

                state = "sad";
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

                await Task.Delay(1000);

                BattleLogManager.Instance.QueueMessage(self, target, "[actor] whispers something\nto [target].");
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false);
            }
        };

        Skills["Shun"] = new Skill
        {
            Name = "SHUN",
            Description = "Deals damage. If the foe is SAD, greatly\nreduce it's DEFENSE. Cost: 20",
            Cost = 20,
            Hidden = false,
            GoesFirst = false,
            Target = SkillTarget.Enemy,
            AnimationId = 12,
            Effect = async (self, target, skill) =>
            {
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] mocks [target].");
                if (target.CurrentState == "angry" || target.CurrentState == "enraged" || target.CurrentState == "furious")
                {
                    GameManager.Instance.AnimationManager.PlayAnimation(219, target);
                    target.AddStatModifier(Modifier.AttackDown, 3);
                }
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 3f - target.CurrentStats.DEF; }, false);
                await Task.Delay(334);
            }
        };

        Skills["Mock"] = new Skill
        {
            Name = "MOCK",
            Description = "Deals damage. If the foe is ANGRY, greatly\nreduce it's ATTACK. Cost: 20",
            Cost = 20,
            Hidden = false,
            GoesFirst = false,
            Target = SkillTarget.Enemy,
            AnimationId = 11,
            Effect = async (self, target, skill) =>
            {
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] shuns [target].");
                if (target.CurrentState == "sad" || target.CurrentState == "depressed" || target.CurrentState == "miserable")
                {
                    GameManager.Instance.AnimationManager.PlayAnimation(219, target);
                    target.AddStatModifier(Modifier.DefenseDown, 3);
                }
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 3f - target.CurrentStats.DEF; }, false);
                await Task.Delay(334);
            }
        };

        Skills["Stare"] = new Skill
        {
            Name = "STARE",
            Description = "Reduces all of a foe's STATS.\nCost: 45",
            Cost = 45,
            Hidden = false,
            GoesFirst = false,
            Target = SkillTarget.Enemy,
            AnimationId = 18,
            Effect = async (self, target, skill) =>
            {
                GameManager.Instance.AnimationManager.PlayAnimation(skill.AnimationId, target);
                await Task.Delay(1660);
                GameManager.Instance.AnimationManager.PlayAnimation(219, target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] stares at [target].");
                BattleLogManager.Instance.QueueMessage(self, target, "[target] feels uncomfortable.");
                target.AddStatModifier(Modifier.AttackDown, 1);
                target.AddStatModifier(Modifier.DefenseDown, 1);
                target.AddStatModifier(Modifier.SpeedDown, 1);
                await Task.Delay(334);
            }
        };

        Skills["Exploit"] = new Skill
        {
            Name = "EXPLOIT",
            Description = "Deals extra damage to a HAPPY, SAD, or\nANGRY foe. Cost: 30",
            Cost = 30,
            Hidden = false,
            GoesFirst = false,
            Target = SkillTarget.Enemy,
            AnimationId = -1,
            Effect = async (self, target, skill) =>
            {
                if (target.CurrentState == "happy" || target.CurrentState == "ecstatic" || target.CurrentState == "manic")
                {
                    await GameManager.Instance.AnimationManager.WaitForAnimation(10, target);
                }
                else if (target.CurrentState == "sad" || target.CurrentState == "depressed" || target.CurrentState == "miserable")
                {
                    await GameManager.Instance.AnimationManager.WaitForAnimation(11, target);
                }
                else if (target.CurrentState == "angry" || target.CurrentState == "enraged" || target.CurrentState == "furious")
                {
                    await GameManager.Instance.AnimationManager.WaitForAnimation(12, target);
                }
                else
                {
                    await GameManager.Instance.AnimationManager.WaitForAnimation(123, target);
                }
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] exploits [target]'s EMOTIONS!");
                if (target.CurrentState != "neutral")
                {
                    BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 3.5f - target.CurrentStats.DEF; }, false);
                }
                else
                {
                    BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2.5f - target.CurrentStats.DEF; }, false);
                }
            }
        };

        Skills["FinalStrike"] = new Skill
        {
            Name = "FINAL STRIKE",
            Description = "Strikes all foes. Deals more damage if OMORI\nhas a higher stage of EMOTION. Cost: 50",
            Cost = 50,
            Hidden = false,
            GoesFirst = false,
            Target = SkillTarget.AllEnemies,
            AnimationId = 13,
            Effect = async (self, target, skill) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] releases his ultimate\nattack!");
                await GameManager.Instance.AnimationManager.WaitForScreenAnimation(skill.AnimationId, true);
                float multiplier = 3f;
                if (self.CurrentState == "manic" || self.CurrentState == "miserable" || self.CurrentState == "furious")
                    multiplier = 6f;
                else if (self.CurrentState == "ecstatic" || self.CurrentState == "depressed" || self.CurrentState == "enraged")
                    multiplier = 5f;
                else if (self.CurrentState == "happy" || self.CurrentState == "sad" || self.CurrentState == "angry")
                    multiplier = 5f;
                foreach (Enemy enemy in BattleManager.Instance.GetAllEnemies())
                {
                    BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * multiplier - target.CurrentStats.DEF; }, false);
                }
            }
        };

        Skills["RedHands"] = new Skill
        {
            Name = "RED HANDS",
            Description = "Deals big damage 4 times.\nCost: 75",
            Cost = 75,
            Hidden = false,
            GoesFirst = false,
            Target = SkillTarget.Enemy,
            AnimationId = -1,
            Effect = async (self, target, skill) =>
            {
                await GameManager.Instance.AnimationManager.WaitForRedHands();
                for (int i = 0; i < 4; i++)
                {
                    BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 3f - target.CurrentStats.DEF; }, false);
                }
            }
        };

        // TODO: special skills (vertigo, cripple, suffocate), these require a special animation

        Skills["AttackAgain1"] = new Skill
        {
            Name = "Attack Again 1",
            Description = "Omori Followup",
            Cost = 0,
            Hidden = true,
            GoesFirst = false,
            Target = SkillTarget.Enemy,
            AnimationId = 3,
            Effect = async (self, target, skill) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] readies his blade.");
                await Task.Delay(1000);
                BattleLogManager.Instance.ClearBattleLog();
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks again!");
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false);
            }
        };

        Skills["Trip1"] = new Skill
        {
            Name = "Trip 1",
            Description = "Omori Followup",
            Cost = 0,
            Hidden = true,
            GoesFirst = false,
            Target = SkillTarget.Enemy,
            AnimationId = 14,
            Effect = async (self, target, skill) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] walks forward.");
                await Task.Delay(1000);
                BattleLogManager.Instance.ClearBattleLog();
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, target);
                GameManager.Instance.AnimationManager.PlayAnimation(219, target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] trips [target]!");
                target.AddStatModifier(Modifier.SpeedDown, 1);
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK + self.CurrentStats.LCK - target.CurrentStats.DEF; }, false);
            }
        };

        Skills["ReleaseEnergy1"] = new Skill
        {
            Name = "Release Energy 1",
            Description = "Omori Followup",
            Cost = 0,
            Hidden = true,
            GoesFirst = false,
            Target = SkillTarget.AllEnemies,
            AnimationId = 15,
            Effect = async (self, target, skill) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] and friends come together and\nuse their ultimate attack!");
                foreach (PartyMemberComponent member in BattleManager.Instance.GetAlivePartyMembers())
                {
                    GameManager.Instance.AnimationManager.PlayAnimation(243, member.Actor, false);
                }
                await GameManager.Instance.AnimationManager.WaitForReleaseEnergy();
                BattleLogManager.Instance.ClearBattleLog();
                await GameManager.Instance.AnimationManager.WaitForScreenAnimation(15, true);
                foreach (Enemy enemy in BattleManager.Instance.GetAllEnemies())
                {
                    BattleManager.Instance.Damage(self, enemy, () => { return 300; }, true, 0f, false, true);
                }
                foreach (PartyMemberComponent member in BattleManager.Instance.GetAlivePartyMembers())
                {
                    member.Actor.AddStatModifier(Modifier.ReleaseEnergy, 1, 9999);
                }
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
                await Task.Delay(1000);
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false, neverCrit: true);
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
                GameManager.Instance.AnimationManager.PlayScreenAnimation(skill.AnimationId, false);
                await Task.Delay(2500);
                self.Heal((int)Math.Round(self.BaseStats.MaxHP * 0.5, MidpointRounding.AwayFromZero));
                self.SetState("neutral", true);
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
                await Task.Delay(1000);
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false);
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
                await GameManager.Instance.AnimationManager.WaitForScreenAnimation(skill.AnimationId, false);
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
                await GameManager.Instance.AnimationManager.WaitForScreenAnimation(skill.AnimationId, true);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] headbutts [target]!");
                if (self.CurrentState == "angry" || self.CurrentState == "enraged")
                    BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 3f - target.CurrentStats.DEF; }, false);
                else
                    BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2.5f - target.CurrentStats.DEF; }, false);
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
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] smashes [target]!");
                target.AddStatModifier(Modifier.DefenseDown);
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2f; }, false);
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
                bool miss = BattleManager.Instance.Damage(self, target, () => { return (self.CurrentStats.ATK * 2f + self.CurrentStats.LCK) - target.CurrentStats.DEF; }, false);
                if (!miss)
                {
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

            }
        };

        Skills["MoodWrecker"] = new Skill
        {
            Name = "MOOD WRECKER",
            Description = "A swing that doesn't miss. Deals extra damage to\nHAPPY foes. Cost: 10",
            Cost = 10,
            Hidden = false,
            GoesFirst = false,
            Target = SkillTarget.Enemy,
            AnimationId = 46,
            Effect = async (self, target, skill) =>
            {
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, target, true);
                await Task.Delay(500);
                if (target.CurrentState == "happy" || target.CurrentState == "ecstatic" || target.CurrentState == "manic")
                {
                    // very nice
                    if (target.CurrentState == "ecstatic" || target.CurrentState == "manic")
                        await GameManager.Instance.AnimationManager.WaitForAnimation(279, target, true);
                    else
                        await GameManager.Instance.AnimationManager.WaitForAnimation(278, target, true);
                    BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
                    BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 3f - target.CurrentStats.DEF; }, true);
                }
                else
                {
                    BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
                    BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2.25f - target.CurrentStats.DEF; }, true);
                }
            }
        };

        // TODO: add support for skills that use the <Not User> tag
        Skills["TeamSpirit"] = new Skill
        {
            Name = "TEAM SPIRIT",
            Description = "Makes AUBREY and a friend HAPPY.\nCost: 10",
            Cost = 10,
            Hidden = false,
            GoesFirst = false,
            Target = SkillTarget.Ally,
            AnimationId = 49,
            Effect = async (self, target, skill) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] cheers on [target]!");
                GameManager.Instance.AnimationManager.PlayAnimation(skill.AnimationId, self, false);
                await Task.Delay(500);
                GameManager.Instance.AnimationManager.PlayScreenAnimation(29, false);
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

                state = "happy";
                switch (self.CurrentState)
                {
                    case "manic":
                        BattleLogManager.Instance.QueueMessage(self, target, "[actor] cannot be any happier!");
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
                    BattleLogManager.Instance.QueueMessage(self, target, "[actor] cannot be any happier!");
            }
        };

        Skills["WindUpThrow"] = new Skill
        {
            Name = "WIND-UP THROW",
            Description = "Damages all foes. Deals more damage the less\nenemies there are. Cost: 20",
            Cost = 20,
            Hidden = false,
            GoesFirst = false,
            Target = SkillTarget.AllEnemies,
            AnimationId = 33,
            Effect = async (self, target, skill) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] throws her weapon!");
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, target);
                int enemies = BattleManager.Instance.GetAllEnemies().Count;
                if (enemies == 1)
                    BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 3f - target.CurrentStats.DEF; }, false);
                else if (enemies == 2)
                    BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2.5f - target.CurrentStats.DEF; }, false);
                else
                    BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2f - target.CurrentStats.DEF; }, false);
            }
        };

        Skills["Mash"] = new Skill
        {
            Name = "MASH",
            Description = "If this skill defeats a foe, recover 100% JUICE.\nCost: 15",
            Cost = 15,
            Hidden = false,
            GoesFirst = false,
            Target = SkillTarget.Enemy,
            AnimationId = 28,
            Effect = async (self, target, skill) =>
            {
                GameManager.Instance.AnimationManager.PlayAnimation(skill.AnimationId, target);
                await Task.Delay(500);
                GameManager.Instance.AnimationManager.PlayAnimation(213, target);
                await Task.Delay(500);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2.5f - target.CurrentStats.DEF; }, false);
                if (target.CurrentHP == 0)
                {
                    GameManager.Instance.AnimationManager.PlayAnimation(213, self);
                    self.HealJuice(self.CurrentStats.MaxJuice);
                    BattleManager.Instance.SpawnDamageNumber(self.CurrentStats.MaxJuice, target.CenterPoint, DamageType.JuiceGain);
                }
            }
        };

        Skills["Beatdown"] = new Skill
        {
            Name = "BEATDOWN",
            Description = "Attacks a foe 3 times.\nCost: 30",
            Cost = 30,
            Hidden = false,
            GoesFirst = false,
            Target = SkillTarget.Enemy,
            AnimationId = 17,
            Effect = async (self, target, skill) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] furiously attacks!");
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, target);
                for (int i = 0; i < 3; i++)
                {
                    BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2f - target.CurrentStats.DEF; }, false);
                    await Task.Delay(1000);
                }
            }
        };

        Skills["LastResort"] = new Skill
        {
            Name = "LAST RESORT",
            Description = "Deals damage based on AUBREY's HEART,\nbut AUBREY becomes TOAST. Cost: 50",
            Cost = 50,
            Hidden = false,
            GoesFirst = false,
            Target = SkillTarget.Enemy,
            AnimationId = 34,
            Effect = async (self, target, skill) =>
            {
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] strikes [target]\nwith all her strength!");
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentHP * 4f; }, false);
                self.Damage(self.CurrentHP);
            }
        };

        Skills["LookAtOmori1"] = new Skill
        {
            Name = "Look At Omori 1",
            Description = "Aubrey Followup",
            Cost = 0,
            Hidden = true,
            GoesFirst = false,
            Target = SkillTarget.Enemy,
            AnimationId = 35,
            Effect = async (self, target, skill) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] looks at OMORI.");
                await Task.Delay(1000);
                await GameManager.Instance.AnimationManager.WaitForScreenAnimation(skill.AnimationId, false);
                await GameManager.Instance.AnimationManager.WaitForAnimation(28, target);
                BattleLogManager.Instance.QueueMessage(self, target, "OMORI didn't notice AUBREY, so\nAUBREY attacks again!");
                BattleManager.Instance.Damage(self, target, () => { return (self.CurrentStats.ATK * 2 + self.CurrentStats.LCK) - target.CurrentStats.DEF; }, false);
            }
        };

        Skills["LookAtKel1"] = new Skill
        {
            Name = "Look At Kel 1",
            Description = "Aubrey Followup",
            Cost = 0,
            Hidden = true,
            GoesFirst = false,
            Target = SkillTarget.Self,
            AnimationId = 38,
            Effect = async (self, target, skill) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] looks at KEL.");
                await Task.Delay(1000);
                GameManager.Instance.AnimationManager.PlayScreenAnimation(skill.AnimationId, false);
                await Task.Delay(2000);
                BattleLogManager.Instance.QueueMessage(self, target, "KEL eggs [actor] on!");
                string state = "angry";
                switch (self.CurrentState)
                {
                    case "furious":
                        BattleLogManager.Instance.QueueMessage(self, target, "[actor] cannot be any angrier!");
                        return;
                    case "enraged":
                        state = "furious";
                        break;
                    case "angry":
                        state = "enraged";
                        break;
                }
                if (self.IsStateValid(state))
                    self.SetState(state);
                else
                    BattleLogManager.Instance.QueueMessage(self, target, "[actor] cannot be any angrier!");
            }
        };

        Skills["LookAtHero1"] = new Skill
        {
            Name = "Look At Hero 1",
            Description = "Aubrey Followup",
            Cost = 0,
            Hidden = true,
            GoesFirst = false,
            Target = SkillTarget.Self,
            AnimationId = 41,
            Effect = async (self, target, skill) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] looks at HERO.");
                await Task.Delay(1000);
                GameManager.Instance.AnimationManager.PlayScreenAnimation(skill.AnimationId, false);
                await Task.Delay(2000);
                GameManager.Instance.AnimationManager.PlayAnimation(214, self, false);
                await Task.Delay(1000);
                BattleLogManager.Instance.QueueMessage(self, target, "HERO tells [actor] to focus!");
                self.AddStatModifier(Modifier.DefenseUp);
                string state = "happy";
                switch (self.CurrentState)
                {
                    case "manic":
                        BattleLogManager.Instance.QueueMessage(self.Name.ToUpper() + " cannot be any happier!");
                        break;
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
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false, neverCrit: true);
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
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 4f - target.CurrentStats.DEF; }, neverCrit: true);
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
                await Task.Delay(1000);
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false);
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
                await GameManager.Instance.AnimationManager.WaitForScreenAnimation(skill.AnimationId, false);
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
                await GameManager.Instance.AnimationManager.WaitForScreenAnimation(skill.AnimationId, true);
                foreach (Enemy enemy in BattleManager.Instance.GetAllEnemies())
                    BattleManager.Instance.Damage(self, enemy, () => { return self.CurrentStats.ATK * 2.5f - enemy.CurrentStats.DEF; }, false);
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
                await GameManager.Instance.AnimationManager.WaitForScreenAnimation(skill.AnimationId, true);
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false, 0.3f);
                await Task.Delay(1000);
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false, 0.3f);
                await Task.Delay(1000);
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false, 0.3f);
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
                GameManager.Instance.AnimationManager.PlayScreenAnimation(skill.AnimationId, true);
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
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false, neverCrit: true);
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
                GameManager.Instance.AnimationManager.PlayAnimation(skill.AnimationId, target, false);
                await Task.Delay(1000);
                target.AddStatModifier(Modifier.AttackUp);
            }
        };
        Skills["PassToOmori1"] = new Skill
        {
            Name = "Pass To Omori 1",
            Description = "Kel Followup",
            Cost = 0,
            Hidden = true,
            GoesFirst = false,
            Target = SkillTarget.Ally,
            AnimationId = 62,
            Effect = async (self, target, skill) =>
            {
                PartyMember first = BattleManager.Instance.GetPartyMember(0);
                BattleLogManager.Instance.QueueMessage(self, first, "[actor] passes to [target].");
                await Task.Delay(1000);
                GameManager.Instance.AnimationManager.PlayScreenAnimation(skill.AnimationId, false);
                await Task.Delay(1000);
                BattleLogManager.Instance.QueueMessage(self, first, "[target] wasn't looking and gets bopped!");
                BattleManager.Instance.Damage(self, first, () => { return 1; }, true, 0f, false, true);
                first.SetState("sad");
            }
        };
        Skills["PassToAubrey1"] = new Skill
        {
            Name = "Pass To Aubrey 1",
            Description = "Kel Followup",
            Cost = 0,
            Hidden = true,
            GoesFirst = false,
            Target = SkillTarget.Enemy,
            AnimationId = 65,
            Effect = async (self, target, skill) =>
            {
                target = BattleManager.Instance.GetRandomAliveEnemy();
                PartyMember second = BattleManager.Instance.GetPartyMember(1);
                BattleLogManager.Instance.QueueMessage(self, second, "[actor] passes to [target].");
                await Task.Delay(1000);
                GameManager.Instance.AnimationManager.PlayScreenAnimation(skill.AnimationId, true);
                await Task.Delay(2000);
                await GameManager.Instance.AnimationManager.WaitForAnimation(66, target);
                BattleLogManager.Instance.QueueMessage(self, second, "[target] knocks the ball out of the park!");
                BattleManager.Instance.Damage(self, target, () => { return second.CurrentStats.ATK + self.CurrentStats.ATK - target.CurrentStats.DEF; }, true);
            }
        };
        Skills["PassToHero1"] = new Skill
        {
            Name = "Pass To Hero 1",
            Description = "Kel Followup",
            Cost = 0,
            Hidden = true,
            GoesFirst = false,
            Target = SkillTarget.AllEnemies,
            AnimationId = 69,
            Effect = async (self, target, skill) =>
            {
                PartyMember second = BattleManager.Instance.GetPartyMember(1);
                PartyMember third = BattleManager.Instance.GetPartyMember(2);
                BattleLogManager.Instance.QueueMessage(self, third, "[actor] passes to [target].");
                await Task.Delay(1000);
                await GameManager.Instance.AnimationManager.WaitForScreenAnimation(skill.AnimationId, true);
                BattleLogManager.Instance.QueueMessage(self, third, "[target] dunks on the foes!");
                foreach (Enemy enemy in BattleManager.Instance.GetAllEnemies())
                {
                    // VANILLA BUG: uses Aubrey's attack instead of Hero's
                    BattleManager.Instance.Damage(self, target, () => { return second.CurrentStats.ATK + self.CurrentStats.ATK - target.CurrentStats.DEF; }, true);
                }
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
                await Task.Delay(1000);
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false);
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
                await GameManager.Instance.AnimationManager.WaitForScreenAnimation(skill.AnimationId, false);
                target.SetState("neutral", true);
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
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, target, false);
                BattleManager.Instance.Heal(self, target, () => { return target.CurrentStats.MaxHP * 0.75f; });
                GameManager.Instance.AnimationManager.PlayAnimation(212, target, false);
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
                GameManager.Instance.AnimationManager.PlayAnimation(skill.AnimationId, target, false);
                BattleManager.Instance.HealJuice(self, target, () => { return target.CurrentStats.MaxJuice * 0.5f; });
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
                    target = BattleManager.Instance.GetRandomDeadPartyMember();
                    if (target == null)
                    {
                        BattleLogManager.Instance.QueueMessage("It had no effect.");
                        return;
                    }
                }
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, target, false);
                target.SetState("neutral", true);
                int heal = (int)Math.Round(target.CurrentStats.MaxHP * 0.7f, MidpointRounding.AwayFromZero);
                target.Heal(heal);
                BattleManager.Instance.SpawnDamageNumber(heal, target.CenterPoint, DamageType.Heal);
                BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovered {heal} HEART!");
                BattleLogManager.Instance.QueueMessage(self, target, "[target] rose again!");
                await Task.Delay(1000);
            }
        };

        Skills["CallOmori1"] = new Skill
        {
            Name = "Call Omori 1",
            Description = "Hero Followup",
            Cost = 0,
            Hidden = true,
            GoesFirst = false,
            Target = SkillTarget.Ally,
            AnimationId = 93,
            Effect = async (self, target, skill) =>
            {
                PartyMember first = BattleManager.Instance.GetPartyMember(0);
                BattleLogManager.Instance.QueueMessage(self, first, "[actor] calls out to [target].");
                await Task.Delay(1000);
                await GameManager.Instance.AnimationManager.WaitForScreenAnimation(skill.AnimationId, false);
                await GameManager.Instance.AnimationManager.WaitForAnimation(212, first, false);
                int heal = (int)Math.Round(first.CurrentStats.MaxHP * 0.15f, MidpointRounding.AwayFromZero);
                first.Heal(heal);
                BattleLogManager.Instance.QueueMessage(self, first, "[actor] signals to [target]!");
                BattleLogManager.Instance.QueueMessage(self, first, $"[target] recovers {heal} HEART!");
                BattleManager.Instance.ForceCommand(first, BattleManager.Instance.GetRandomAliveEnemy(), Skills["OAttack"]);
            }
        };

        Skills["CallAubrey1"] = new Skill
        {
            Name = "Call Aubrey 1",
            Description = "Hero Followup",
            Cost = 0,
            Hidden = true,
            GoesFirst = false,
            Target = SkillTarget.Ally,
            AnimationId = 94,
            Effect = async (self, target, skill) =>
            {
                PartyMember second = BattleManager.Instance.GetPartyMember(1);
                BattleLogManager.Instance.QueueMessage(self, second, "[actor] calls out to [target].");
                await Task.Delay(1000);
                await GameManager.Instance.AnimationManager.WaitForScreenAnimation(skill.AnimationId, false);
                await GameManager.Instance.AnimationManager.WaitForAnimation(212, second, false);
                int heal = (int)Math.Round(second.CurrentStats.MaxHP * 0.15f, MidpointRounding.AwayFromZero);
                second.Heal(heal);
                BattleLogManager.Instance.QueueMessage(self, second, "[actor] signals to [target]!");
                BattleLogManager.Instance.QueueMessage(self, second, $"[target] recovers {heal} HEART!");
                BattleManager.Instance.ForceCommand(second, BattleManager.Instance.GetRandomAliveEnemy(), Skills["AAttack"]);
            }
        };

        Skills["CallKel1"] = new Skill
        {
            Name = "Call Kel 1",
            Description = "Hero Followup",
            Cost = 0,
            Hidden = true,
            GoesFirst = false,
            Target = SkillTarget.Ally,
            AnimationId = 95,
            Effect = async (self, target, skill) =>
            {
                PartyMember fourth = BattleManager.Instance.GetPartyMember(3);
                BattleLogManager.Instance.QueueMessage(self, fourth, "[actor] calls out to [target].");
                await Task.Delay(1000);
                await GameManager.Instance.AnimationManager.WaitForScreenAnimation(skill.AnimationId, false);
                await GameManager.Instance.AnimationManager.WaitForAnimation(212, fourth, false);
                int heal = (int)Math.Round(fourth.CurrentStats.MaxHP * 0.15f, MidpointRounding.AwayFromZero);
                fourth.Heal(heal);
                BattleLogManager.Instance.QueueMessage(self, fourth, "[actor] signals to [target]!");
                BattleLogManager.Instance.QueueMessage(self, fourth, $"[target] recovers {heal} HEART!");
                BattleManager.Instance.ForceCommand(fourth, BattleManager.Instance.GetRandomAliveEnemy(), Skills["KAttack"]);
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
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false, neverCrit: true);
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
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, target, false);
                float heal = target.CurrentStats.MaxHP * 0.25f;
                float variance = GameManager.Instance.Random.RandfRange(0.8f, 1.2f);
                int finalHeal = (int)Math.Round(heal * variance, MidpointRounding.AwayFromZero);
                target.Heal(finalHeal);
                BattleManager.Instance.SpawnDamageNumber(finalHeal, target.CenterPoint, DamageType.Heal);
                GameManager.Instance.AnimationManager.PlayAnimation(212, target, false);
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
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, target, false);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] bumps into [target]!");
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false);
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
                GameManager.Instance.AnimationManager.PlayScreenAnimation(skill.AnimationId, false);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] runs around!");
                await Task.Delay(100);
                target = BattleManager.Instance.GetRandomAlivePartyMember();
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 1.5f - target.CurrentStats.DEF; }, false);
                await Task.Delay(917);
                target = BattleManager.Instance.GetRandomAlivePartyMember();
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 1.5f - target.CurrentStats.DEF; }, false);
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
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, target, false);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] nibbles at [target]?");
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false);
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
                await GameManager.Instance.AnimationManager.WaitForAnimation(215, target, false);
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
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, target, false);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] slaps [target].");
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false);
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
                await GameManager.Instance.AnimationManager.WaitForScreenAnimation(skill.AnimationId, false);
                foreach (PartyMemberComponent member in BattleManager.Instance.GetAlivePartyMembers()) {
                    BattleManager.Instance.Damage(self, member.Actor, () => { return self.CurrentStats.ATK; }, false, 0.1f, neverCrit: true);
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
                await GameManager.Instance.AnimationManager.WaitForScreenAnimation(skill.AnimationId, false);
                foreach (PartyMemberComponent member in BattleManager.Instance.GetAlivePartyMembers())
                {
                    BattleManager.Instance.Damage(self, member.Actor, () => { return self.CurrentStats.ATK * 2.5f - member.Actor.CurrentStats.DEF; }, false);
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
                await GameManager.Instance.AnimationManager.WaitForScreenAnimation(skill.AnimationId, false);
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

        // SLIME GIRLS //
        Skills["ComboAttack"] = new Skill
        {
            Name = "ComboAttack",
            Description = "ComboAttack",
            Cost = 0,
            Hidden = true,
            GoesFirst = false,
            Target = SkillTarget.Enemy,
            Effect = async (self, target, skill) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "The [actor] attack all at once!");
                GameManager.Instance.AnimationManager.PlayAnimation(133, target, false);
                await Task.Delay(580);
                GameManager.Instance.AnimationManager.PlayAnimation(134, target, false);
                await Task.Delay(580);
                GameManager.Instance.AnimationManager.PlayAnimation(135, target, false);
                await Task.Delay(580);
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2f - target.CurrentStats.DEF; }, false);
            }
        };

        Skills["StrangeGas"] = new Skill
        {
            Name = "StrangeGas",
            Description = "StrangeGas",
            Cost = 0,
            Hidden = true,
            GoesFirst = false,
            Target = SkillTarget.AllEnemies,
            Effect = async (self, target, skill) =>
            {
                BattleLogManager.Instance.QueueMessage("MEDUSA threw a bottle...");
                GameManager.Instance.AnimationManager.PlayScreenAnimation(194, false);
                await Task.Delay(1500);
                GameManager.Instance.AnimationManager.PlayScreenAnimation(181, false);
                BattleLogManager.Instance.QueueMessage("A strange gas fills the room.");
                await Task.Delay(2000);

                foreach (PartyMemberComponent member in BattleManager.Instance.GetAlivePartyMembers())
                {
                    int roll = GameManager.Instance.Random.RandiRange(0, 2);
                    string state = "";
                    switch (roll)
                    {
                        case 0:
                            state = "sad";
                            switch (member.Actor.CurrentState)
                            {
                                case "miserable":
                                    continue;
                                case "depressed":
                                    state = "miserable";
                                    break;
                                case "sad":
                                    state = "depressed";
                                    break;
                            }
                            break;
                        case 1:
                            state = "angry";
                            switch (member.Actor.CurrentState)
                            {
                                case "furious":
                                    continue;
                                case "enraged":
                                    state = "furious";
                                    break;
                                case "angry":
                                    state = "enraged";
                                    break;
                            }
                            break;
                        case 2:
                            state = "happy";
                            switch (member.Actor.CurrentState)
                            {
                                case "manic":
                                    continue;
                                case "ecstatic":
                                    state = "manic";
                                    break;
                                case "happy":
                                    state = "ecstatic";
                                    break;
                            }
                            break;
                    }
                    if (member.Actor.IsStateValid(state))
                        member.Actor.SetState(state);
                }
            }
        };

        Skills["Dynamite"] = new Skill
        {
            Name = "Dynamite",
            Description = "Dynamite",
            Cost = 0,
            Hidden = true,
            GoesFirst = false,
            Target = SkillTarget.AllEnemies,
            Effect = async (self, target, skill) =>
            {
                BattleLogManager.Instance.QueueMessage("MEDUSA threw a bottle...");
                GameManager.Instance.AnimationManager.PlayScreenAnimation(194, false);
                await Task.Delay(1500);
                GameManager.Instance.AnimationManager.PlayScreenAnimation(172, false);
                BattleLogManager.Instance.QueueMessage("And it explodes!");
                await Task.Delay(2000);

                foreach (PartyMemberComponent member in BattleManager.Instance.GetAlivePartyMembers())
                {
                    BattleManager.Instance.Damage(self, member.Actor, () => { return 75; }, false, 0f, false, true);
                }
            }
        };

        Skills["StingRay"] = new Skill
        {
            Name = "StingRay",
            Description = "StingRay",
            Cost = 0,
            Hidden = true,
            GoesFirst = false,
            Target = SkillTarget.Enemy,
            AnimationId = 193,
            Effect = async (self, target, skill) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "MOLLY fires her stingers!\n[target] gets struck!");
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, target, false);
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2; }, false);
                GameManager.Instance.AnimationManager.PlayAnimation(215, target, false);
                target.AddStatModifier(Modifier.SpeedDown, 3);
            }
        };

        Skills["Chainsaw"] = new Skill
        {
            Name = "Chainsaw",
            Description = "Chainsaw",
            Cost = 0,
            Hidden = true,
            GoesFirst = false,
            Target = SkillTarget.Enemy,
            AnimationId = 208,
            Effect = async (self, target, skill) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] pulls out a chainsaw!");
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, target, false);
                for (int i = 0; i < 3; i++)
                {
                    BattleManager.Instance.Damage(self, target, () => { return 40; }, false, 0.75f, false, true);
                    await Task.Delay(500);
                }
            }
        };

        Skills["Swap"] = new Skill
        {
            Name = "Swap",
            Description = "Swap",
            Cost = 0,
            Hidden = true,
            GoesFirst = false,
            Target = SkillTarget.Enemy,
            AnimationId = 191,
            Effect = async (self, target, skill) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] did their thing!\nHEART and JUICE were swapped!");
                await GameManager.Instance.AnimationManager.WaitForScreenAnimation(skill.AnimationId, false);
                foreach (PartyMemberComponent member in BattleManager.Instance.GetAlivePartyMembers())
                {
                    int hp = member.Actor.CurrentHP;
                    int juice = member.Actor.CurrentJuice;
                    member.Actor.CurrentHP = juice + 1;
                    member.Actor.CurrentJuice = hp;
                }
            }
        };

        Skills["SlimeUltimateAttack"] = new Skill
        {
            Name = "SlimeUltimateAttack",
            Description = "SlimeUltimateAttack",
            Cost = 0,
            Hidden = true,
            GoesFirst = false,
            Target = SkillTarget.Enemy,
            AnimationId = 293,
            Effect = async (self, target, skill) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] throw everything they have!");
                GameManager.Instance.AnimationManager.PlayScreenAnimation(skill.AnimationId, false);
                await Task.Delay(1162);
                GameManager.Instance.AnimationManager.PlayScreenAnimation(181, false);
                await Task.Delay(332);
                foreach (PartyMemberComponent partyMember in BattleManager.Instance.GetAlivePartyMembers())
                {
                    BattleManager.Instance.SpawnDamageNumber(partyMember.Actor.CurrentJuice, partyMember.Actor.CenterPoint, DamageType.JuiceLoss);
                    partyMember.Actor.CurrentJuice = 0;
                }
                await Task.Delay(1660);
                // TODO: screen tint?
                await Task.Delay(332);
                GameManager.Instance.AnimationManager.PlayScreenAnimation(193, false);
                await Task.Delay(332);

            }
        };

        // AUBREY (Enemy) //

        Skills["AEAttack"] = new Skill
        {
            Name = "AEAttack",
            Description = "AEAttack",
            Cost = 0,
            Hidden = true,
            GoesFirst = false,
            Target = SkillTarget.Enemy,
            AnimationId = 28,
            Effect = async (self, target, skill) =>
            {
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, target, false);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false, neverCrit: true);
            }
        };

        Skills["AEDoNothing"] = new Skill
        {
            Name = "AEDoNothing",
            Description = "AEDoNothing",
            Cost = 0,
            Hidden = true,
            GoesFirst = false,
            Target = SkillTarget.Enemy,
            AnimationId = -1,
            Effect = async (self, target, skill) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] spits on your shoe.");
                await Task.CompletedTask;
            }
        };

        Skills["AEHeadbutt"] = new Skill
        {
            Name = "AEHeadbutt",
            Description = "AEHeadbutt",
            Cost = 0,
            Hidden = true,
            GoesFirst = false,
            Target = SkillTarget.Enemy,
            AnimationId = 124,
            Effect = async (self, target, skill) =>
            {
                await GameManager.Instance.AnimationManager.WaitForAnimation(skill.AnimationId, target, false);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] headbutts [target]!");
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 3 - target.CurrentStats.DEF; }, false, neverCrit: true);
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
                GameManager.Instance.AnimationManager.PlayAnimation(212, target, false);
                target.Heal(100);
                BattleManager.Instance.SpawnDamageNumber(100, target.CenterPoint, DamageType.Heal);
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
                GameManager.Instance.AnimationManager.PlayAnimation(212, target, false);
                float heal = target.CurrentStats.MaxHP * 0.4f;
                int finalHeal = (int)Math.Round(heal, MidpointRounding.AwayFromZero);
                target.Heal(finalHeal);
                BattleManager.Instance.SpawnDamageNumber(finalHeal, target.CenterPoint, DamageType.Heal);
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
                    target = BattleManager.Instance.GetRandomDeadPartyMember();
                    if (target == null)
                    {
                        BattleLogManager.Instance.QueueMessage("It had no effect.");
                        return;
                    }
                }
                await GameManager.Instance.AnimationManager.WaitForAnimation(item.AnimationId, target, false);
                target.CurrentHP = target.CurrentStats.MaxHP / 2;
                target.SetState("neutral", true);
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
                GameManager.Instance.AnimationManager.PlayAnimation(213, target, false);
                target.HealJuice(75);
                BattleManager.Instance.SpawnDamageNumber(75, target.CenterPoint, DamageType.JuiceGain);
                BattleLogManager.Instance.QueueMessage(self, target, "[target] recovered 75 JUICE!");
                await Task.CompletedTask;
            }
        };

        Items["FRIES"] = new Item
        {
            Name = "FRIES",
            Description = "From France, wherever that is...\nHeals 60 HEART to all friends.",
            Target = SkillTarget.AllAllies,
            AnimationId = 212,
            Effect = async (self, target, item) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] uses FRIES!");
                foreach (PartyMemberComponent member in BattleManager.Instance.GetAlivePartyMembers())
                {
                    GameManager.Instance.AnimationManager.PlayAnimation(212, member.Actor, false);
                    member.Actor.Heal(60);
                    BattleManager.Instance.SpawnDamageNumber(60, member.Actor.CenterPoint, DamageType.Heal);
                }
                BattleLogManager.Instance.QueueMessage("Everyone recovered 60 HEART!");
                await Task.CompletedTask;
            }
        };
        #endregion

        #region TOYS
        Items["RUBBER BAND"] = new Item
        {
            Name = "RUBBER BAND",
            Description = "Deals damage to a foe and reduces\ntheir DEFENSE.",
            IsToy = true,
            Target = SkillTarget.Enemy,
            AnimationId = 219,
            Effect = async (self, target, item) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] uses RUBBER BAND!");
                BattleManager.Instance.Damage(self, target, () => { return 50; }, true, 0, neverCrit: true);
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
                foreach (PartyMemberComponent member in BattleManager.Instance.GetAlivePartyMembers())
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

        Items["RAIN CLOUD"] = new Item
        {
            Name = "RAIN CLOUD",
            Description = "Angsty water droplets.\nInflicts SAD on all friends.",
            IsToy = true,
            Target = SkillTarget.AllAllies,
            AnimationId = -1,
            Effect = async (self, target, item) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] uses RAIN CLOUD!");
                AudioManager.Instance.PlaySFX("BA_sad_level_2", 1, 0.9f);
                foreach (PartyMemberComponent member in BattleManager.Instance.GetAlivePartyMembers())
                {
                    string state = "sad";
                    switch (member.Actor.CurrentState)
                    {
                        case "miserable":
                            BattleLogManager.Instance.QueueMessage(self, member.Actor, "[target] cannot be any sadder!");
                            continue;
                        case "depressed":
                            state = "miserable";
                            break;
                        case "sad":
                            state = "depressed";
                            break;
                    }
                    if (member.Actor.IsStateValid(state))
                        member.Actor.SetState(state);
                    else
                        BattleLogManager.Instance.QueueMessage(self, member.Actor, "[target] cannot be any sadder!");
                }
                await Task.CompletedTask;
            }
        };
        #endregion

        #region WEAPONS
        Weapons["Shiny Knife"] = new Weapon("Shiny Knife", new Stats(atk: 5, hit: 100));
        Weapons["Knife"] = new Weapon("Shiny Knife", new Stats(atk: 7, spd:2, hit: 100));
        Weapons["Dull Knife"] = new Weapon("Dull Knife", new Stats(atk: 9, spd: 4, lck: 2, hit: 100));
        Weapons["Rusty Knife"] = new Weapon("Rusty Knife", new Stats(atk: 11, def: 2, spd: 6, lck: 4, hit: 100));
        Weapons["Red Knife"] = new Weapon("Red Knife", new Stats(atk: 13, def: 6, spd: 6, lck: 6, hit: 100));

        Weapons["Fly Swatter"] = new Weapon("Fly Swatter", new Stats(atk: 1, hit: 1000));
        Weapons["Steak Knife"] = new Weapon("Steak Knife", new Stats(atk: 30, hit: 25));
        Weapons["Hands"] = new Weapon("Hands", new Stats(atk: 2, hit: 95));
        Weapons["Steak Knife"] = new Weapon("Steak Knife", new Stats(atk: 30, hit: 25));
        Weapons["Steak Knife"] = new Weapon("Steak Knife", new Stats(atk: 30, hit: 25));
        // potential todo: other violin variants?
        Weapons["Violin"] = new Weapon("Violin", new Stats(atk: 14, hit: 1000));

        Weapons["Stuffed Toy"] = new Weapon("Stuffed Toy", new Stats(atk: 4, hit: 100));
        Weapons["Comet Hammer"] = new Weapon("Comet Hammer", new Stats(atk: 6, lck: 2, hit: 100));
        Weapons["Body Pillow"] = new Weapon("Body Pillow", new Stats(hp: 10, atk: 8, hit: 100));
        Weapons["Pool Noodle"] = new Weapon("Pool Noodle", new Stats(atk: -5, def: -5, spd: -5, lck: -5, hit: 100));
        Weapons["Cool Noodle"] = new Weapon("Cool Noodle", new Stats(atk: 15, hit: 100));
        Weapons["Hero's Trophy"] = new Weapon("Hero's Trophy", new Stats(atk: 10, def: 5, hit: 100));
        Weapons["Mailbox"] = new Weapon("Mailbox", new Stats(atk: 12, hit: 100));
        Weapons["Baguette"] = new Weapon("Baguette", new Stats(atk: 10, def: 10, hit: 100));
        Weapons["Sweetheart Bust"] = new Weapon("Sweetheart Bust", new Stats(atk: 20, spd: -30, hit: 75));
        Weapons["Baseball Bat"] = new Weapon("Baseball Bat", new Stats(hp: 10, atk: 20, spd: 10, lck: 10, hit: 100));

        Weapons["Nail Bat"] = new Weapon("Nail Bat", new Stats(atk: 3, hit: 95));

        Weapons["Rubber Ball"] = new Weapon("Rubber Ball", new Stats(atk: 3, hit: 100));
        Weapons["Meteor Ball"] = new Weapon("Meteor Ball", new Stats(atk: 4, lck: 2, hit: 100));
        Weapons["Blood Orange"] = new Weapon("Blood Orange", new Stats(juice: 30, atk: 6, hit: 100));
        Weapons["Jack"] = new Weapon("Jack", new Stats(atk: 12, def: -6, lck: -6, hit: 100));
        Weapons["Beach Ball"] = new Weapon("Beach Ball", new Stats(atk: 10, spd: 25, hit: 100));
        Weapons["Coconut"] = new Weapon("Coconut", new Stats(juice: 50, atk: 8, hit: 100));
        Weapons["Globe"] = new Weapon("Globe", new Stats(atk: 10, hit: 1000));
        Weapons["Chicken Ball"] = new Weapon("Chicken Ball", new Stats(spd: 200, hit: 100));
        Weapons["Snowball"] = new Weapon("Snowball", new Stats(atk: 13, hit: 100));
        Weapons["Basketball"] = new Weapon("Basketball", new Stats(juice: 50, atk: 15, spd: 100, lck: 15, hit: 100));

        Weapons["Basketball (Real World)"] = new Weapon("Basketball", new Stats(atk: 2, hit: 95));

        Weapons["Spatula"] = new Weapon("Spatula", new Stats(atk: 4, hit: 100));
        Weapons["Rolling Pin"] = new Weapon("Rolling Pin", new Stats(hp: 10, atk: 12, def: 12, hit: 100));
        Weapons["Teapot"] = new Weapon("Teapot", new Stats(juice: 30, atk: 6, hit: 100));
        Weapons["Frying Pan"] = new Weapon("Frying Pan", new Stats(hp: 30, atk: 7, hit: 100));
        Weapons["Blender"] = new Weapon("Blender", new Stats(juice: 30, atk: 7, hit: 100));
        Weapons["Baking Pan"] = new Weapon("Baking Pan", new Stats(hp: 10, atk: 6, hit: 100));
        Weapons["Tenderizer"] = new Weapon("Tenderizer", new Stats(atk: 30, hit: 100));
        Weapons["LOL Sword"] = new Weapon("LOL Sword", new Stats(juice: 10, atk: 14, hit: 100));
        Weapons["Ol' Reliable"] = new Weapon("Ol' Reliable", new Stats(hp: 20, juice: 20, atk: 20, hit: 100));
        Weapons["Shucker"] = new Weapon("Shucker", new Stats(atk: 10, hit: 100));

        Weapons["Fist"] = new Weapon("Fist", new Stats(atk: 1, hit: 95));
        #endregion
    }
}