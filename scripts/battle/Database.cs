using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class Database
{
    private static readonly Dictionary<string, Skill> Skills = [];
    private static readonly Dictionary<string, Item> Items = [];
    private static readonly Dictionary<string, Weapon> Weapons = [];
    private static readonly Dictionary<string, Charm> Charms = [];

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

    public static bool TryGetCharm(string name, out Charm charm)
    {
        return Charms.TryGetValue(name, out charm);
    }

    public static void Init()
    {
        #region SKILLS
        Skills["Guard"] = new Skill(
            name: "GUARD",
            description: "Acts first, reducing damage taken for 1 turn.\nCost: 0",
            target: SkillTarget.Self,
            cost: 0,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] guards.");
                await GameManager.Instance.AnimationManager.WaitForAnimation(115, self, false);
                self.AddStatModifier(Modifier.Guard, 1, 1);
            },
            goesFirst: true
        );

        // OMORI //
        Skills["OAttack"] = new Skill(
            name: "Attack",
            description: "Basic Attack",
            target: SkillTarget.Enemy,
            cost: 0,
            effect: async (self, target) =>
            {
                await Task.Delay(1000);
                await GameManager.Instance.AnimationManager.WaitForAnimation(3, target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false);
            },
            hidden: true
        );
        Skills["SadPoem"] = new Skill(
            name: "SAD POEM",
            description: "Inflicts SAD on a friend or foe.\nCost: 5",
            target: SkillTarget.AllyOrEnemy,
            cost: 5,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] reads a sad poem.");
                await GameManager.Instance.AnimationManager.WaitForAnimation(5, self, false);
                MakeSad(target);
            }
        );
        Skills["LuckySlice"] = new Skill(
            name: "LUCKY SLICE",
            description: "Acts first. An attack that's stronger\nwhen OMORI is HAPPY. Cost: 15",
            target: SkillTarget.Enemy,
            cost: 15,
            effect: async (self, target) =>
            {
                await GameManager.Instance.AnimationManager.WaitForAnimation(8, target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] lunges at [target]!");
                if (self.CurrentState == "happy" || self.CurrentState == "ecstatic" || self.CurrentState == "manic")
                    BattleManager.Instance.Damage(self, target, () => { return (self.CurrentStats.ATK + self.CurrentStats.LCK) * 2f - target.CurrentStats.DEF; }, false);
                else
                    BattleManager.Instance.Damage(self, target, () => { return (self.CurrentStats.ATK + self.CurrentStats.LCK) * 1.5f - target.CurrentStats.DEF; }, false);
            },
            goesFirst: true
        );
        Skills["Stab"] = new Skill(
            name: "STAB",
            description: "Always deals a critical hit.\nIgnores DEFENSE when OMORI is sad. Cost: 13",
            target: SkillTarget.Enemy,
            cost: 13,
            effect: async (self, target) =>
            {
                await GameManager.Instance.AnimationManager.WaitForAnimation(9, target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] stabs [target].");
                if (self.CurrentState == "sad" || self.CurrentState == "depressed" || self.CurrentState == "miserable")
                    BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2f; }, false, guaranteeCrit: true);
                else
                    BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 1.5f - target.CurrentStats.DEF; }, false, guaranteeCrit: true);
            },
            goesFirst: true
        );

        Skills["Trick"] = new Skill(
            name: "TRICK",
            description: "Deals damage. If the foe is HAPPY, greatly\nreduce it's SPEED. Cost: 20",
            target: SkillTarget.Enemy,
            cost: 20,
            effect: async (self, target) =>
            {
                await GameManager.Instance.AnimationManager.WaitForAnimation(10, target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] tricks [target].");
                if (target.CurrentState == "happy" || target.CurrentState == "ecstatic" || target.CurrentState == "manic")
                {
                    GameManager.Instance.AnimationManager.PlayAnimation(219, target);
                    target.AddStatModifier(Modifier.SpeedDown, 3);
                }
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 3f - target.CurrentStats.DEF; }, false);
                await Task.Delay(334);
            }
        );

        Skills["HackAway"] = new Skill(
            name: "HACK AWAY",
            description: "Attacks 3 times, hitting random foes.\nCost: 30",
            target: SkillTarget.AllEnemies,
            cost: 30,
            effect: async (self, target) =>
            {
                await GameManager.Instance.AnimationManager.WaitForScreenAnimation(6, true);
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
        );

        Skills["PainfulTruth"] = new Skill(
            name: "PAINFUL TRUTH",
            description: "Deals damage to a foe. OMORI and the foe\nbecome SAD. Cost: 10",
            target: SkillTarget.Enemy,
            cost: 10,
            effect: async (self, target) =>
            {
                GameManager.Instance.AnimationManager.PlayAnimation(5, self, false);
                GameManager.Instance.AnimationManager.PlayAnimation(19, target);

                MakeSad(self);
                MakeSad(target);

                await Task.Delay(1000);

                BattleLogManager.Instance.QueueMessage(self, target, "[actor] whispers something\nto [target].");
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false);
            }
        );

        Skills["Shun"] = new Skill(
            name: "SHUN",
            description: "Deals damage. If the foe is SAD, greatly\nreduce it's DEFENSE. Cost: 20",
            target: SkillTarget.Enemy,
            cost: 20,
            effect: async (self, target) =>
            {
                await GameManager.Instance.AnimationManager.WaitForAnimation(12, target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] mocks [target].");
                if (target.CurrentState == "angry" || target.CurrentState == "enraged" || target.CurrentState == "furious")
                {
                    GameManager.Instance.AnimationManager.PlayAnimation(219, target);
                    target.AddStatModifier(Modifier.AttackDown, 3);
                }
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 3f - target.CurrentStats.DEF; }, false);
                await Task.Delay(334);
            }
        );

        Skills["Mock"] = new Skill(
            name: "MOCK",
            description: "Deals damage. If the foe is ANGRY, greatly\nreduce it's ATTACK. Cost: 20",
            target: SkillTarget.Enemy,
            cost: 20,
            effect: async (self, target) =>
            {
                await GameManager.Instance.AnimationManager.WaitForAnimation(11, target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] shuns [target].");
                if (target.CurrentState == "sad" || target.CurrentState == "depressed" || target.CurrentState == "miserable")
                {
                    GameManager.Instance.AnimationManager.PlayAnimation(219, target);
                    target.AddStatModifier(Modifier.DefenseDown, 3);
                }
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 3f - target.CurrentStats.DEF; }, false);
                await Task.Delay(334);
            }
        );

        Skills["Stare"] = new Skill(
            name: "STARE",
            description: "Reduces all of a foe's STATS.\nCost: 45",
            target: SkillTarget.Enemy,
            cost: 45,
            effect: async (self, target) =>
            {
                GameManager.Instance.AnimationManager.PlayAnimation(18, target);
                await Task.Delay(1660);
                GameManager.Instance.AnimationManager.PlayAnimation(219, target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] stares at [target].");
                BattleLogManager.Instance.QueueMessage(self, target, "[target] feels uncomfortable.");
                target.AddStatModifier(Modifier.AttackDown, 1);
                target.AddStatModifier(Modifier.DefenseDown, 1);
                target.AddStatModifier(Modifier.SpeedDown, 1);
                await Task.Delay(334);
            }
        );

        Skills["Exploit"] = new Skill(
            name: "EXPLOIT",
            description: "Deals extra damage to a HAPPY, SAD, or\nANGRY foe. Cost: 30",
            target: SkillTarget.Enemy,
            cost: 30,
            effect: async (self, target) =>
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
        );

        Skills["FinalStrike"] = new Skill(
            name: "FINAL STRIKE",
            description: "Strikes all foes. Deals more damage if OMORI\nhas a higher stage of EMOTION. Cost: 50",
            target: SkillTarget.AllEnemies,
            cost: 50,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] releases his ultimate\nattack!");
                await GameManager.Instance.AnimationManager.WaitForScreenAnimation(13, true);
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
        );

        Skills["RedHands"] = new Skill(
            name: "RED HANDS",
            description: "Deals big damage 4 times.\nCost: 75",
            target: SkillTarget.Enemy,
            cost: 75,
            effect: async (self, target) =>
            {
                await GameManager.Instance.AnimationManager.WaitForRedHands();
                for (int i = 0; i < 4; i++)
                {
                    BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 3f - target.CurrentStats.DEF; }, false);
                }
            }
        );

        // TODO: special skills (vertigo, cripple, suffocate), these require a special animation

        Skills["AttackAgain1"] = new Skill(
            name: "Attack Again 1",
            description: "Omori Followup",
            target: SkillTarget.Enemy,
            cost: 0,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] readies his blade.");
                await Task.Delay(1000);
                BattleLogManager.Instance.ClearBattleLog();
                await GameManager.Instance.AnimationManager.WaitForAnimation(3, target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks again!");
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false);
            },
            hidden: true
        );

        Skills["Trip1"] = new Skill(
            name: "Trip 1",
            description: "Omori Followup",
            target: SkillTarget.Enemy,
            cost: 0,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] walks forward.");
                await Task.Delay(1000);
                BattleLogManager.Instance.ClearBattleLog();
                await GameManager.Instance.AnimationManager.WaitForAnimation(14, target);
                GameManager.Instance.AnimationManager.PlayAnimation(219, target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] trips [target]!");
                target.AddStatModifier(Modifier.SpeedDown, 1);
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK + self.CurrentStats.LCK - target.CurrentStats.DEF; }, false);
            },
            hidden: true
        );

        Skills["ReleaseEnergy1"] = new Skill(
            name: "Release Energy 1",
            description: "Omori Followup",
            target: SkillTarget.AllEnemies,
            cost: 0,
            effect: async (self, target) =>
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
            },
            hidden: true
        );

        // SUNNY

        Skills["SAttack"] = new Skill(
            name: "Attack",
            description: "Basic Attack",
            target: SkillTarget.Enemy,
            cost: 0,
            effect: async (self, target) =>
            {
                await Task.Delay(1000);
                await GameManager.Instance.AnimationManager.WaitForAnimation(108, target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false, neverCrit: true);
            },
            hidden: true
        );

        Skills["CalmDown"] = new Skill(
            name: "CALM DOWN",
            description: "Removes EMOTIONS and heals some HEART.\nCost: 0",
            target: SkillTarget.Self,
            cost: 0,
            effect: async (self, target) =>
            {
                AudioManager.Instance.FadeBGMTo(10f);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] calms down.");
                GameManager.Instance.AnimationManager.PlayScreenAnimation(104, false);
                await Task.Delay(2500);
                self.Heal((int)Math.Round(self.BaseStats.MaxHP * 0.5, MidpointRounding.AwayFromZero));
                self.SetState("neutral", true);
                AudioManager.Instance.FadeBGMTo(100f);
            },
            goesFirst: true
        );


        // AUBREY //
        Skills["AAttack"] = new Skill(
            name: "Attack",
            description: "Basic Attack",
            target: SkillTarget.Enemy,
            cost: 0,
            effect: async (self, target) =>
            {
                await Task.Delay(1000);
                await GameManager.Instance.AnimationManager.WaitForAnimation(28, target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false);
            },
            hidden: true
        );
        Skills["PepTalk"] = new Skill(
            name: "PEP TALK",
            description: "Makes a friend or foe HAPPY.\nCost: 5",
            target: SkillTarget.AllyOrEnemy,
            cost: 5,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] cheers on [target]!");
                await GameManager.Instance.AnimationManager.WaitForScreenAnimation(29, false);
                MakeHappy(target);
            }
        );
        Skills["Headbutt"] = new Skill(
            name: "HEADBUTT",
            description: "Deals big damage, but AUBREY also takes damage.\nStronger when AUBREY is ANGRY. Cost: 5",
            target: SkillTarget.Enemy,
            cost: 5,
            effect: async (self, target) =>
            {
                double neededHp = Math.Floor(self.CurrentStats.MaxHP * 0.2);
                if (self.CurrentHP < neededHp)
                {
                    BattleLogManager.Instance.QueueMessage(self, target, "[actor] does not have enough HP!");
                    // refund juice
                    self.CurrentJuice += Skills["Headbutt"].Cost;
                    return;
                }
                await GameManager.Instance.AnimationManager.WaitForScreenAnimation(30, true);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] headbutts [target]!");
                if (self.CurrentState == "angry" || self.CurrentState == "enraged")
                    BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 3f - target.CurrentStats.DEF; }, false);
                else
                    BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2.5f - target.CurrentStats.DEF; }, false);
                self.CurrentHP = (int)Math.Max(1f, self.CurrentHP - Math.Floor(self.CurrentStats.MaxHP * 0.2));
            }
        );

        Skills["PowerHit"] = new Skill(
            name: "POWER HIT",
            description: "An attack that ignore's a foe's DEFENSE,\nthen reduces the foe's DEFENSE. Cost: 20",
            target: SkillTarget.Enemy,
            cost: 20,
            effect: async (self, target) =>
            {
                GameManager.Instance.AnimationManager.PlayAnimation(31, target);
                await Task.Delay(1000);
                await GameManager.Instance.AnimationManager.WaitForAnimation(219, target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] smashes [target]!");
                target.AddStatModifier(Modifier.DefenseDown);
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2f; }, false);
            }
        );

        Skills["Twirl"] = new Skill(
            name: "TWIRL",
            description: "AUBREY attacks a foe and becomes HAPPY.\nCost: 10",
            target: SkillTarget.Enemy,
            cost: 10,
            effect: async (self, target) =>
            {
                GameManager.Instance.AnimationManager.PlayAnimation(45, target);
                await Task.Delay(500);
                GameManager.Instance.AnimationManager.PlayAnimation(28, target);
                await Task.Delay(500);
                bool miss = BattleManager.Instance.Damage(self, target, () => { return (self.CurrentStats.ATK * 2f + self.CurrentStats.LCK) - target.CurrentStats.DEF; }, false);
                if (!miss)
                {
                    MakeHappy(self);
                }

            }
        );

        Skills["MoodWrecker"] = new Skill(
            name: "MOOD WRECKER",
            description: "A swing that doesn't miss. Deals extra damage to\nHAPPY foes. Cost: 10",
            target: SkillTarget.Enemy,
            cost: 10,
            effect: async (self, target) =>
            {
                await GameManager.Instance.AnimationManager.WaitForAnimation(46, target, true);
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
        );

        // TODO: add support for skills that use the <Not User> tag
        Skills["TeamSpirit"] = new Skill(
            name: "TEAM SPIRIT",
            description: "Makes AUBREY and a friend HAPPY.\nCost: 10",
            target: SkillTarget.Ally,
            cost: 10,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] cheers on [target]!");
                GameManager.Instance.AnimationManager.PlayAnimation(49, self, false);
                await Task.Delay(500);
                GameManager.Instance.AnimationManager.PlayScreenAnimation(29, false);
                MakeHappy(target);
                MakeHappy(self);
            }
        );

        Skills["WindUpThrow"] = new Skill(
            name: "WIND-UP THROW",
            description: "Damages all foes. Deals more damage the less\nenemies there are. Cost: 20",
            target: SkillTarget.AllEnemies,
            cost: 20,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] throws her weapon!");
                await GameManager.Instance.AnimationManager.WaitForAnimation(33, target);
                int enemies = BattleManager.Instance.GetAllEnemies().Count;
                if (enemies == 1)
                    BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 3f - target.CurrentStats.DEF; }, false);
                else if (enemies == 2)
                    BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2.5f - target.CurrentStats.DEF; }, false);
                else
                    BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2f - target.CurrentStats.DEF; }, false);
            }
        );

        Skills["Mash"] = new Skill(
            name: "MASH",
            description: "If this skill defeats a foe, recover 100% JUICE.\nCost: 15",
            target: SkillTarget.Enemy,
            cost: 15,
            effect: async (self, target) =>
            {
                GameManager.Instance.AnimationManager.PlayAnimation(28, target);
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
        );

        Skills["Beatdown"] = new Skill(
            name: "BEATDOWN",
            description: "Attacks a foe 3 times.\nCost: 30",
            target: SkillTarget.Enemy,
            cost: 30,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] furiously attacks!");
                await GameManager.Instance.AnimationManager.WaitForAnimation(17, target);
                for (int i = 0; i < 3; i++)
                {
                    BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2f - target.CurrentStats.DEF; }, false);
                    await Task.Delay(1000);
                }
            }
        );

        Skills["LastResort"] = new Skill(
            name: "LAST RESORT",
            description: "Deals damage based on AUBREY's HEART,\nbut AUBREY becomes TOAST. Cost: 50",
            target: SkillTarget.Enemy,
            cost: 50,
            effect: async (self, target) =>
            {
                await GameManager.Instance.AnimationManager.WaitForAnimation(34, target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] strikes [target]\nwith all her strength!");
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentHP * 4f; }, false);
                self.Damage(self.CurrentHP);
            }
        );

        Skills["LookAtOmori1"] = new Skill(
            name: "Look At Omori 1",
            description: "Aubrey Followup",
            target: SkillTarget.Enemy,
            cost: 0,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] looks at OMORI.");
                await Task.Delay(1000);
                await GameManager.Instance.AnimationManager.WaitForScreenAnimation(35, false);
                await GameManager.Instance.AnimationManager.WaitForAnimation(28, target);
                BattleLogManager.Instance.QueueMessage(self, target, "OMORI didn't notice AUBREY, so\nAUBREY attacks again!");
                BattleManager.Instance.Damage(self, target, () => { return (self.CurrentStats.ATK * 2 + self.CurrentStats.LCK) - target.CurrentStats.DEF; }, false);
            },
            hidden: true
        );

        Skills["LookAtKel1"] = new Skill(
            name: "Look At Kel 1",
            description: "Aubrey Followup",
            target: SkillTarget.Self,
            cost: 0,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] looks at KEL.");
                await Task.Delay(1000);
                GameManager.Instance.AnimationManager.PlayScreenAnimation(38, false);
                await Task.Delay(2000);
                BattleLogManager.Instance.QueueMessage(self, target, "KEL eggs [actor] on!");
                MakeAngry(self);
            },
            hidden: true
        );

        Skills["LookAtHero1"] = new Skill(
            name: "Look At Hero 1",
            description: "Aubrey Followup",
            target: SkillTarget.Self,
            cost: 0,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] looks at HERO.");
                await Task.Delay(1000);
                GameManager.Instance.AnimationManager.PlayScreenAnimation(41, false);
                await Task.Delay(2000);
                GameManager.Instance.AnimationManager.PlayAnimation(214, self, false);
                await Task.Delay(1000);
                BattleLogManager.Instance.QueueMessage(self, target, "HERO tells [actor] to focus!");
                self.AddStatModifier(Modifier.DefenseUp);
                MakeHappy(self);
            },
            hidden: true
        );



        Skills["ARWAttack"] = new Skill(
            name: "Attack",
            description: "Basic Attack",
            target: SkillTarget.Enemy,
            cost: 0,
            effect: async (self, target) =>
            {
                await GameManager.Instance.AnimationManager.WaitForAnimation(48, target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false, neverCrit: true);
            },
            hidden: true
        );

        Skills["Homerun"] = new Skill(
            name: "HOMERUN",
            description: "Has a chance to instantly defeat a\nfoe. AUBREY also takes damage. Cost: 25",
            target: SkillTarget.Enemy,
            cost: 25,
            effect: async (self, target) =>
            {
                await GameManager.Instance.AnimationManager.WaitForAnimation(32, target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] hits a home run!");
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 4f - target.CurrentStats.DEF; }, neverCrit: true);
                int roll = GameManager.Instance.Random.RandiRange(0, 100);
                if (roll < 11)
                {
                    target.CurrentHP = 0;
                }
                self.CurrentHP = Math.Max(0, (int)Math.Round(self.CurrentHP - self.BaseStats.MaxHP * 0.2f, MidpointRounding.AwayFromZero));
            }
        );

        // KEL //
        Skills["KAttack"] = new Skill(
            name: "Attack",
            description: "Basic Attack",
            target: SkillTarget.Enemy,
            cost: 0,
            effect: async (self, target) =>
            {
                await Task.Delay(1000);
                await GameManager.Instance.AnimationManager.WaitForAnimation(54, target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false);
            },
            hidden: true
        );
        Skills["Annoy"] = new Skill(
            name: "ANNOY",
            description: "Makes a friend or foe ANGRY.\nCost: 5",
            target: SkillTarget.AllyOrEnemy,
            cost: 5,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] annoys [target]!");
                await GameManager.Instance.AnimationManager.WaitForScreenAnimation(55, false);
                MakeAngry(target);
            }
        );
        Skills["Rebound"] = new Skill(
            name: "REBOUND",
            description: "Deals damage to all foes.\nCost: 15",
            target: SkillTarget.AllEnemies,
            cost: 15,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor]'s ball bounces everywhere!");
                await GameManager.Instance.AnimationManager.WaitForScreenAnimation(56, true);
                foreach (Enemy enemy in BattleManager.Instance.GetAllEnemies())
                    BattleManager.Instance.Damage(self, enemy, () => { return self.CurrentStats.ATK * 2.5f - enemy.CurrentStats.DEF; }, false);
            }
        );

        Skills["RunNGun"] = new Skill(
            name: "RUN 'N GUN",
            description: "KEL does an attack based on his SPEED\ninstead of his ATTACK. Cost: 15",
            target: SkillTarget.Enemy,
            cost: 15,
            effect: async (self, target) =>
            {
                GameManager.Instance.AnimationManager.PlayAnimation(72, self, false);
                await Task.Delay(500);
                GameManager.Instance.AnimationManager.PlayAnimation(54, target);
                await Task.Delay(500);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.SPD * 1.5f - target.CurrentStats.DEF; }, false);
            }
        );

        Skills["Curveball"] = new Skill(
            name: "CURVEBALL",
            description: "Makes a foe feel a random EMOTION. Deals\nextra damage to foes with EMOTION. Cost: 20",
            target: SkillTarget.Enemy,
            cost: 20,
            effect: async (self, target) =>
            {
                GameManager.Instance.AnimationManager.PlayScreenAnimation(73, true);
                await Task.Delay(1000);
                GameManager.Instance.AnimationManager.PlayAnimation(67, target);
                await Task.Delay(500);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] throws a curveball...");
                bool hit;
                if (target.CurrentState != "neutral")
                    hit = BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 3f - target.CurrentStats.DEF; }, false);
                else
                    hit = BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2f - target.CurrentStats.DEF; }, false);
                if (hit)
                {
                    BattleManager.Instance.RandomEmotion(target);
                }
            }
        );

        Skills["Ricochet"] = new Skill(
            name: "RICOCHET",
            description: "Deals damage to a foe 3 times.\nCost: 30",
            target: SkillTarget.Enemy,
            cost: 30,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] does a fancy ball trick!");
                await GameManager.Instance.AnimationManager.WaitForScreenAnimation(58, true);
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false, 0.3f);
                await Task.Delay(1000);
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false, 0.3f);
                await Task.Delay(1000);
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false, 0.3f);
            }
        );

        Skills["Megaphone"] = new Skill(
            name: "MEGAPHONE",
            description: "Makes all friends ANGRY.\nCost: 45",
            target: SkillTarget.AllAllies,
            cost: 45,
            effect: async (self, target) =>
            {
                GameManager.Instance.AnimationManager.PlayScreenAnimation(74, false);
                await Task.Delay(1000);
                await GameManager.Instance.AnimationManager.WaitForScreenAnimation(55, false);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] runs around and annoys everyone!");
                foreach (PartyMemberComponent member in BattleManager.Instance.GetAlivePartyMembers())
                {
                    MakeAngry(member.Actor);
                }
            }
        );

        Skills["Rally"] = new Skill(
            name: "RALLY",
            description: "KEL becomes HAPPY. KEL's friends recover\nsome ENERGY and JUICE. Cost: 50",
            target: SkillTarget.Self,
            cost: 50,
            effect: async (self, target) =>
            {
                GameManager.Instance.AnimationManager.PlayScreenAnimation(61, false);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] gets everyone pumped up!");
                MakeHappy(self);
                BattleLogManager.Instance.QueueMessage(self, target, "Everyone gains ENERGY!");
                BattleManager.Instance.AddEnergy(4);
                foreach (PartyMemberComponent member in BattleManager.Instance.GetAlivePartyMembers())
                {
                    GameManager.Instance.AnimationManager.PlayAnimation(213, member.Actor, false);
                    int rounded = (int)Math.Round(member.Actor.CurrentStats.MaxJuice * 0.3f, MidpointRounding.AwayFromZero);
                    target.HealJuice(rounded);
                    BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovered {rounded} JUICE!");
                }
                await Task.Delay(500);
            }
        );

        Skills["Comeback"] = new Skill(
            name: "COMEBACK",
            description: "Makes KEL HAPPY. If SAD was removed,\nKEL gains FLEX. Cost: 25",
            target: SkillTarget.Self,
            cost: 25,
            effect: async (self, target) =>
            {
                if (self.CurrentState == "sad" || self.CurrentState == "depressed" || self.CurrentState == "miserable")
                {
                    GameManager.Instance.AnimationManager.PlayAnimation(76, self, false);
                    await Task.Delay(1000);
                    self.AddStatModifier(Modifier.Flex, turns: int.MaxValue);
                    GameManager.Instance.AnimationManager.PlayAnimation(214, self, false);
                }
                else
                {
                    GameManager.Instance.AnimationManager.PlayAnimation(75, self, false);
                }
                MakeHappy(self);
            }
        );

        Skills["Tickle"] = new Skill(
            name: "TICKLE",
            description: "All attacks on a foe will hit right\nin the HEART for the turn. Cost: 55",
            target: SkillTarget.Enemy,
            cost: 55,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] tickles [target]!");
                BattleLogManager.Instance.QueueMessage(self, target, "[target] let their guard down!");
                target.AddStatModifier(Modifier.Tickle, turns: 1);
                await Task.CompletedTask;
            }
        );

        Skills["JuiceMe"] = new Skill(
            name: "JUICE ME",
            description: "Heals a lot of JUICE to a friend, but\nalso hurts the friend. Cost: 10",
            target: SkillTarget.Ally,
            cost: 10,
            effect: async (self, target) =>
            {
                string weapon = (self as PartyMember).Weapon.Name;
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] passes the " + weapon + " to [target]!");
                GameManager.Instance.AnimationManager.PlayAnimation(123, target, false);
                int rounded = (int)Math.Round(target.CurrentStats.MaxJuice * 0.3f, MidpointRounding.AwayFromZero);
                target.HealJuice(rounded);
                BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovered {rounded} JUICE!");
                // can juice me miss???
                BattleManager.Instance.Damage(self, target, () => { return target.CurrentHP * .25f; }, true, 0f, neverCrit: true);
                await Task.CompletedTask;
            }
        );

        Skills["Snowball"] = new Skill(
            name: "SNOWBALL",
            description: "Makes a foe SAD.\nAlso deals big damage to SAD foes. Cost: 20",
            target: SkillTarget.Enemy,
            cost: 20,
            effect: async (self, target) =>
            {
                await GameManager.Instance.AnimationManager.WaitForAnimation(60, target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] throws a snowball at [target]!");
                if (target.CurrentState == "sad" || target.CurrentState == "depressed" || target.CurrentState == "miserable")
                {
                    BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 3f - target.CurrentStats.DEF; }, false);
                }
                else
                {
                    BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2.5f - target.CurrentStats.DEF; }, false);
                    MakeSad(target);
                }
            }
        );

        Skills["Flex"] = new Skill(
            name: "FLEX",
            description: "KEL deals more damage next turn and increases\nHIT RATE for his next attack. Cost: 10",
            target: SkillTarget.Self,
            cost: 10,
            effect: async (self, target) =>
            {
                GameManager.Instance.AnimationManager.PlayScreenAnimation(57, true);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] flexes and feels his best!");
                BattleLogManager.Instance.QueueMessage(self, target, "[actor]'s HIT RATE rose!");
                self.AddStatModifier(Modifier.Flex, turns: int.MaxValue);
                await Task.CompletedTask;
            }
        );

        Skills["KRWAttack"] = new Skill(
            name: "Attack",
            description: "Basic Attack",
            target: SkillTarget.Enemy,
            cost: 0,
            effect: async (self, target) =>
            {
                await GameManager.Instance.AnimationManager.WaitForAnimation(77, target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false, neverCrit: true);
            },
            hidden: true
        );

        Skills["Encourage"] = new Skill(
            name: "ENCOURAGE",
            description: "KEL encourages a friend.\nRaises their attack. No cost.",
            target: SkillTarget.Ally,
            cost: 0,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] gives some encouragement!");
                GameManager.Instance.AnimationManager.PlayAnimation(214, target, false);
                await Task.Delay(1000);
                target.AddStatModifier(Modifier.AttackUp);
            }
        );
        Skills["PassToOmori1"] = new Skill(
            name: "Pass To Omori 1",
            description: "Kel Followup",
            target: SkillTarget.Ally,
            cost: 0,
            effect: async (self, target) =>
            {
                PartyMember first = BattleManager.Instance.GetPartyMember(0);
                BattleLogManager.Instance.QueueMessage(self, first, "[actor] passes to [target].");
                await Task.Delay(1000);
                GameManager.Instance.AnimationManager.PlayScreenAnimation(62, false);
                await Task.Delay(1000);
                BattleLogManager.Instance.QueueMessage(self, first, "[target] wasn't looking and gets bopped!");
                BattleManager.Instance.Damage(self, first, () => { return 1; }, true, 0f, false, true);
                first.SetState("sad");
            },
            hidden: true
        );
        Skills["PassToAubrey1"] = new Skill(
            name: "Pass To Aubrey 1",
            description: "Kel Followup",
            target: SkillTarget.Enemy,
            cost: 0,
            effect: async (self, target) =>
            {
                target = BattleManager.Instance.GetRandomAliveEnemy();
                PartyMember second = BattleManager.Instance.GetPartyMember(1);
                BattleLogManager.Instance.QueueMessage(self, second, "[actor] passes to [target].");
                await Task.Delay(1000);
                GameManager.Instance.AnimationManager.PlayScreenAnimation(65, true);
                await Task.Delay(2000);
                await GameManager.Instance.AnimationManager.WaitForAnimation(66, target);
                BattleLogManager.Instance.QueueMessage(self, second, "[target] knocks the ball out of the park!");
                BattleManager.Instance.Damage(self, target, () => { return second.CurrentStats.ATK + self.CurrentStats.ATK - target.CurrentStats.DEF; }, true);
            },
            hidden: true
        );
        Skills["PassToHero1"] = new Skill(
            name: "Pass To Hero 1",
            description: "Kel Followup",
            target: SkillTarget.AllEnemies,
            cost: 0,
            effect: async (self, target) =>
            {
                PartyMember second = BattleManager.Instance.GetPartyMember(1);
                PartyMember third = BattleManager.Instance.GetPartyMember(2);
                BattleLogManager.Instance.QueueMessage(self, third, "[actor] passes to [target].");
                await Task.Delay(1000);
                await GameManager.Instance.AnimationManager.WaitForScreenAnimation(69, true);
                BattleLogManager.Instance.QueueMessage(self, third, "[target] dunks on the foes!");
                foreach (Enemy enemy in BattleManager.Instance.GetAllEnemies())
                {
                    // VANILLA BUG: uses Aubrey's attack instead of Hero's
                    BattleManager.Instance.Damage(self, target, () => { return second.CurrentStats.ATK + self.CurrentStats.ATK - target.CurrentStats.DEF; }, true);
                }
            },
            hidden: true
        );

        // HERO //
        Skills["HAttack"] = new Skill(
            name: "Attack",
            description: "Basic Attack",
            target: SkillTarget.Enemy,
            cost: 0,
            effect: async (self, target) =>
            {
                await Task.Delay(1000);
                await GameManager.Instance.AnimationManager.WaitForAnimation(83, target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false);
            },
            hidden: true
        );
        Skills["Massage"] = new Skill(
            name: "MASSAGE",
            description: "Removes a friend or foe's EMOTION.\nCost: 5",
            target: SkillTarget.AllyOrEnemy,
            cost: 5,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] massages [target]!");
                await GameManager.Instance.AnimationManager.WaitForScreenAnimation(86, false);
                target.SetState("neutral", true);
                BattleLogManager.Instance.QueueMessage(target.Name.ToUpper() + " calms down...");
            }
        );
        Skills["SpicyFood"] = new Skill(
            name: "SPICY FOOD",
            description: "Damages a foe and makes them ANGRY.\nCost: 15",
            target: SkillTarget.Enemy,
            cost: 15,
            effect: async (self, target) =>
            {
                await GameManager.Instance.AnimationManager.WaitForAnimation(98, target);
                MakeAngry(target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] cooks some spicy food!");
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2f - target.CurrentStats.DEF; }, false, neverCrit: true);
            }
        );
        Skills["Tenderize"] = new Skill(
            name: "TENDERIZE",
            description: "Deals big damage to a foe and reduces\ntheir DEFENSE. Cost: 30",
            target: SkillTarget.Enemy,
            cost: 30,
            effect: async (self, target) =>
            {
                GameManager.Instance.AnimationManager.PlayScreenAnimation(86, true);
                await Task.Delay(332);
                GameManager.Instance.AnimationManager.PlayAnimation(124, target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] intensely massages\n[target]!");
                target.AddStatModifier(Modifier.DefenseDown);
                GameManager.Instance.AnimationManager.PlayAnimation(219, target);
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 4f - target.CurrentStats.DEF; }, false);
            }
        );
        Skills["Smile"] = new Skill(
           name: "SMILE",
           description: "Acts first, reducing a foe's ATTACK.\nCost: 25",
           target: SkillTarget.Enemy,
           cost: 25,
           goesFirst: true,
           effect: async (self, target) =>
           {
               BattleLogManager.Instance.QueueMessage(self, target, "[actor] smiles.");
               await GameManager.Instance.AnimationManager.WaitForScreenAnimation(87, false);
               await Task.Delay(332);
               target.AddStatModifier(Modifier.AttackDown);
               await GameManager.Instance.AnimationManager.WaitForAnimation(219, target);
           }
        );
        Skills["Dazzle"] = new Skill(
           name: "DAZZLE",
           description: "Acts first. Reduces all foes' ATTACK and\nmakes them HAPPY. Cost: 35",
           target: SkillTarget.AllEnemies,
           cost: 35,
           goesFirst: true,
           effect: async (self, target) =>
           {
               GameManager.Instance.AnimationManager.PlayAnimation(90, self, false);
               await Task.Delay(500);
               foreach (Enemy enemy in BattleManager.Instance.GetAllEnemies())
               {
                   BattleLogManager.Instance.QueueMessage(self, enemy, "[actor] smiles at [target]!");
                   GameManager.Instance.AnimationManager.PlayAnimation(276, enemy);
                   enemy.AddStatModifier(Modifier.AttackDown);
                   MakeHappy(enemy);
                   GameManager.Instance.AnimationManager.PlayAnimation(219, enemy);
               }
           }
        );
        Skills["FastFood"] = new Skill(
           name: "FAST FOOD",
           description: "Acts first, healing a friend for 40% of\ntheir HEART. Cost: 15",
           target: SkillTarget.Ally,
           cost: 15,
           goesFirst: true,
           effect: async (self, target) =>
           {
               BattleLogManager.Instance.QueueMessage(self, target, "[actor] prepares a quick meal for [target].");
               await GameManager.Instance.AnimationManager.WaitForAnimation(85, target, false);
               int rounded = (int)Math.Round(target.CurrentStats.MaxHP * .4f, MidpointRounding.AwayFromZero);
               target.Heal(rounded);
               BattleManager.Instance.SpawnDamageNumber(rounded, target.CenterPoint, DamageType.Heal);
               BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovered {rounded} HEART!");
               GameManager.Instance.AnimationManager.PlayAnimation(212, target);
               await Task.Delay(1000);
           }
        );
        Skills["ShareFood"] = new Skill(
           name: "SHARE FOOD",
           description: "HERO and a friend recover some HEART.\nCost: 15",
           target: SkillTarget.Ally,
           cost: 15,
           effect: async (self, target) =>
           {
               BattleLogManager.Instance.QueueMessage(self, target, "[actor] shares food with [target]!");
               GameManager.Instance.AnimationManager.PlayAnimation(85, target, false);
               GameManager.Instance.AnimationManager.PlayAnimation(85, self, false);

               int rounded = (int)Math.Round(target.CurrentStats.MaxHP * .5f, MidpointRounding.AwayFromZero);
               target.Heal(rounded);
               BattleManager.Instance.SpawnDamageNumber(rounded, target.CenterPoint, DamageType.Heal);
               GameManager.Instance.AnimationManager.PlayAnimation(212, target);

               rounded = (int)Math.Round(self.CurrentStats.MaxHP * .5f, MidpointRounding.AwayFromZero);
               self.Heal(rounded);
               BattleManager.Instance.SpawnDamageNumber(rounded, self.CenterPoint, DamageType.Heal);
               GameManager.Instance.AnimationManager.PlayAnimation(212, self);
               await Task.Delay(1000);
           }
        );
        Skills["SnackTime"] = new Skill(
           name: "SNACK TIME",
           description: "Heals all friends for 40% of their HEART.\nCost: 25",
           target: SkillTarget.AllAllies,
           cost: 25,
           effect: async (self, target) =>
           {
               BattleLogManager.Instance.QueueMessage(self, target, "[actor] made snacks for everyone!");
               GameManager.Instance.AnimationManager.PlayScreenAnimation(88, false);
               await Task.Delay(1666);
               foreach (PartyMemberComponent member in BattleManager.Instance.GetAlivePartyMembers())
               {
                   BattleManager.Instance.Heal(self, member.Actor, () => { return member.Actor.CurrentStats.MaxHP * 0.4f; }, 0f);
                   GameManager.Instance.AnimationManager.PlayAnimation(212, target, false);
               }
           }
        );
        Skills["GatorAid"] = new Skill(
           name: "GATOR AID",
           description: "Boosts all friends' DEFENSE.\nCost: 15",
           target: SkillTarget.AllAllies,
           cost: 15,
           effect: async (self, target) =>
           {
               await GameManager.Instance.AnimationManager.WaitForScreenAnimation(100, false);
               BattleLogManager.Instance.QueueMessage(self, target, "[actor] gets a little help from a friend.");
               BattleLogManager.Instance.QueueMessage("Everyone's DEFENSE rose!");
               foreach (PartyMemberComponent member in BattleManager.Instance.GetAlivePartyMembers())
               {
                    member.Actor.AddStatModifier(Modifier.DefenseUp, silent: true);
                    GameManager.Instance.AnimationManager.PlayAnimation(214, member.Actor, false);
               }
           }
        );
        Skills["TeaTime"] = new Skill(
            name: "TEA TIME",
            description: "Heals some of a friend's HEART and JUICE.\nCost: 25",
            target: SkillTarget.Ally,
            cost: 10,
            effect: async (self, target) =>
            {
                GameManager.Instance.AnimationManager.PlayAnimation(89, target, false);
                await Task.Delay(2000);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] brings out some tea for a break.");
                BattleLogManager.Instance.QueueMessage(self, target, "[target] feels refreshed!");
                int heartHeal = (int)Math.Round(target.CurrentStats.MaxHP * 0.3f, MidpointRounding.AwayFromZero);
                target.Heal(heartHeal);
                BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovers {heartHeal} HEART!");
                BattleManager.Instance.SpawnDamageNumber(heartHeal, target.CenterPoint, DamageType.Heal);
                int juiceHeal = (int)Math.Round(target.CurrentStats.MaxJuice * 0.2f, MidpointRounding.AwayFromZero);
                target.HealJuice(juiceHeal);
                BattleManager.Instance.SpawnDamageNumber(juiceHeal, target.CenterPoint + new Vector2(0, 50), DamageType.JuiceGain);
                BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovers {juiceHeal} JUICE!");
            }
        );
        Skills["Cook"] = new Skill(
            name: "COOK",
            description: "Heals a friend for 75% of their HEART.\nCost: 10",
            target: SkillTarget.Ally,
            cost: 10,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] makes a cookie just for [target]!");
                await GameManager.Instance.AnimationManager.WaitForAnimation(85, target, false);
                BattleManager.Instance.Heal(self, target, () => { return target.CurrentStats.MaxHP * 0.75f; });
                GameManager.Instance.AnimationManager.PlayAnimation(212, target, false);
                await Task.Delay(1000);
            }
        );
        Skills["Refresh"] = new Skill(
            name: "REFRESH",
            description: "Heals 50% of a friend's JUICE.\nCost: 40",
            target: SkillTarget.Ally,
            cost: 40,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] makes a refreshment for [target].");
                GameManager.Instance.AnimationManager.PlayAnimation(213, target, false);
                BattleManager.Instance.HealJuice(self, target, () => { return target.CurrentStats.MaxJuice * 0.5f; });
                await Task.Delay(1000);
            }
        );
        Skills["HomemadeJam"] = new Skill(
            name: "HOMEMADE JAM",
            description: "Brings back a friend that is TOAST.\nCost: 40",
            target: SkillTarget.DeadAlly,
            cost: 40,
            effect: async (self, target) =>
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
                await GameManager.Instance.AnimationManager.WaitForAnimation(269, target, false);
                target.SetState("neutral", true);
                int heal = (int)Math.Round(target.CurrentStats.MaxHP * 0.7f, MidpointRounding.AwayFromZero);
                target.Heal(heal);
                BattleManager.Instance.SpawnDamageNumber(heal, target.CenterPoint, DamageType.Heal);
                BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovered {heal} HEART!");
                BattleLogManager.Instance.QueueMessage(self, target, "[target] rose again!");
                await Task.Delay(1000);
            }
        );

        Skills["CallOmori1"] = new Skill(
            name: "Call Omori 1",
            description: "Hero Followup",
            target: SkillTarget.Ally,
            cost: 0,
            effect: async (self, target) =>
            {
                PartyMember first = BattleManager.Instance.GetPartyMember(0);
                BattleLogManager.Instance.QueueMessage(self, first, "[actor] calls out to [target].");
                await Task.Delay(1000);
                await GameManager.Instance.AnimationManager.WaitForScreenAnimation(93, false);
                await GameManager.Instance.AnimationManager.WaitForAnimation(212, first, false);
                int heal = (int)Math.Round(first.CurrentStats.MaxHP * 0.15f, MidpointRounding.AwayFromZero);
                first.Heal(heal);
                BattleLogManager.Instance.QueueMessage(self, first, "[actor] signals to [target]!");
                BattleLogManager.Instance.QueueMessage(self, first, $"[target] recovers {heal} HEART!");
                BattleManager.Instance.ForceCommand(first, BattleManager.Instance.GetRandomAliveEnemy(), Skills["OAttack"]);
            },
            hidden: true
        );

        Skills["CallAubrey1"] = new Skill(
            name: "Call Aubrey 1",
            description: "Hero Followup",
            target: SkillTarget.Ally,
            cost: 0,
            effect: async (self, target) =>
            {
                PartyMember second = BattleManager.Instance.GetPartyMember(1);
                BattleLogManager.Instance.QueueMessage(self, second, "[actor] calls out to [target].");
                await Task.Delay(1000);
                await GameManager.Instance.AnimationManager.WaitForScreenAnimation(94, false);
                await GameManager.Instance.AnimationManager.WaitForAnimation(212, second, false);
                int heal = (int)Math.Round(second.CurrentStats.MaxHP * 0.15f, MidpointRounding.AwayFromZero);
                second.Heal(heal);
                BattleLogManager.Instance.QueueMessage(self, second, "[actor] signals to [target]!");
                BattleLogManager.Instance.QueueMessage(self, second, $"[target] recovers {heal} HEART!");
                BattleManager.Instance.ForceCommand(second, BattleManager.Instance.GetRandomAliveEnemy(), Skills["AAttack"]);
            },
            hidden: true
        );

        Skills["CallKel1"] = new Skill(
            name: "Call Kel 1",
            description: "Hero Followup",
            target: SkillTarget.Ally,
            cost: 0,
            effect: async (self, target) =>
            {
                PartyMember fourth = BattleManager.Instance.GetPartyMember(3);
                BattleLogManager.Instance.QueueMessage(self, fourth, "[actor] calls out to [target].");
                await Task.Delay(1000);
                await GameManager.Instance.AnimationManager.WaitForScreenAnimation(95, false);
                await GameManager.Instance.AnimationManager.WaitForAnimation(212, fourth, false);
                int heal = (int)Math.Round(fourth.CurrentStats.MaxHP * 0.15f, MidpointRounding.AwayFromZero);
                fourth.Heal(heal);
                BattleLogManager.Instance.QueueMessage(self, fourth, "[actor] signals to [target]!");
                BattleLogManager.Instance.QueueMessage(self, fourth, $"[target] recovers {heal} HEART!");
                BattleManager.Instance.ForceCommand(fourth, BattleManager.Instance.GetRandomAliveEnemy(), Skills["KAttack"]);
            },
            hidden: true
        );

        Skills["HRWAttack"] = new Skill(
            name: "Attack",
            description: "Basic Attack",
            target: SkillTarget.Enemy,
            cost: 0,
            effect: async (self, target) =>
            {
                await GameManager.Instance.AnimationManager.WaitForAnimation(99, target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false, neverCrit: true);
            },
            hidden: true
        );

        Skills["FirstAid"] = new Skill(
            name: "FIRST AID",
            description: "Heals a friend for 25% of their HEART.\nCost: 10",
            target: SkillTarget.Ally,
            cost: 10,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] provides first aid!");
                await GameManager.Instance.AnimationManager.WaitForAnimation(114, target, false);
                float heal = target.CurrentStats.MaxHP * 0.25f;
                float variance = GameManager.Instance.Random.RandfRange(0.8f, 1.2f);
                int finalHeal = (int)Math.Round(heal * variance, MidpointRounding.AwayFromZero);
                target.Heal(finalHeal);
                BattleManager.Instance.SpawnDamageNumber(finalHeal, target.CenterPoint, DamageType.Heal);
                GameManager.Instance.AnimationManager.PlayAnimation(212, target, false);
                BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovered {finalHeal} HEART!");
                await Task.Delay(1000);
            }
        );

        // LOST SPROUT MOLE //
        Skills["LSMAttack"] = new Skill(
            name: "Attack",
            description: "Basic Attack",
            target: SkillTarget.Enemy,
            cost: 0,
            effect: async (self, target) =>
            {
                await GameManager.Instance.AnimationManager.WaitForAnimation(123, target, false);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] bumps into [target]!");
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false);
            },
            hidden: true
        );

        Skills["LSMDoNothing"] = new Skill(
            name: "Do Nothing",
            description: "Does nothing",
            target: SkillTarget.Enemy,
            cost: 0,
            effect: async (self, target) =>
            {
                AudioManager.Instance.PlaySFX("BA_do_nothing_dance");
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] is rolling around.");
                await Task.CompletedTask;
            },
            hidden: true
        );

        Skills["LSMRunAround"] = new Skill(
            name: "Run Around",
            description: "Run Around",
            target: SkillTarget.Enemy,
            cost: 0,
            effect: async (self, target) =>
            {
                GameManager.Instance.AnimationManager.PlayScreenAnimation(200, false);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] runs around!");
                await Task.Delay(100);
                target = BattleManager.Instance.GetRandomAlivePartyMember();
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 1.5f - target.CurrentStats.DEF; }, false);
                await Task.Delay(917);
                target = BattleManager.Instance.GetRandomAlivePartyMember();
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 1.5f - target.CurrentStats.DEF; }, false);
            },
            hidden: true
        );

        // FOREST BUNNY? //
        Skills["FBQAttack"] = new Skill(
            name: "Attack",
            description: "Basic Attack",
            target: SkillTarget.Enemy,
            cost: 0,
            effect: async (self, target) =>
            {
                await GameManager.Instance.AnimationManager.WaitForAnimation(123, target, false);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] nibbles at [target]?");
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false);
            },
            hidden: true
        );

        Skills["FBQDoNothing"] = new Skill(
            name: "Do Nothing",
            description: "Does nothing",
            target: SkillTarget.Enemy,
            cost: 0,
            effect: async (self, target) =>
            {
                AudioManager.Instance.PlaySFX("BA_do_nothing_falls_over");
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] is hopping around?");
                await Task.CompletedTask;
            },
            hidden: true
        );

        Skills["FBQBeCute"] = new Skill(
            name: "Be Cute",
            description: "Be Cute",
            target: SkillTarget.Enemy,
            cost: 0,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] winks at [target]?");
                await GameManager.Instance.AnimationManager.WaitForAnimation(148, self);
                await GameManager.Instance.AnimationManager.WaitForAnimation(215, target, false);
                target.AddStatModifier(Modifier.AttackDown);
            },
            hidden: true
        );

        Skills["FBQSadEyes"] = new Skill(
            name: "Sad Eyes",
            description: "Sad Eyes",
            target: SkillTarget.Enemy,
            cost: 0,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] looks sadly at [target]?");
                await GameManager.Instance.AnimationManager.WaitForAnimation(149, self);
                MakeSad(target);
            },
            hidden: true
        );

        // SWEETHEART //
        Skills["SHAttack"] = new Skill(
            name: "Attack",
            description: "Basic Attack",
            target: SkillTarget.Enemy,
            cost: 0,
            effect: async (self, target) =>
            {
                await GameManager.Instance.AnimationManager.WaitForAnimation(132, target, false);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] slaps [target].");
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false);
            },
            hidden: true
        );

        Skills["SharpInsult"] = new Skill(
            name: "Sharp Insult",
            description: "Sharp Insult",
            target: SkillTarget.AllEnemies,
            cost: 0,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] insults everyone!");
                await GameManager.Instance.AnimationManager.WaitForScreenAnimation(183, false);
                foreach (PartyMemberComponent member in BattleManager.Instance.GetAlivePartyMembers()) {
                    BattleManager.Instance.Damage(self, member.Actor, () => { return self.CurrentStats.ATK; }, false, 0.1f, neverCrit: true);
                    MakeAngry(member.Actor);
                }
            },
            hidden: true
        );

        Skills["SwingMace"] = new Skill(
            name: "Swing Mace",
            description: "Swing Mace",
            target: SkillTarget.AllEnemies,
            cost: 0,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] swings her mace!");
                await GameManager.Instance.AnimationManager.WaitForScreenAnimation(206, false);
                foreach (PartyMemberComponent member in BattleManager.Instance.GetAlivePartyMembers())
                {
                    BattleManager.Instance.Damage(self, member.Actor, () => { return self.CurrentStats.ATK * 2.5f - member.Actor.CurrentStats.DEF; }, false);
                }
            },
            hidden: true
        );

        Skills["Brag"] = new Skill(
            name: "Brag",
            description: "Brag",
            target: SkillTarget.Self,
            cost: 0,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] boasts about one of her\nmany, many talents!");
                await GameManager.Instance.AnimationManager.WaitForScreenAnimation(162, false);
                MakeHappy(self);
            },
            hidden: true
        );

        // SLIME GIRLS //
        Skills["ComboAttack"] = new Skill(
            name: "ComboAttack",
            description: "ComboAttack",
            target: SkillTarget.Enemy,
            cost: 0,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "The [actor] attack all at once!");
                GameManager.Instance.AnimationManager.PlayAnimation(133, target, false);
                await Task.Delay(580);
                GameManager.Instance.AnimationManager.PlayAnimation(134, target, false);
                await Task.Delay(580);
                GameManager.Instance.AnimationManager.PlayAnimation(135, target, false);
                await Task.Delay(580);
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2f - target.CurrentStats.DEF; }, false, neverCrit: true);
            },
            hidden: true
        );

        Skills["StrangeGas"] = new Skill(
            name: "StrangeGas",
            description: "StrangeGas",
            target: SkillTarget.AllEnemies,
            cost: 0,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage("MEDUSA threw a bottle...");
                GameManager.Instance.AnimationManager.PlayScreenAnimation(194, false);
                await Task.Delay(1500);
                GameManager.Instance.AnimationManager.PlayScreenAnimation(181, false);
                BattleLogManager.Instance.QueueMessage("A strange gas fills the room.");
                await Task.Delay(2000);

                foreach (PartyMemberComponent member in BattleManager.Instance.GetAlivePartyMembers())
                {
                    BattleManager.Instance.RandomEmotion(member.Actor);
                }
            },
            hidden: true
        );

        Skills["Dynamite"] = new Skill(
            name: "Dynamite",
            description: "Dynamite",
            target: SkillTarget.AllEnemies,
            cost: 0,
            effect: async (self, target) =>
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
            },
            hidden: true
        );

        Skills["StingRay"] = new Skill(
            name: "StingRay",
            description: "StingRay",
            target: SkillTarget.Enemy,
            cost: 0,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "MOLLY fires her stingers!\n[target] gets struck!");
                await GameManager.Instance.AnimationManager.WaitForAnimation(193, target, false);
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2; }, false, neverCrit: true);
                GameManager.Instance.AnimationManager.PlayAnimation(215, target, false);
                target.AddStatModifier(Modifier.SpeedDown, 3);
            },
            hidden: true
        );

        Skills["Chainsaw"] = new Skill(
            name: "Chainsaw",
            description: "Chainsaw",
            target: SkillTarget.Enemy,
            cost: 0,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] pulls out a chainsaw!");
                await GameManager.Instance.AnimationManager.WaitForAnimation(208, target, false);
                for (int i = 0; i < 3; i++)
                {
                    BattleManager.Instance.Damage(self, target, () => { return 40; }, false, 0.75f, false, true);
                    await Task.Delay(500);
                }
            },
            hidden: true
        );

        Skills["Swap"] = new Skill(
            name: "Swap",
            description: "Swap",
            target: SkillTarget.Enemy,
            cost: 0,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] did their thing!\nHEART and JUICE were swapped!");
                await GameManager.Instance.AnimationManager.WaitForScreenAnimation(191, false);
                foreach (PartyMemberComponent member in BattleManager.Instance.GetAlivePartyMembers())
                {
                    int hp = member.Actor.CurrentHP;
                    int juice = member.Actor.CurrentJuice;
                    member.Actor.CurrentHP = juice + 1;
                    member.Actor.CurrentJuice = hp;
                }
            },
            hidden: true
        );

        Skills["SlimeUltimateAttack"] = new Skill(
            name: "SlimeUltimateAttack",
            description: "SlimeUltimateAttack",
            target: SkillTarget.Enemy,
            cost: 0,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] throw everything they have!");
                GameManager.Instance.AnimationManager.PlayScreenAnimation(293, false);
                await Task.Delay(1162);
                GameManager.Instance.AnimationManager.PlayScreenAnimation(181, false);
                await Task.Delay(332);
                foreach (PartyMemberComponent partyMember in BattleManager.Instance.GetAlivePartyMembers())
                {
                    BattleManager.Instance.SpawnDamageNumber(partyMember.Actor.CurrentJuice, partyMember.Actor.CenterPoint, DamageType.JuiceLoss);
                    partyMember.Actor.CurrentJuice = 0;
                }
                await Task.Delay(1660);
                // TODO: screen tint
                await Task.Delay(332);
                GameManager.Instance.AnimationManager.PlayScreenAnimation(193, false);
                await Task.Delay(664);
                foreach (PartyMemberComponent partyMember in BattleManager.Instance.GetAlivePartyMembers())
                {
                    partyMember.Actor.AddStatModifier(Modifier.AttackDown, 3, silent: true);
                    partyMember.Actor.AddStatModifier(Modifier.DefenseDown, 3, silent: true);
                    partyMember.Actor.AddStatModifier(Modifier.SpeedDown, 3, silent: true);
                    GameManager.Instance.AnimationManager.PlayAnimation(215, partyMember.Actor, false);
                }
                BattleLogManager.Instance.QueueMessage("Everyone's ATTACK fell.");
                await Task.Delay(166);
                BattleLogManager.Instance.QueueMessage("Everyone's DEFENSE fell.");
                await Task.Delay(166);
                BattleLogManager.Instance.QueueMessage("Everyone's SPEED fell.");
                await Task.Delay(1660);
                GameManager.Instance.AnimationManager.PlayScreenAnimation(172, false);
                await Task.Delay(332);
                foreach (PartyMemberComponent partyMember in BattleManager.Instance.GetAlivePartyMembers())
                {
                    BattleManager.Instance.Damage(self, partyMember.Actor, () => { return partyMember.Actor.CurrentStats.MaxHP * 0.4f; }, false, 0f, neverCrit: true);
                    BattleManager.Instance.RandomEmotion(partyMember.Actor);
                }
                await Task.Delay(664);
            },
            hidden: true
        );

        // AUBREY (Enemy) //

        Skills["AEAttack"] = new Skill(
            name: "AEAttack",
            description: "AEAttack",
            target: SkillTarget.Enemy,
            cost: 0,
            effect: async (self, target) =>
            {
                await GameManager.Instance.AnimationManager.WaitForAnimation(28, target, false);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks [target]!");
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 2 - target.CurrentStats.DEF; }, false, neverCrit: true);
            },
            hidden: true
        );

        Skills["AEDoNothing"] = new Skill(
            name: "AEDoNothing",
            description: "AEDoNothing",
            target: SkillTarget.Enemy,
            cost: 0,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] spits on your shoe.");
                await Task.CompletedTask;
            },
            hidden: true
        );

        Skills["AEHeadbutt"] = new Skill(
            name: "AEHeadbutt",
            description: "AEHeadbutt",
            target: SkillTarget.Enemy,
            cost: 0,
            effect: async (self, target) =>
            {
                await GameManager.Instance.AnimationManager.WaitForAnimation(124, target, false);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] headbutts [target]!");
                BattleManager.Instance.Damage(self, target, () => { return self.CurrentStats.ATK * 3 - target.CurrentStats.DEF; }, false, neverCrit: true);
            },
            hidden: true
        );

        #endregion

        #region SNACKS

        // will most likely be file driven in the future

        AddSnack("Tofu", "Soft cardboard, basically.\nHeals 5 HEART.", 5);
        AddSnack("Candy", "A child's favorite food. Sweet!\nHeals 30 HEART.", 30);
        AddSnack("Smores", "S'more smores, please!\nHeals 50 HEART.", 50);
        AddSnack("Granola Bar", "A healthy stick of grain.\nHeals 60 HEART.", 60);
        AddSnack("Bread", "A slice of life.\nHeals 60 HEART.", 60);
        AddSnack("Nachos", "Suggested serving size: 6-8 nachos.\nHeals 75 HEART.", 75);
        AddSnack("Chicken Wing", "Wing of chicken.\nHeals 80 HEART.", 80);
        AddSnack("Hot Dog", "Better than a cold dog.\nHeals 100 HEART.", 100);
        AddSnack("Waffle", "Designed to hold syrup!\nHeals 150 HEART.", 150);
        AddSnack("Pancake", "Not designed to hold syrup...\nHeals 150 HEART.", 150);
        AddSnack("Pizza Slice", "1/8th of a Whole pizza.\nHeals 175 HEART.", 175);
        AddSnack("Fish Taco", "Aquatic taco.\nHeals 200 HEART.", 200);
        AddSnack("Cheeseburger", "Contains all food groups, so it's healthy!\nHeals 250 HEART.", 250);

        AddSnack("Chocolate", "Chocolate!? Oh, it's baking chocolate...\nHeals 40% of HEART.", 0.4f);
        AddSnack("Donut", "Circular bread with a hole in it.\nHeals 60% of HEART.", 0.6f);
        AddSnack("Ramen", "Now that is a lot of sodium!\nHeals 80% of HEART.", 0.8f);
        AddSnack("Spaghetti", "Wet noodles slathered with chunky sauce.\nFully heals a friend's HEART.", 1.0f);
        AddSnack("Dino Pasta", "Pasta shaped line dinosaurs.\nFully restores a friend's HEART.", 1.0f);

        AddGroupSnack("Popcorn", "9/10 dentists hate it.\nHeals 35 HEART to all friends.", 35);
        AddGroupSnack("Fries", "From France, wherever that is...\nHeals 60 HEART to all friends.", 60);
        AddGroupSnack("Cheese Wheel", "Delicious, yet functional.\nHeals 100 HEART to all friends.", 100);
        AddGroupSnack("Whole Chicken", "An entire chicken, wings and all.\nHeals 175 HEART to all friends.", 175);
        AddGroupSnack("Whole Pizza", "8/8ths of a whole pizza.\nHeals 250 HEART to all friends.", 250);
        AddGroupSnack("Dino Clumps", "Chicken nuggets shaped like dinosaurs.\nHeals 250 HEART to all friends.", 250);

        AddJuiceSnack("Plum Juice", "For seniors. Wait, that's prune juice.\nHeals 15 JUICE.", 15);
        AddJuiceSnack("Apple Juice", "Apparently better than orange juice.\nHeals 25 JUICE.", 25);
        AddJuiceSnack("Breadfruit Juice", "Does not taste like bread.\nHeals 50 JUICE.", 50);
        AddJuiceSnack("Lemonade", "When life gives you lemons, make this!\nHeals 75 JUICE.", 75);
        AddJuiceSnack("Orange Juice", "Apparently better than apple juice.\nHeals 100 JUICE.", 100);
        AddJuiceSnack("Pineapple Juice", "Painful... Why do you drink it?\nHeals 150 JUICE.", 150);
        AddJuiceSnack("Bottled Water", "Water in a bottle.\nHeals 100 JUICE.", 100);
        AddJuiceSnack("Fruit Juice?", "You're not sure what fruit it is.\nHeals 75 JUICE.", 75);


        AddJuiceSnack("Cherry Soda", "Carbonated hell sludge.\nHeals 25% of JUICE.", 0.25f);
        AddJuiceSnack("Star Fruit Soda", "To be shared with a friend.\nHeals 35% of JUICE.", 0.35f);
        AddJuiceSnack("Tasty Soda", "Tasty soda for thirsty people.\nHeals 50% of JUICE.", 0.5f);
        AddJuiceSnack("Peach Soda", "A regular peach soda.\nHeals 60% of JUICE.", 0.6f);
        AddJuiceSnack("Butt Peach Soda", "An irregular peach soda.\nHeals 61% of JUICE.", 0.61f);
        AddJuiceSnack("Watermelon Juice", "Heavenly nectar.\nFully heals a friend's JUICE.", 1.0f);
        AddJuiceSnack("Dino Melon Soda", "Melon soda in a dino-shaped bottle.\nFully heals a friend's JUICE.", 1.0f);

        AddGroupJuiceSnack("Banana Smoothie", "A little bland, but it does the job.\nHeals 20 JUICE to all friends.", 20);
        AddGroupJuiceSnack("Mango Smoothie", "Makes you tango!\nHeals 40 JUICE to all friends.", 40);
        AddGroupJuiceSnack("Berry Smoothie", "A healthy smoothie that tastes like dirt.\nHeals 60 JUICE to all friends.", 60);
        AddGroupJuiceSnack("Melon Smoothie", "Chunky green melon goodness.\nHeals 80 JUICE to all friends.", 80);
        AddGroupJuiceSnack("S.berry Smoothie", "The default smoothie.\nHeals 100 JUICE to all friends.", 100);
        AddGroupJuiceSnack("Dino Smoothie", "Berry smoothie in a dino-shaped cup.\nHeals 150 JUICE to all friends.", 150);

        AddComboSnack("Tomato", "You say tomato, I say tomato.\nHeals 100 HEART and 50 JUICE.", 100, 50);
        AddComboSnack("Combo Meal", "What more could you ask for?", 250, 100);

        Items["Grape Soda"] = new Item(
            name: "GRAPE SODA",
            description: "Objectively the best soda.\nHeals 80% of JUICE.",
            target: SkillTarget.Ally,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, $"[actor] uses GRAPE SODA!");
                GameManager.Instance.AnimationManager.PlayAnimation(212, target, false);
                // grape soda uses emotion due to an oversight
                BattleManager.Instance.HealJuice(self, target, () => { return target.CurrentStats.MaxJuice * 0.8f; });
                await Task.CompletedTask;
            }
        );

        Items["Coffee"] = new Item(
            name: "COFFEE",
            description: "Bitter bean juice.\nIncreases a friend's SPEED.",
            target: SkillTarget.Ally,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, $"[actor] uses COFFEE!");
                GameManager.Instance.AnimationManager.PlayAnimation(214, target, false);
                // coffee heals, uses emotion, and has a variance due to an oversight
                BattleManager.Instance.Heal(self, target, () => { return target.CurrentStats.MaxJuice * 0.1f; }, 0.2f);
                target.AddStatModifier(Modifier.SpeedUp, 3);
                await Task.CompletedTask;
            }
        );

        Items["☐☐☐"] = new Item(
           name: "☐☐☐",
           description: "☐☐☐☐☐☐☐☐☐ ☐☐☐ ☐☐☐",
           target: SkillTarget.Ally,
           effect: async (self, target) =>
           {
               BattleLogManager.Instance.QueueMessage(self, target, $"[actor] uses ☐☐☐!");
               GameManager.Instance.AnimationManager.PlayAnimation(215, target, false);
               // ☐☐☐ uses emotion due to an oversight
               BattleManager.Instance.Heal(self, target, () => { return 50; }, 0f);
               await Task.CompletedTask;
           }
       );

        Items["Prune Juice"] = new Item(
            name: "PRUNE JUICE",
            description: "This tastes horrible. Don't drink it.\nHeals 30 JUICE...probably.",
            target: SkillTarget.Ally,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] uses PRUNE JUICE!");
                GameManager.Instance.AnimationManager.PlayAnimation(213, target, false);
                int total = 30;
                if (BattleManager.Instance.GetAllPartyMembers().Any(x => x.Actor.Weapon.Name == "Blender" || x.Actor.Weapon.Name == "Ol' Reliable"))
                    total = 45;
                target.HealJuice(total);
                BattleManager.Instance.SpawnDamageNumber(total, target.CenterPoint, DamageType.JuiceGain);
                BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovered {total} JUICE!");
                int hpLoss = (int)Math.Round(target.CurrentHP * 0.3f, MidpointRounding.AwayFromZero);
                target.Damage(hpLoss);
                // damaging items don't kill
                if (target.CurrentHP == 0)
                    target.CurrentHP = 1;
                await Task.CompletedTask;
            }
        );

        Items["Rotten Milk"] = new Item(
            name: "ROTTEN MILK",
            description: "This is bad. Don't drink it.\nHeals 10 juice + ???",
            target: SkillTarget.Ally,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] uses ROTTEN MILK!");
                GameManager.Instance.AnimationManager.PlayAnimation(213, target, false);
                int total = 10;
                if (BattleManager.Instance.GetAllPartyMembers().Any(x => x.Actor.Weapon.Name == "Blender" || x.Actor.Weapon.Name == "Ol' Reliable"))
                    total = 15;
                target.HealJuice(total);
                BattleManager.Instance.SpawnDamageNumber(total, target.CenterPoint, DamageType.JuiceGain);
                BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovered {total} JUICE!");
                int hpLoss = (int)Math.Round(target.CurrentHP * 0.5f, MidpointRounding.AwayFromZero);
                target.Damage(hpLoss);
                // damaging items don't kill
                if (target.CurrentHP == 0)
                    target.CurrentHP = 1;
                await Task.CompletedTask;
            }
        );

        Items["Milk"] = new Item(
            name: "MILK",
            description: "Good for your bones. Heals 10 juice\nand increases DEFENSE for the battle.",
            target: SkillTarget.Ally,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] uses MILK!");
                GameManager.Instance.AnimationManager.PlayAnimation(213, target, false);
                await Task.Delay(2000);
                GameManager.Instance.AnimationManager.PlayAnimation(214, target, false);
                int total = 10;
                if (BattleManager.Instance.GetAllPartyMembers().Any(x => x.Actor.Weapon.Name == "Blender" || x.Actor.Weapon.Name == "Ol' Reliable"))
                    total = 15;
                target.HealJuice(total);
                BattleManager.Instance.SpawnDamageNumber(total, target.CenterPoint, DamageType.JuiceGain);
                BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovered {total} JUICE!");
                target.AddStatModifier(Modifier.DefenseUp);
                await Task.CompletedTask;
            }
        );

        Items["Sno-Cone"] = new Item(
            name: "SNO-CONE",
            description: "Heals a friend's HEART and JUICE, and\nraises ALL STATS for the battle.",
            target: SkillTarget.Ally,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, $"[actor] uses SNO-CONE!");
                await GameManager.Instance.AnimationManager.WaitForAnimation(214, target, false);
                target.Heal(target.CurrentStats.MaxHP);
                target.HealJuice(target.CurrentStats.MaxJuice);
                target.AddStatModifier(Modifier.SnoCone, turns: int.MaxValue);
                BattleLogManager.Instance.QueueMessage(self, target, $"[actor]'s ATTACK rose!");
                BattleLogManager.Instance.QueueMessage(self, target, $"[actor]'s DEFENSE rose!");
                BattleLogManager.Instance.QueueMessage(self, target, $"[actor]'s SPEED rose!");
                BattleLogManager.Instance.QueueMessage(self, target, $"[actor]'s LUCK rose!");
            }
        );

        Items["Life Jam"] = new Item(
            name: "LIFE JAM",
            description: "Infused with the spirit of life.\nRevives a friend that is TOAST.",
            target: SkillTarget.DeadAlly,
            effect: async (self, target) =>
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
                await GameManager.Instance.AnimationManager.WaitForAnimation(269, target, false);
                if (BattleManager.Instance.GetAllPartyMembers().Any(x => x.Actor.Charm.Name == "Breadphones"))
                    target.CurrentHP = target.CurrentStats.MaxHP;
                else
                    target.CurrentHP = target.CurrentStats.MaxHP / 2;
                target.SetState("neutral", true);
                BattleLogManager.Instance.QueueMessage(self, target, "[target] rose again!");
            }
        );

        Items["Dino Jam"] = new Item(
           name: "DINO JAM",
           description: "Infused with the spirit of dino life.\nFully revives a friend that is TOAST.",
           target: SkillTarget.DeadAlly,
           effect: async (self, target) =>
           {
               BattleLogManager.Instance.QueueMessage(self, target, "[actor] uses DINO JAM!");
               if (target.CurrentState != "toast")
               {
                   target = BattleManager.Instance.GetRandomDeadPartyMember();
                   if (target == null)
                   {
                       BattleLogManager.Instance.QueueMessage("It had no effect.");
                       return;
                   }
               }
               await GameManager.Instance.AnimationManager.WaitForAnimation(269, target, false);
               target.CurrentHP = target.CurrentStats.MaxHP;
               target.SetState("neutral", true);
               BattleLogManager.Instance.QueueMessage(self, target, "[target] rose again!");
           }
        );

        Items["Jam Packets"] = new Item(
           name: "JAM PACKETS",
           description: "Infused with the spirit of life.\nRevives all friends that are TOAST.",
           target: SkillTarget.AllDeadAllies,
           effect: async (self, target) =>
           {
               BattleLogManager.Instance.QueueMessage(self, null, "[actor] uses JAM PACKETS!");
               List<PartyMemberComponent> dead = BattleManager.Instance.GetDeadPartyMembers();
               if (dead.Count == 0)
               {
                   BattleLogManager.Instance.QueueMessage("It had no effect.");
                   return;
               }
               foreach (PartyMemberComponent member in dead)
               {
                   GameManager.Instance.AnimationManager.PlayAnimation(269, member.Actor, false);
                   member.Actor.CurrentHP = member.Actor.CurrentStats.MaxHP / 4;
                   member.Actor.SetState("neutral", true);
                   BattleLogManager.Instance.QueueMessage(self, member.Actor, "[target] rose again!");
               }
               await Task.CompletedTask;
           }
        );

        // TODO: faraway town snacks

        #endregion

        #region TOYS
        Items["RUBBER BAND"] = new Item(
            name: "RUBBER BAND",
            description: "Deals damage to a foe and reduces\ntheir DEFENSE.",
            target: SkillTarget.Enemy,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] uses RUBBER BAND!");
                BattleManager.Instance.Damage(self, target, () => { return 50; }, true, 0, neverCrit: true);
                await GameManager.Instance.AnimationManager.WaitForAnimation(219, target);
                target.AddStatModifier(Modifier.DefenseDown);
            },
            isToy: true
        );

        Items["AIR HORN"] = new Item(
            name: "AIR HORN",
            description: "Who would invent this!?\nInflicts ANGER on all friends.",
            target: SkillTarget.AllAllies,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] uses AIR HORN!");
                AudioManager.Instance.PlaySFX("SE_airhorn", 1, 0.9f);
                foreach (PartyMemberComponent member in BattleManager.Instance.GetAlivePartyMembers())
                {
                    MakeAngry(member.Actor);
                }
                await Task.CompletedTask;
            },
            isToy: true
        );

        Items["RAIN CLOUD"] = new Item(
            name: "RAIN CLOUD",
            description: "Angsty water droplets.\nInflicts SAD on all friends.",
            target: SkillTarget.AllAllies,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] uses RAIN CLOUD!");
                AudioManager.Instance.PlaySFX("BA_sad_level_2", 1, 0.9f);
                foreach (PartyMemberComponent member in BattleManager.Instance.GetAlivePartyMembers())
                {
                    MakeSad(member.Actor);
                }
                await Task.CompletedTask;
            },
            isToy: true
        );
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

        #region CHARMS
        // TODO: missing charms (special behavior/unused): sales tag, chef's chat, contract, abbi's eye, unused charms
        Charms["3-leaf Clover"] = new Charm("3-leaf Clover", new Stats(lck: 3));
        Charms["4-leaf Clover"] = new Charm("4-leaf Clover", new Stats(hp: 4, lck: 4));
        Charms["5-leaf Clover"] = new Charm("5-leaf Clover", () =>
        {
            return new Stats(lck: 2 + BattleManager.Instance.Energy);
        });
        Charms["Backpack"] = new Charm("Backpack", new Stats(def: 2));
        Charms["Baseball Cap"] = new Charm("Baseball Cap", new Stats(def: 10, spd: 15));
        Charms["Binoculars"] = new Charm("Binoculars", new Stats(def: 2, hit: 200));
        Charms["Blanket"] = new Charm("Blanket", new Stats(hp: 10, def: 1));
        Charms["Bow Tie"] = new Charm("Bow Tie", new Stats(def: 4));
        Charms["Bracelet"] = new Charm("Bracelet", new Stats(def: 1));
        Charms["Breadphones"] = new Charm("Breadphones", new Stats(hp: 10, def: 5));
        Charms["Bubble Wrap"] = new Charm("Bubble Wrap", new Stats(def: 3));
        Charms["Bunny Ears"] = new Charm("Bunny Ears", new Stats(def: 3, spd: 12));
        Charms["Cat Ears"] = new Charm("Cat Ears", new Stats(def: 1, spd: 10));
        Charms["Cellphone"] = new Charm("Cellphone", new Stats(def: 10));
        Charms["Cool Glasses"] = new Charm("Cool Glasses", new Stats(atk: 5, def: 5));
        Charms["Cough Mask"] = new Charm("Cough Mask", new Stats(25, 25, 10, 10, 10, 10));
        Charms["Daisy"] = new Charm("Daisy", new Stats(hp: 10), (actor) =>
        {
            actor.SetState("happy", true);
        });
        Charms["Eye Patch"] = new Charm("Eye Patch", new Stats(atk: 7, hit: -25));
        Charms["Faux Tail"] = new Charm("Faux Tail", new Stats(spd: 15));
        Charms["Fedora"] = new Charm("Fedora", new Stats(def: 5, lck: 5));
        Charms["Finger"] = new Charm("Finger", new Stats(atk: 10, def: -5), (actor) =>
        {
            actor.SetState("angry", true);
        });
        Charms["Fox Tail"] = new Charm("Fox Tail", () =>
        {
            return new Stats(spd: 5 + (3 * BattleManager.Instance.Energy));
        });
        Charms["Friendship Bracelet"] = new Charm("Friendship Bracelet", new Stats(10, 10));
        Charms["Nerdy Glasses"] = new Charm("Nerdy Glasses", new Stats(def: 5, hit: 200));
        Charms["Gold Watch"] = new Charm("Gold Watch", new Stats(spd: -10));
        Charms["Hard Hat"] = new Charm("Hard Hat", new Stats(def: 6));
        Charms["Headband"] = new Charm("Headband", new Stats(juice: 20, atk: 10, def: 3, spd: 15));
        Charms["Heart String"] = new Charm("Heart String", new Stats(hp: 30), (actor) =>
        {
            actor.SetState("happy", true);
        });
        Charms["High Heels"] = new Charm("High Heels", new Stats(atk: 10, spd: -10));
        Charms["Homework"] = new Charm("Homework", new Stats(), (actor) =>
        {
            actor.SetState("sad", true);
        });
        Charms["Inner Tube"] = new Charm("Inner Tube", () =>
        {
            return new Stats(def: 2 + BattleManager.Instance.Energy);
        });
        Charms["Magical Bean"] = new Charm("Magical Bean", new Stats(), (actor) =>
        {
            BattleManager.Instance.RandomEmotion(actor);
        });
        Charms["Onion Ring"] = new Charm("Onion Ring", new Stats(20, 20));
        Charms["Paper Bag"] = new Charm("Paper Bag", new Stats(hp: 40, def: 13));
        Charms["Hector"] = new Charm("Hector", new Stats());
        Charms["Pretty Bow"] = new Charm("Pretty Bow", new Stats(hp: 50, atk: 10, def: 3));
        Charms["Punching Bag"] = new Charm("Punching Bag", new Stats(), (actor) =>
        {
            actor.SetState("angry", true);
        });
        Charms["Rabbit Foot"] = new Charm("Rabbit Foot", new Stats(spd: 15, lck: 10));
        Charms["Red Ribbon"] = new Charm("Red Ribbon", () =>
        {
            return new Stats(atk: 1 + (2 * BattleManager.Instance.Energy), def: 5);
        });
        Charms["Deep Poetry Book"] = new Charm("Deep Poetry Book", new Stats(), (actor) =>
        {
            actor.SetState("sad", true);
        });
        Charms["Rubber Duck"] = new Charm("Rubber Duck", new Stats(def: 7));
        Charms["Seer Goggles"] = new Charm("Seer Goggles", new Stats(def: 1, lck: 3, hit: 200));
        Charms["Top Hat"] = new Charm("Top Hat", new Stats(hp: 13, def: 13, lck: 13));
        Charms["Hector Jr."] = new Charm("Hector Jr.", () =>
        {
            int energy = BattleManager.Instance.Energy;
            return new Stats(atk: 1 + energy, def: 1 + energy, spd: 1 + energy, lck: energy);
        });
        Charms["Wedding Ring"] = new Charm("Wedding Ring", new Stats(10, 10, 3, 3, 3, 3), (actor) =>
        {
            actor.SetState("happy", true);
        });
        Charms["Wishbone"] = new Charm("Wishbone", new Stats(lck: 7));
        Charms["Veggie Kid"] = new Charm("Veggie Kid", new Stats(15, 15));
        Charms["Watering Pail"] = new Charm("Watering Pail", new Stats(juice: 10));
        Charms["Sunscreen"] = new Charm("Sunscreen", new Stats(hp: 15));
        Charms["Rake"] = new Charm("Rake", new Stats(atk: 3));
        Charms["Scarf"] = new Charm("Scarf", new Stats(def: 3));
        Charms["Cotton Ball"] = new Charm("Cotton Ball", new Stats(def: 1, spd: 3));
        Charms["Flashlight"] = new Charm("Flashlight", new Stats(def: 4));
        Charms["Universal Remote"] = new Charm("Universal Remote", new Stats(10, 10, 5, 5, 5, 5));
        Charms["TV Remote"] = new Charm("TV Remote", new Stats(hp: 5, def: 2));
        Charms["Flower Crown"] = new Charm("Flower Crown", new Stats(100, 25));
        Charms["Tulip Hairstick"] = new Charm("Tulip Hairstick", new Stats(hp: 50));
        Charms["Gladiolus Hairband"] = new Charm("Gladiolus Hairband", new Stats(atk: 10, lck: 10, hit: 100));
        Charms["Cactus Hairclip"] = new Charm("Cactus Hairclip", new Stats(hp: 15, def: 15));
        Charms["Rose Hairclip"] = new Charm("Rose Hairclip", new Stats(15, 15, 5, 5, 5, 5, 100));
        Charms["Seashell Necklace"] = new Charm("Seashell Necklace", new Stats(hp: 25, juice: 25, def: 5));
        #endregion
    }

    /// <summary>
    /// Adds a snack that provides flat healing
    /// </summary>
    private static void AddSnack(string name, string description, int healing)
    {
        Items[name] = new Item(
            name: name.ToUpper(),
            description: description,
            target: SkillTarget.Ally,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, $"[actor] uses {name.ToUpper()}!");
                GameManager.Instance.AnimationManager.PlayAnimation(212, target, false);
                int heal = healing;
                if (BattleManager.Instance.GetAllPartyMembers().Any(x => x.Actor.Weapon.Name == "Frying Pan" || x.Actor.Weapon.Name == "Ol' Reliable"))
                    heal = (int)Math.Round(heal * 1.5f, MidpointRounding.AwayFromZero);
                target.Heal(heal);
                BattleManager.Instance.SpawnDamageNumber(heal, target.CenterPoint, DamageType.Heal);
                BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovered {heal} HEART!");
                await Task.CompletedTask;
            }
        );
    }

    /// <summary>
    /// Adds a snack that provides flat juice healing
    /// </summary>
    private static void AddJuiceSnack(string name, string description, int juice)
    {
        Items[name] = new Item(
            name: name.ToUpper(),
            description: description,
            target: SkillTarget.Ally,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, $"[actor] uses {name.ToUpper()}!");
                GameManager.Instance.AnimationManager.PlayAnimation(213, target, false);
                int total = juice;
                if (BattleManager.Instance.GetAllPartyMembers().Any(x => x.Actor.Weapon.Name == "Blender" || x.Actor.Weapon.Name == "Ol' Reliable"))
                    total = (int)Math.Round(total * 1.5f, MidpointRounding.AwayFromZero);
                target.HealJuice(total);
                BattleManager.Instance.SpawnDamageNumber(total, target.CenterPoint, DamageType.JuiceGain);
                BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovered {total} JUICE!");
                await Task.CompletedTask;
            }
        );
    }

    /// <summary>
    /// Adds a snack that provides percentage-based juice healing
    /// </summary>
    private static void AddJuiceSnack(string name, string description, float percentage)
    {
        Items[name] = new Item(
            name: name.ToUpper(),
            description: description,
            target: SkillTarget.Ally,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, $"[actor] uses {name.ToUpper()}!");
                GameManager.Instance.AnimationManager.PlayAnimation(213, target, false);
                float juice = target.CurrentStats.MaxJuice * percentage;
                if (BattleManager.Instance.GetAllPartyMembers().Any(x => x.Actor.Weapon.Name == "Blender" || x.Actor.Weapon.Name == "Ol' Reliable"))
                    juice *= 1.5f;
                int finalJuice = (int)Math.Round(juice, MidpointRounding.AwayFromZero);
                target.HealJuice(finalJuice);
                BattleManager.Instance.SpawnDamageNumber(finalJuice, target.CenterPoint, DamageType.JuiceGain);
                BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovered {finalJuice} JUICE!");
                await Task.CompletedTask;
            }
        );
    }

    /// <summary>
    /// Adds a snack that provides percentage-based healing
    /// </summary>
    private static void AddSnack(string name, string description, float percentage)
    {
        Items[name] = new Item(
            name: name.ToUpper(),
            description: description,
            target: SkillTarget.Ally,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, $"[actor] uses {name.ToUpper()}!");
                GameManager.Instance.AnimationManager.PlayAnimation(212, target, false);
                float heal = target.CurrentStats.MaxHP * percentage;
                if (BattleManager.Instance.GetAllPartyMembers().Any(x => x.Actor.Weapon.Name == "Frying Pan" || x.Actor.Weapon.Name == "Ol' Reliable"))
                    heal *= 1.5f;
                int finalHeal = (int)Math.Round(heal, MidpointRounding.AwayFromZero);
                target.Heal(finalHeal);
                BattleManager.Instance.SpawnDamageNumber(finalHeal, target.CenterPoint, DamageType.Heal);
                BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovered {finalHeal} HEART!");
                await Task.CompletedTask;
            }
        );
    }

    /// <summary>
    /// Adds a snack that provides flat healing to all allies
    /// </summary>
    private static void AddGroupSnack(string name, string description, int healing)
    {
        Items[name] = new Item(
           name: name.ToUpper(),
           description: description,
           target: SkillTarget.AllAllies,
           effect: async (self, target) =>
           {
               BattleLogManager.Instance.QueueMessage(self, target, $"[actor] uses {name.ToUpper()}!");
               int heal = healing;
               if (BattleManager.Instance.GetAllPartyMembers().Any(x => x.Actor.Weapon.Name == "Frying Pan" || x.Actor.Weapon.Name == "Ol' Reliable"))
                   heal = (int)Math.Round(heal * 1.5f, MidpointRounding.AwayFromZero);
               foreach (PartyMemberComponent member in BattleManager.Instance.GetAlivePartyMembers())
               {
                   GameManager.Instance.AnimationManager.PlayAnimation(212, member.Actor, false);
                   member.Actor.Heal(heal);
                   BattleManager.Instance.SpawnDamageNumber(heal, member.Actor.CenterPoint, DamageType.Heal);
               }
               BattleLogManager.Instance.QueueMessage($"Everyone recovered {heal} HEART!");
               await Task.CompletedTask;
           }
       );
    }

    /// <summary>
    /// Adds a snack that provides flat juice healing to all allies
    /// </summary>
    private static void AddGroupJuiceSnack(string name, string description, int juice)
    {
        Items[name] = new Item(
           name: name.ToUpper(),
           description: description,
           target: SkillTarget.AllAllies,
           effect: async (self, target) =>
           {
               BattleLogManager.Instance.QueueMessage(self, target, $"[actor] uses {name.ToUpper()}!");
               int total = juice;
               if (BattleManager.Instance.GetAllPartyMembers().Any(x => x.Actor.Weapon.Name == "Blender" || x.Actor.Weapon.Name == "Ol' Reliable"))
                   total = (int)Math.Round(total * 1.5f, MidpointRounding.AwayFromZero);
               foreach (PartyMemberComponent member in BattleManager.Instance.GetAlivePartyMembers())
               {
                   GameManager.Instance.AnimationManager.PlayAnimation(213, member.Actor, false);
                   member.Actor.HealJuice(total);
                   BattleManager.Instance.SpawnDamageNumber(total, member.Actor.CenterPoint, DamageType.JuiceGain);
               }
               BattleLogManager.Instance.QueueMessage($"Everyone recovered {total} JUICE!");
               await Task.CompletedTask;
           }
       );
    }

    /// <summary>
    /// A snack that provides flat healing and juice
    /// </summary>
    private static void AddComboSnack(string name, string description, int healing, int juice)
    {
        Items[name] = new Item(
            name: name.ToUpper(),
            description: description,
            target: SkillTarget.Ally,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, $"[actor] uses {name.ToUpper()}!");
                GameManager.Instance.AnimationManager.PlayAnimation(212, target, false);
                int heal = healing;
                int total = juice;
                if (BattleManager.Instance.GetAllPartyMembers().Any(x => x.Actor.Weapon.Name == "Frying Pan" || x.Actor.Weapon.Name == "Ol' Reliable"))
                    heal = (int)Math.Round(heal * 1.5f, MidpointRounding.AwayFromZero);
                // donald compiler please come save us donald compiler please save us
                if (BattleManager.Instance.GetAllPartyMembers().Any(x => x.Actor.Weapon.Name == "Blender" || x.Actor.Weapon.Name == "Ol' Reliable"))
                    total = (int)Math.Round(total * 1.5f, MidpointRounding.AwayFromZero);
                target.Heal(heal);
                target.HealJuice(total);
                BattleManager.Instance.SpawnDamageNumber(heal, target.CenterPoint, DamageType.Heal);
                BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovered {heal} HEART!");
                BattleLogManager.Instance.QueueMessage(self, target, $"[target] recovered {total} JUICE!");
                await Task.CompletedTask;
            }
        );
    }

    private static void MakeSad(Actor who)
    {
        string state = "sad";
        switch (who.CurrentState)
        {
            case "miserable":
                BattleLogManager.Instance.QueueMessage(null, who, "[target] cannot be any sadder!");
                return;
            case "depressed":
                state = "miserable";
                break;
            case "sad":
                state = "depressed";
                break;
        }
        if (who.IsStateValid(state))
            who.SetState(state);
        else
            BattleLogManager.Instance.QueueMessage(null, who, "[target] cannot be any sadder!");
    }

    private static void MakeHappy(Actor who)
    {
        string state = "happy";
        switch (who.CurrentState)
        {
            case "manic":
                BattleLogManager.Instance.QueueMessage(null, who, "[target] cannot be any happier!");
                return;
            case "ecstatic":
                state = "manic";
                break;
            case "happy":
                state = "ecstatic";
                break;
        }
        if (who.IsStateValid(state))
            who.SetState(state);
        else
            BattleLogManager.Instance.QueueMessage(null, who, "[target] cannot be any happier!");
    }

    private static void MakeAngry(Actor who)
    {
        string state = "angry";
        switch (who.CurrentState)
        {
            case "furious":
                BattleLogManager.Instance.QueueMessage(null, who, "[target] cannot be any angrier!");
                return;
            case "enraged":
                state = "furious";
                break;
            case "angry":
                state = "enraged";
                break;
        }
        if (who.IsStateValid(state))
            who.SetState(state);
        else
            BattleLogManager.Instance.QueueMessage(null, who, "[target] cannot be any angrier!");
    }
}