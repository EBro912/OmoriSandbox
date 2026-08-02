using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Animation;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;
using OmoriSandbox.Extensions;

namespace OmoriSandbox.Actors;

internal sealed class HumphreyGrande : Enemy
{
    public override string Name => "HUMPHREY GRANDE";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/humphrey_grande.tres");
    protected override Stats Stats => new(3700, 425, 68, 30, 1, 10, 95);
    protected override string[] EquippedSkills => ["HUGAttack"];
    
    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "sad" or "happy" or "angry";
    }
    
    public override BattleCommand ProcessAI()
    {
        return new BattleCommand(this, SelectTarget(), Skills["HUGAttack"]);
    }

    public override async Task ProcessBattleConditions()
    {
        if (CurrentHP < 370)
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
            BattleManager.Instance.TransformEnemy(this, "HumphreyFace", offset: new Vector2(0, -60));
            await Wait.Milliseconds(2000);
            await AnimationManager.Instance.WaitForTintScreen(ColorsExtension.TransparentBlack, 1f);
        }
    }
}