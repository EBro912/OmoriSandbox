using Godot;
using OmoriSandbox;
using OmoriSandbox.Animation;
using OmoriSandbox.Battle;
using OmoriSandbox.Actors;
using OmoriSandbox.Modding;

namespace AquaMod;

public partial class AquaMod : Mod
{
    public override void OnLoad()
    {
        RegisterEnemy<Aqua>("Aqua");
        
        RegisterSkill("AQKnifeChain", new Skill(
            name: "AQKnifeChain",
            description: "AQKnifeChain",
            target: SkillTarget.Enemy,
            cost: 0,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, "[actor] throws a chain of knives!");
                if (self is Aqua)
                    self.PlayAnimation("chain");
                Tween tween;
                for (int i = 0; i < 6; i++)
                {
                    tween = CreateTween();
                    tween.TweenProperty(self.Sprite, "offset",
                        new Vector2(GameManager.Instance.Random.RandiRange(-50, 50),
                            GameManager.Instance.Random.RandiRange(-50, 50)), 0.2f).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
                    await AnimationManager.Instance.WaitForAnimation(20, target);
                    BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 5 - target.CurrentStats.DEF, false, 0.1f, neverCrit: true);
                }
                tween = CreateTween();
                tween.TweenProperty(self.Sprite, "offset", Vector2.Zero, 0.2f).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
                await ToSignal(tween, Tween.SignalName.Finished);
                self.ClearAnimation();
            }
        ));
        
        RegisterSkill("AQKnifeFan", new Skill(
            name: "AQKnifeFan",
            description: "AQKnifeFan",
            target: SkillTarget.Enemy,
            cost: 0,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, "[actor] throws a fan of knives!");
                await Wait.Milliseconds(250);
                await AnimationManager.Instance.WaitForAnimation(6, target);
                for (int i = 0; i < 4; i++)
                {
                    BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 5 - target.CurrentStats.DEF, false, 0.1f, neverCrit: true);
                }
            }
        ));
        
        RegisterSkill("AQKnifeCircle", new Skill(
            name: "AQKnifeCircle",
            description: "AQKnifeCircle",
            target: SkillTarget.Enemy,
            cost: 0,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, "[actor] throws knives in a circle!");
                await Wait.Milliseconds(500);
                for (int i = 0; i < 12; i++)
                {
                    await AnimationManager.Instance.WaitForAnimation(i % 2 == 0 ? 9 : 20, target);
                    BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 5 - target.CurrentStats.DEF, false, 0.1f, neverCrit: true);
                }
            }
        ));
        
        RegisterSkill("AQOmega", new Skill(
            name: "AQOmega",
            description: "AQOmega",
            target: SkillTarget.AllEnemies,
            cost: 0,
            effect: async (self, targets) =>
            {
                BattleLogManager.Instance.QueueMessage(self, "[actor] uses her OMEGA attack!");
                await Wait.Milliseconds(1000);
                foreach (Actor target in targets)
                    AnimationManager.Instance.PlayAnimation(13, target);
                for (int i = 0; i < 10; i++)
                {
                    foreach (Actor target in targets)
                        BattleManager.Instance.Damage(self, target, () => self.CurrentStats.ATK * 5 - target.CurrentStats.DEF, false, 0.1f, neverCrit: true);
                    await Wait.Milliseconds(350);
                }
            }
        ));

        RegisterSkill("CoolPose", new Skill(
            name: "COOLPOSE",
            description: "25% mercy",
            target: SkillTarget.Enemy,
            cost: 0,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, "[actor] struck a cool pose!");
                AnimationManager.Instance.PlayAnimation(148, self);
                if (target is Aqua enemy)
                {
                    enemy.AddMercy("pose");
                    target.PlayAnimation("pose");
                    await Wait.Milliseconds(1000);
                    BattleLogManager.Instance.QueueMessage("... the enemy posed back!");
                    await Wait.Milliseconds(1000);
                    await enemy.DoACTDialogue("pose");
                    target.ClearAnimation();
                }
                else
                {
                    BattleLogManager.Instance.QueueMessage("...but nothing happened.");
                }
            }
        ));
        
        RegisterSkill("Spin", new Skill(
            name: "SPIN",
            description: "25% mercy",
            target: SkillTarget.Enemy,
            cost: 0,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, "[actor] spun in place!");
                AnimationManager.Instance.PlayAnimation(72, self);
                if (target is Aqua enemy)
                {
                    enemy.AddMercy("spin");
                    target.PlayAnimation("spin");
                    await Wait.Milliseconds(1000);
                    BattleLogManager.Instance.QueueMessage("... the enemy spun around, too!");
                    await Wait.Milliseconds(1000);
                    await enemy.DoACTDialogue("spin");
                    target.ClearAnimation();
                }
                else
                {
                    BattleLogManager.Instance.QueueMessage("...but nothing happened.");
                }
            }
        ));
        
        RegisterSkill("Care", new Skill(
            name: "CARE",
            description: "25% mercy",
            target: SkillTarget.Enemy,
            cost: 0,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, "[actor] showed tender loving care!");
                AnimationManager.Instance.PlayAnimation(212, self);
                if (target is Aqua enemy)
                {
                    enemy.AddMercy("laugh");
                    target.PlayAnimation("laugh");
                    await Wait.Milliseconds(1000);
                    BattleLogManager.Instance.QueueMessage("... the enemy laughed!");
                    await Wait.Milliseconds(1000);
                    await enemy.DoACTDialogue("care");
                    target.ClearAnimation();
                }
                else
                {
                    BattleLogManager.Instance.QueueMessage("...but nothing happened.");
                }
            }
        ));
        
        RegisterSkill("Dance", new Skill(
            name: "DANCE",
            description: "25% mercy",
            target: SkillTarget.Enemy,
            cost: 0,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, "[actor] danced with enthusiasm!");
                AudioManager.Instance.PlaySFX("BA_do_nothing_dance", 1, 0.9f);
                if (target is Aqua enemy)
                {
                    enemy.AddMercy("dance");
                    target.PlayAnimation("dance");
                    await Wait.Milliseconds(1000);
                    BattleLogManager.Instance.QueueMessage("... the enemy danced, too!");
                    await Wait.Milliseconds(1000);
                    await enemy.DoACTDialogue("dance");
                    target.ClearAnimation();
                }
                else
                {
                    BattleLogManager.Instance.QueueMessage("...but nothing happened.");
                }
            }
        ));
        
        GD.Print("AquaMod loaded!");
    }
}