using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Animation;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class HangingBodyMari : Enemy
{
    internal int TroopStep { get; set; }
    internal int AfraidLines { get; set; }

    public override string Name => "HANGING BODY";
    public override Vector2 InfoBoxOffset => new(0, -300);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/omori_something_hanging_body.tres");
    protected override Stats Stats => new(9999, 5000, 10, 999, 10, 10, 1000);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral";
    }

    protected override string[] EquippedSkills => ["OSBBlurryNothing"];

    public override Task OnSpawned()
    {
        AddStatModifier("Immune", silent: true);
        return Task.CompletedTask;
    }

    public override async Task ProcessEndOfTurn()
    {
        await RunCounterPage();
    }

    internal async Task RunCounterPage()
    {
        if (BattleManager.Instance.AbortRequested)
            return;
        if (TroopStep >= 1)
        {
            if (AfraidLines == 0)
                AfraidLines++;
            else if (AfraidLines != 4)
            {
                AfraidLines++;
                await ShowAfraidLine();
            }
            if (TroopStep == 5)
                await SimulationMari();
            else
                TroopStep++;
        }
    }

    private async Task ShowAfraidLine()
    {
        string name = BattleManager.Instance.GetPartyMemberAtPosition(2)?.Name.ToUpper() ?? "BASIL";
        DialogueManager.Instance.QueueMessage($"{name} is too AFRAID to move.");
        await DialogueManager.Instance.WaitForDialogue();
    }

    private async Task SimulationMari()
    {
        await Wait.Frames(100);
        StopBGM();
        StopBGS();
        AnimationManager.Instance.ShowPicture(3, "simulation_mari_scare", Vector2.Zero, false, Vector2.One, 255);
        AudioManager.Instance.PlaySFX("SE_bs_scare7", 1f, 0.9f);
        await AnimationManager.Instance.PlayPictureAnimation(3, "scare");
        AnimationManager.Instance.ShowPicture(4, "black_screen", Vector2.Zero, false, Vector2.One, 255);
        AnimationManager.Instance.TintScreen(-255, -255, -255, 0, 1 / 60f);
        await Wait.Frames(60);
        BattleLogManager.Instance.ClearBattleLog();
        await FearPages.End(this);
    }

    public override BattleCommand ProcessAI()
    {
        return new BattleCommand(this, SelectTarget(), Skills["OSBBlurryNothing"]);
    }
}
