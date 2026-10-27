using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class DorothiTrain : Enemy
{
    public override string Name => "???";
    public override Vector2 InfoBoxOffset => new(0, -135);
    public override Vector2 DamageNumberOffset => new(0, -140);
    public override bool InfoBoxCursorAboveBox => true;
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/dorothi.tres");
    protected override Stats Stats => new(450, 50, 22, 10, 22, 13, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "afraid";
    }

    protected override string[] EquippedSkills => ["DORAttack", "DRTLiar", "DORAttackTwice"];

    private bool Fell;

    public override Task OnStartOfBattle()
    {
        AddImmortal();
        return Task.CompletedTask;
    }

    public override async Task ProcessStartOfCommands()
    {
        switch (BattleManager.Instance.TurnCount)
        {
            case 1:
                DialogueManager.Instance.QueueMessage(this, "SUNNY...", font: DialogueManager.FontType.Jagged);
                await DialogueManager.Instance.WaitForDialogue();
                break;
            case 2:
                DialogueManager.Instance.QueueMessage(this, @"SUNNY...\| SUNNY...\| SUNNY...", font: DialogueManager.FontType.Jagged);
                await DialogueManager.Instance.WaitForDialogue();
                break;
        }
    }

    public override async Task ProcessBattleConditions()
    {
        if (Fell || !IsBelowHP(0.1f))
            return;
        Fell = true;
        DialogueManager.Instance.QueueMessage(this, @"SUNNY...\| I'm...\| sorry...", font: DialogueManager.FontType.Jagged);
        await DialogueManager.Instance.WaitForDialogue();
        await AbortBattle();
    }

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["DORAttack"]);

        if (Roll() < 66)
            goto attack;
        if (Roll() < 36)
            goto liar;
        goto attackTwice;
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["DORAttack"]);
    liar:
        return new BattleCommand(this, SelectTarget(), Skills["DRTLiar"]);
    attackTwice:
        return new BattleCommand(this, SelectTargets(2), Skills["DORAttackTwice"]);
    }
}
