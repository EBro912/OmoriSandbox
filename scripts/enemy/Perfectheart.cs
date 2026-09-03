using Godot;
using System.Threading.Tasks;

using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;
using OmoriSandbox.Animation;

namespace OmoriSandbox.Actors;
internal sealed class Perfectheart : Enemy
{
    public override string Name => "PERFECTHEART";
    public override Vector2 InfoBoxOffset => new(0, -165);
    public override bool InfoBoxCursorAboveBox => true;
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/perfectheart.tres");
    protected override Stats Stats => new(10000, 5000, 140, 140, 140, 15, 1000);

    protected override string[] EquippedSkills => ["PHStealHeart", "PHStealBreath", "PHWrath", "PHExploitEmotion", "PHSpare", "PHAngelicVoice"];
    internal override bool ObserveHasMulti => true;

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "sad" or "happy" or "angry";
    }

    private bool SecondPhase = false;
    private bool HasSpoken = false;

    private int Variable1001 = 0;
    private Sprite2D OverlaySprite = null;

    public override BattleCommand ProcessAI()
    {
        if (HasMultiTargetObserve())
        {
            if (!PreRolling)
                Variable1001 = 1;
            return new BattleCommand(this, SelectAllTargets(), Skills["PHWrath"]);
        }

        if (HasObserveTarget(out PartyMember observe))
        {
            if (!PreRolling)
                Variable1001 = 2;
            return new BattleCommand(this, observe, Skills["PHStealHeart"]);
        }

        if (Variable1001 == 1)
            return new BattleCommand(this, SelectAllTargets(), Skills["PHWrath"]);
        if (Roll() < 36)
            return new BattleCommand(this, SelectTarget(), Skills["PHStealHeart"]);
        if (Roll() < 26)
            return new BattleCommand(this, SelectTarget(), Skills["PHStealBreath"]);
        if (Roll() < 36)
            return new BattleCommand(this, SelectAllTargets(), Skills["PHAngelicVoice"]);
        if (Roll() < 46)
            return new BattleCommand(this, SelectTarget(), Skills["PHExploitEmotion"]);
        return new BattleCommand(this, SelectTarget(), Skills["PHSpare"]);
    }

    public override async Task ProcessBattleConditions()
    {
        // recreate omori bug where phase 2 is skipped if perfectheart is observed
        if (Variable1001 == 0 && IsBelowHP(0.35f) && !SecondPhase)
        {
            DialogueManager.Instance.QueueMessage(this, @"Oh...\! You are quite strong.");
            DialogueManager.Instance.QueueMessage(this, "It seems I must try a bit harder.");
            await DialogueManager.Instance.WaitForDialogue();
            AudioManager.Instance.PlaySFX("GEN_shine", 0.5f, 0.9f);
            OverlaySprite = AnimationManager.Instance.SpawnPerfectheartOverlay(new Vector2(CenterPoint.X, CenterPoint.Y - 45));
            await Wait.Milliseconds(2000);
            AnimationManager.Instance.PlayAnimation(216, this);
            CurrentHP = CurrentStats.MaxHP;
            SetEmotion("neutral", true);
            RemoveAllStatModifiers();
            await Wait.Milliseconds(1000);
            SecondPhase = true;
            Variable1001 = 1;
        }
        
        if (CurrentHP > 0 && IsBelowHP(0.25f) && !HasSpoken)
        {
            DialogueManager.Instance.QueueMessage(this, @"Hm?\! W-What's this?\! A drop of sweat?");
            DialogueManager.Instance.QueueMessage(this, @"My, my...\! I cannot believe this.");
            await DialogueManager.Instance.WaitForDialogue();
            HasSpoken = true;
        }
    }

    public override async Task OnDefeat()
    {
        OverlaySprite?.QueueFree();
        DialogueManager.Instance.QueueMessage(this, @"Ah.\! You have bested me.");
        DialogueManager.Instance.QueueMessage(this, @"Right, then.\! I know when to admit defeat.");
        await DialogueManager.Instance.WaitForDialogue();
    }

    public override async Task OnStartOfBattle()
    {
        DialogueManager.Instance.QueueMessage(this, @"[br]Remember, children...\! You brought this upon yourselves!");
        await DialogueManager.Instance.WaitForDialogue();
    }

    public override async Task OnEndOfBattle(bool victory)
    {
        if (!victory)
        {
            DialogueManager.Instance.QueueMessage(this, "I said that you would regret this, children.");
            DialogueManager.Instance.QueueMessage(this, "Don't make me do this again.");
            await DialogueManager.Instance.WaitForDialogue();
        }
    }
}
