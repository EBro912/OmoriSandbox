using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Animation;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;
using OmoriSandbox.Extensions;

namespace OmoriSandbox.Actors;

internal sealed class FearOfDrowning : Enemy
{
    public override string Name => "SOMETHING";
    public override Vector2 InfoBoxOffset => new(0, -350);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>($"res://animations/fear_of_drowning_{Phase}.tres");
    protected override Stats Stats => new(10300, 0, 84, 84, 70, 10, 95);
    protected override string[] EquippedSkills => ["FODAttack", "FODDoNothing", "FODDragDown", "FODWhirlpool", "FODDrowning1", "FODDrowning2", "FODDrowning3"];

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id == "neutral";
    }

    private int Phase = 1;

    public override BattleCommand ProcessAI()
    {
        if (Roll() < 41)
            return new BattleCommand(this, SelectTarget(), Skills["FODAttack"]);
        if (Roll() < 26)
            return new BattleCommand(this, SelectTarget(), Skills["FODDoNothing"]);
        if (Roll() < 41)
            return new BattleCommand(this, SelectTarget(), Skills["FODDragDown"]);
        return new BattleCommand(this, SelectAllTargets(), Skills["FODWhirlpool"]);
    }

    public override async Task OnStartOfBattle()
    {
        DialogueManager.Instance.QueueMessage("The room fills with water.");
        await DialogueManager.Instance.WaitForDialogue();
    }

    public override Task ProcessEndOfTurn()
    {
        BattleManager.Instance.ForceCommand(this, SelectAllTargets(), Skills["FODWhirlpool"]);
        BattleManager.Instance.ForceCommand(this, SelectAllTargets(), Skills[$"FODDrowning{Phase}"]);
        return Task.CompletedTask;
    }

    public override async Task ProcessBattleConditions()
    {
        if (IsBelowHP(0.7f) && Phase == 1)
        {
            Phase = 2;
            await AnimationManager.Instance.WaitForTintScreen(Colors.Black, 1f);
            UpdateSprite();
            await AnimationManager.Instance.WaitForTintScreen(ColorsExtension.TransparentBlack, 1f);
        }

        if (IsBelowHP(0.3f) && Phase == 2)
        {
            Phase = 3;
            await AnimationManager.Instance.WaitForTintScreen(Colors.Black, 1f);
            UpdateSprite();
            await AnimationManager.Instance.WaitForTintScreen(ColorsExtension.TransparentBlack, 1f);
        }
    }

    private void UpdateSprite()
    {
        // change sprite on phase change
        Sprite.SpriteFrames = Animation;
        Sprite.Animation = "neutral";
        Sprite.Play();
    }
}