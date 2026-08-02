using System.Threading.Tasks;
using Godot;
using OmoriSandbox;
using OmoriSandbox.Animation;
using OmoriSandbox.Battle;
using OmoriSandbox.Editor;
using OmoriSandbox.Modding;

namespace ExampleItemsMod;

public partial class ExampleItemsMod : Mod
{
    public override void OnLoad()
    {
        // a custom stat modifier. the state icon is loaded from this mod's stateicons folder
        // and referenced by file base name
        RegisterStatModifier("Buttered", () => new ButteredStatModifier()
            .WithStateIcons(new StateIcon("bnw_butter", "Buttered\nSPD boost, recovers HEART each turn,\nincoming attacks slide off.")));

        // a snack, using an icon from the vanilla consumables sheet (Bread's icon)
        RegisterItem("Burnt Toast", new Item(
            name: "BURNT TOAST",
            description: "A slice of life, left in too long.\nHeals 1 HEART.",
            target: SkillTarget.Ally,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] uses BURNT TOAST!");
                AnimationManager.Instance.PlayAnimation(212, target);
                target.Heal(1);
                BattleManager.Instance.SpawnDamageNumber(1, target.CenterPoint, DamageType.Heal);
                BattleLogManager.Instance.QueueMessage(self, target, "It's... very burnt.\n[target] recovered 1 HEART!");
                await Task.CompletedTask;
            },
            spritesheetPath: "res://assets/system/itemConsumables.png",
            spriteIndex: 68
        ));

        // an item without icon data falls back to text-only info in the item menu
        RegisterItem("Crumbs", new Item(
            name: "CRUMBS",
            description: "Just crumbs from the bottom of the bag.\nHeals 1 JUICE.",
            target: SkillTarget.Ally,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] eats some CRUMBS!");
                target.HealJuice(1);
                BattleManager.Instance.SpawnDamageNumber(1, target.CenterPoint, DamageType.JuiceGain);
                await Task.CompletedTask;
            }
        ));

        // a toy. fixed damage that respects the "toys use emotion damage" setting,
        // like the vanilla rubber bands
        RegisterItem("Bigger Rubber Band", new Item(
            name: "BIGGER RUBBER BAND",
            description: "Deals even bigger damage to a foe and\ngreatly reduces their DEFENSE.",
            target: SkillTarget.Enemy,
            effect: async (self, target) =>
            {
                BattleLogManager.Instance.QueueMessage(self, target, "[actor] uses BIGGER RUBBER BAND!");
                BattleManager.Instance.Damage(self, target, () => 300, true, 0, neverCrit: true,
                    ignoreEmotion: !SettingsMenuManager.Instance.ToysUseEmotionDamage);
                await AnimationManager.Instance.WaitForAnimation(219, target);
                target.AddTierStatModifier("DefenseDown", 2);
            },
            isToy: true,
            spritesheetPath: "res://assets/system/itemConsumables.png",
            spriteIndex: 69
        ));

        // a weapon that reapplies the custom modifier every turn
        RegisterEquipment("Butter Knife", new Equipment("Butter Knife",
            [new StatBonus(StatType.ATK, 6), new StatBonus(StatType.HIT, 100)])
            .WithStartOfTurnEffect(actor =>
            {
                actor.AddStatModifier("Buttered", silent: true);
                return Task.CompletedTask;
            }));

        // a charm with a dynamic stat bonus (scales with the energy bar, like 5-leaf Clover)
        // and a start-of-battle emotion (like Daisy)
        RegisterEquipment("6-leaf Clover", new Equipment("6-leaf Clover", true)
            .WithApplyEffect(() => [new StatBonus(StatType.LCK, 3 + BattleManager.Instance.Energy)])
            .WithStartOfBattleEffect(actor =>
            {
                actor.SetEmotion("happy", true);
                return Task.CompletedTask;
            }));

        GD.Print("ExampleItemsMod loaded!");
    }
}
