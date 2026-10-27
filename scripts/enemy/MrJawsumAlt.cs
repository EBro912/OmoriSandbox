using System.Linq;
using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Animation;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;
internal sealed class MrJawsumAlt : Enemy
{
    public override string Name => "MR. JAWSUM";
    public override Vector2 InfoBoxOffset => new(0, -225);
    public override Vector2 DamageNumberOffset => new(0, -150);
    public override bool InfoBoxCursorAboveBox => true;
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/mr_jawsum.tres");
    protected override Stats Stats => new(3000, 1000, 999, 60, 1, 10, 95);
    protected override string[] EquippedSkills => ["MJSummonGator", "MJAttackOrder"];

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    private readonly Enemy[] Gators = new Enemy[2];
    private int Stage;
    private int BarrierHP;
    private int BarrierJuice;

    // VANILLA BUG: the Boss Rush summon doesn't refresh MR. JAWSUM, so after a lone mid-turn SUMMON MINION a hit on him that lands before he's refreshed isn't shared with the new gators
    internal bool BarrierHasFriends { get; private set; }

    private void RefreshBarrier() => BarrierHasFriends = BattleManager.Instance.GetAllAliveEnemies().Count > 1;

    private void RefreshBarrierOnChange(ref int seen, int value)
    {
        if (value == seen)
            return;
        seen = value;
        RefreshBarrier();
    }

    private void BarrierActionStarted(object sender, ActionEventArgs e)
    {
        if (StillInBattle() && e.Actor == this)
            RefreshBarrier();
    }

    private void BarrierActionEnded(object sender, ActionEventArgs e)
    {
        if (StillInBattle() && e.Targets.Contains(this) && !(e.Actor == this && e.Action == Skills["MJSummonGator"]))
            RefreshBarrier();
    }

    private bool StillInBattle()
    {
        if (BattleManager.Instance.GetAllEnemies().Contains(this))
            return true;
        UnhookBarrier();
        return false;
    }

    private void UnhookBarrier()
    {
        BattleManager.Instance.ActionStarted -= BarrierActionStarted;
        BattleManager.Instance.ActionEnded -= BarrierActionEnded;
    }

    public override BattleCommand ProcessAI()
    {
        if (Gators.All(x => x?.Component == null || x.IsToast))
            return new BattleCommand(this, this, Skills["MJSummonGator"]);
        if (Roll() < 21)
            return new BattleCommand(this, SelectAllEnemies(), Skills["MJAttackOrder"]);
        if (Gators.Any(x => x?.Component == null || x.IsToast))
            return new BattleCommand(this, this, Skills["MJSummonGator"]);
        return new BattleCommand(this, SelectAllEnemies(), Skills["MJAttackOrder"]);
    }

    internal void SpawnGatorGuy()
    {
        Enemy gator = ReinforcementSlots.Summon(this, ReinforcementSlots.MrJawsumBossRush);
        if (gator == null)
        {
            GD.PushWarning("Tried to summon more than 2 gator guys!");
            return;
        }
        Gators[ReinforcementSlots.SlotOf(gator, ReinforcementSlots.MrJawsumBossRush) - 2] = gator;
    }

    public override async Task ProcessBattleConditions()
    {

        if (Stage > 2) 
            return;
        
        if (IsBelowHP(0.99f) && Stage == 0)
        {
            DialogueManager.Instance.QueueMessage(this, "I WANT THESE KIDS GONE YOU UNDERSTAND!?");
            await DialogueManager.Instance.WaitForDialogue();
            Stage = 1;
        }
        
        if (IsBelowHP(0.75f) && Stage <= 1)
        {
            DialogueManager.Instance.QueueMessage(this, @"The GATOR GUY who runs them out gets free pizza...\![br][shake rate=20]on me!");
            await DialogueManager.Instance.WaitForDialogue();
            Stage = 2;
        }
        
        if (IsBelowHP(0.5f) && Stage <= 2)
        {
            AudioManager.Instance.PlaySFX("se_thunder_bolt", volume: 0.9f);
            AudioManager.Instance.PlaySFX("se_fire_whoosh", pitch: 0.7f, volume: 0.9f);
            AnimationManager.Instance.InitShake(new Shake(1, 8, 15));
            AnimationManager.Instance.TintOverlay(new Color(1f, 0f, 0f, 120f / 255f));
            AnimationManager.Instance.TintOverlay(new Color(1f, 0f, 0f, 0f), 0.5f);
            SetEmotion("angry", true);
            ClearResult();
            AddBattlebackLayer(3, "battlebacks_dw_jawsum");
            await Wait.Frames(20);
            DialogueManager.Instance.QueueMessage(this, @"What do you mean we're running low on henchmen!?\! That's impossible!");
            await DialogueManager.Instance.WaitForDialogue();
            Stage = 3;
        }
    }

    public override async Task OnDefeat()
    {
        UnhookBarrier();
        DialogueManager.Instance.QueueMessage(this, "You let yourselves be foiled by a bunch of children!?");
        DialogueManager.Instance.QueueMessage(this, "WHAT DID I EVEN HIRE YOU FOR!?");
        await DialogueManager.Instance.WaitForDialogue();
    }

    public override async Task OnStartOfBattle()
    {
        BarrierHP = CurrentHP;
        BarrierJuice = CurrentJuice;
        OnHPChanged += (_, _) => RefreshBarrierOnChange(ref BarrierHP, CurrentHP);
        OnJuiceChanged += (_, _) => RefreshBarrierOnChange(ref BarrierJuice, CurrentJuice);
        OnStatModifierAdded += (_, _) => RefreshBarrier();
        OnStatModifierRemoved += (_, _) => RefreshBarrier();
        OnEmotionChanged += (_, _) => RefreshBarrier();
        BattleManager.Instance.ActionStarted += BarrierActionStarted;
        BattleManager.Instance.ActionEnded += BarrierActionEnded;
        AddStatModifier("MinionBarrier");
        SpawnGatorGuy();
        SpawnGatorGuy();
        DialogueManager.Instance.QueueMessage(this, @"Boys...\! would you be so kind as to show these kids the way out?");
        await DialogueManager.Instance.WaitForDialogue();
    }

    public override Task ProcessStartOfTurn()
    {
        RefreshBarrier();
        return Task.CompletedTask;
    }

    public override Task ProcessEndOfTurn()
    {
        RefreshBarrier();
        return Task.CompletedTask;
    }

    public override async Task OnEndOfBattle(BattleOutcome outcome)
    {
        UnhookBarrier();
        if (outcome == BattleOutcome.Defeat) {
            DialogueManager.Instance.QueueMessage(CenterPoint, "[wave freq=20.0][font_size=43]JAWHAW[font_size=58]HAW[font_size=73]HAW!!!");
            DialogueManager.Instance.QueueMessage(this, "That's what happens when you mess with MR. JAWSUM!");
            await DialogueManager.Instance.WaitForDialogue();
        }
    }
}
