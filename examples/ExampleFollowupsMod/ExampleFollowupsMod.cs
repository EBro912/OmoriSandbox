using System.Collections.Generic;
using Godot;
using OmoriSandbox;
using OmoriSandbox.Actors;
using OmoriSandbox.Animation;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;
using OmoriSandbox.Modding;

namespace ExampleFollowupsMod;

public partial class ExampleFollowupsMod : Mod
{
    public override void OnLoad()
    {
        // groups must be registered before the emotions that use them.
        // SMUG is a sideways variant of the happy family, aka its own group that beats happy.
        // to instead extend a vanilla ladder, register only an emotion with .WithGroup("happy", 4).
        RegisterEmotionGroup(new EmotionGroup("smug")
            .WithBeatsGroup("happy")
            .WithMaxTierMessage("[target] can't get any SMUGGER!"));

        RegisterEmotion(new Emotion("smug")
            .WithGroup("smug", 1)
            // vanilla party members have no "smug" sprite animation, so reuse "happy"
            .WithAnimationName("happy")
            // float bonuses are multipliers, int bonuses are flat (like vanilla happy)
            .WithStatBonuses(new StatBonus(StatType.LCK, 2f), new StatBonus(StatType.SPD, 1.3f),
                new StatBonus(StatType.HIT, -5))
            // optional damage rate override for certain element ids
            .WithDefensiveRate("angry", 1.5f)
            // reuse the vanilla ECSTATIC label and face graphics
            .WithAsset(EmotionAsset.Vanilla(4, 3, 0)));
            // for modded graphics, use FromModTextures to specify the above head and back graphic
            // .WithAsset(EmotionAsset.FromModTextures("MyMod/sprites/smug_above.png", "MyMod/sprites/smug_back.png")));
            // or take regions of a single spritesheet:
            // .WithAsset(EmotionAsset.FromModTextures(
            //     "MyMod/sprites/emotions.png", new Rect2(0, 0, 98, 22),
            //     "MyMod/sprites/emotions.png", new Rect2(0, 22, 100, 100))));

        RegisterSkill("ExAttackAgain", new Skill(
            name: "Attack Again EX",
            description: "Omori Followup",
            target: SkillTarget.Enemy,
            cost: 0,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] readies his blade with a smirk.");
                await Wait.Milliseconds(1000);
                BattleLogManager.Instance.ClearBattleLog();
                await AnimationManager.Instance.WaitForAnimation(3, target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] attacks again!");
                BattleManager.Instance.Damage(self, target,
                    () => (self.CurrentStats.ATK * 2) + self.CurrentStats.LCK - target.CurrentStats.DEF, false);
            },
            hidden: true
        ).WithCustomRequirement((_) => true));

        RegisterSkill("ExTrip", new Skill(
            name: "Trip EX",
            description: "Omori Followup",
            target: SkillTarget.Enemy,
            cost: 0,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] strolls forward.");
                await Wait.Milliseconds(1000);
                BattleLogManager.Instance.ClearBattleLog();
                await AnimationManager.Instance.WaitForAnimation(14, target);
                AnimationManager.Instance.PlayAnimation(219, target);
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] trips [target]!");
                target.AddStatModifier("SpeedDown");
                target.SetEmotion("sad");
                BattleManager.Instance.Damage(self, target,
                    () => self.CurrentStats.ATK + self.CurrentStats.LCK - target.CurrentStats.DEF, false);
            },
            hidden: true
        ).WithCustomRequirement((_) => true));

        // a custom release energy. this will use 10 energy instead of 3.
        RegisterSkill("ReleaseEnergyEx", new Skill(
            name: "Release Energy EX",
            description: "Omori Followup",
            target: SkillTarget.AllEnemies,
            cost: 0,
            effect: async (self, targets) =>
            {
                BattleLogManager.Instance.QueueMessage(self,
                    "[actor] and friends come together and\nuse their ultimate attack!");
                foreach (PartyMemberComponent member in BattleManager.Instance.GetAlivePartyMembers())
                    AnimationManager.Instance.PlayAnimation(243, member.Actor);

                await AnimationManager.Instance.WaitForReleaseEnergy();
                BattleLogManager.Instance.ClearBattleLog();
                await AnimationManager.Instance.WaitForScreenAnimation(15, true);
                foreach (Actor enemy in targets)
                    BattleManager.Instance.Damage(self, enemy, () => 450, true, 0f, false, true);

                // everyone gets the vanilla release energy buff and feels SMUG
                foreach (PartyMemberComponent member in BattleManager.Instance.GetAlivePartyMembers())
                {
                    member.Actor.AddStatModifier("ReleaseEnergy");
                    member.Actor.SetEmotion("smug", true);
                }
            },
            hidden: true
        ).WithCustomRequirement((_) => true));

        // load vanilla graphics
        // modded textures would use .FromModTexture instead, as shown below
        Texture2D bubbles = ResourceLoader.Load<Texture2D>("res://assets/system/ACS_Bubble.png");
        RegisterFollowupSet("OmoriEx", new Dictionary<FollowupInput, FollowupEntry>
        {
            // theoretical custom texture entry (leave commented)
            // { FollowupInput.Up, FollowupEntry.FromModTexture(0, "ExAttackAgain", "MyMod/sprites/bubble_up.png")},
            { FollowupInput.Up, FollowupEntry.FromTexture(0, "ExAttackAgain", bubbles, new Rect2(0, -1, 127, 99)) },
            { FollowupInput.Horizontal, FollowupEntry.FromTexture(0, "ExTrip", bubbles, new Rect2(127, 0, 127, 99)) },
            { FollowupInput.Down, FollowupEntry.FromTexture(0, "ReleaseEnergyEx", bubbles, new Rect2(258, 0, 127, 99)) },
        }, tiered: false);

        GD.Print("ExampleFollowupsMod loaded!");
    }
}
