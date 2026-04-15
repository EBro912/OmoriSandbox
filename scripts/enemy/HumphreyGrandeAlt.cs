using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Animation;
using OmoriSandbox.Battle;
using OmoriSandbox.Extensions;

namespace OmoriSandbox.Actors;

internal sealed class HumphreyGrandeAlt : Enemy
{
    public override string Name => "HUMPHREY GRANDE";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/humphrey_grande.tres");
    protected override Stats Stats => new(9000, 4000, 100, 25, 1, 10, 95);
    protected override string[] EquippedSkills => ["HUGAttack"];
    
    public override bool IsStateValid(string state)
    {
        return state is "neutral" or "sad" or "happy" or "angry" or "toast";
    }
    
    public override BattleCommand ProcessAI()
    {
        return new BattleCommand(this, SelectTarget(), Skills["HUGAttack"]);
    }

    public override async Task ProcessBattleConditions()
    {
        if (CurrentHP < 900)
        {
            DialogueManager.Instance.QueueMessage("HUMPHREY", CenterPoint, @"[wave freq=10.0]Just a warning... it's about to get smelly!\| It's time for you all to get in my belly![/wave]");
            await DialogueManager.Instance.WaitForDialogue();
            await AnimationManager.Instance.WaitForTintScreen(Colors.Black, 0.5f);
            await Wait.Milliseconds(1000);
            AnimationManager.Instance.TintScreen(ColorsExtension.TransparentBlack, 0.1f);
            await AnimationManager.Instance.WaitForHumphreySwallow();
            AnimationManager.Instance.TintScreen(Colors.Black);
            BattleLogManager.Instance.ClearBattleLog();
            foreach (PartyMember member in SelectAllTargets())
                BattleManager.Instance.Damage(this, member, () => member.CurrentStats.MaxHP * 0.25f, true, 0.5f, neverCrit: true);
            BattleManager.Instance.TransformEnemy(this, "HumphreyFace (Boss Rush)");
            await Wait.Milliseconds(2000);
            await AnimationManager.Instance.WaitForTintScreen(ColorsExtension.TransparentBlack, 1f);
        }
    }
}