using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Animation;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class LifeJamGuyAlt : Enemy
{
    public override string Name => "LIFE JAM GUY";
    public override Vector2 InfoBoxOffset => new(0, -350);
    public override Vector2 DamageNumberOffset => new(0, -75);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/life_jam_guy.tres");
    protected override Stats Stats => new(9999, 0, 1, 999, 1, 10, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["LJGDoNothing"];

    public override BattleCommand ProcessAI()
    {
        return new BattleCommand(this, this, Skills["LJGDoNothing"]);
    }

    public override Task OnStartOfBattle()
    {
        PartyMember slotZero = BattleManager.Instance.GetPartyMemberAtPosition(0);
        if (slotZero != null && !slotZero.IsToast)
        {
            slotZero.AddStatModifier("Immortal", silent: true);
            slotZero.ClearResult();
        }
        return Task.CompletedTask;
    }

    public override async Task ProcessEndOfTurn()
    {
        if (BattleManager.Instance.AbortRequested)
            return;
        int turn = BattleManager.Instance.TurnCount;
        if (turn is < 1 or > 3)
            return;
        AudioManager.Instance.PlaySFX("BA_BED", 1f, 0.9f);
        AnimationManager.Instance.InitShake(new Shake(9, 5, 60));
        await Wait.Frames(60);
        DialogueManager.Instance.QueueMessage("LIFE JAM GUY spilled some [color=#5bd863]LIFE JAM[/color]!");
        await DialogueManager.Instance.WaitForDialogue();
        BattleManager.Instance.AddItem("Life Jam", 1);
        await Wait.Frames(20);
        DialogueManager.Instance.QueueMessage("You got [color=#5bd863]LIFE JAM[/color]!");
        await DialogueManager.Instance.WaitForDialogue();
        AudioManager.Instance.PlaySFX("SE_NewSkill", 1f, 0.9f);
        await Wait.Frames(60);
        DialogueManager.Instance.QueueMessage(this, @"Use [color=#5bd863]LIFE JAM[/color]!\![br]It really works!");
        await DialogueManager.Instance.WaitForDialogue();
        await Wait.Frames(turn == 3 ? 60 : 20);
        if (turn != 3)
            return;
        DialogueManager.Instance.QueueMessage(this, @"Enjoy your free samples!\! Goodbye now, haha!");
        await DialogueManager.Instance.WaitForDialogue();
        await Wait.Frames(20);
        await AnimationManager.Instance.WaitForTintOverlay(Colors.Black, 24 / 60f, 22);
        await Wait.Frames(60);
        if (!await AbortBattle())
            await BattleManager.Instance.RestoreAfterScriptedEnding();
    }
}
