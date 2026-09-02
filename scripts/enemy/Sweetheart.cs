using System.Threading.Tasks;
using Godot;

using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;
internal sealed class Sweetheart : Enemy
{
	public override string Name => "SWEETHEART";
	public override Vector2 InfoBoxOffset => new(0, -225);
	public override bool InfoBoxCursorAboveBox => true;
	public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/sweetheart.tres");
	protected override Stats Stats => new(3300, 1650, 30, 25, 40, 10, 90);

	protected override string[] EquippedSkills => ["SHAttack", "SharpInsult", "SwingMace", "Brag"];

	private int Stage = 0;

	public override bool IsEmotionValid(Emotion emotion)
	{
		if (IsEmotionLocked)
			return false;

		return emotion.Id is "neutral" or "sad" or "happy" or "angry";
	}


	public override BattleCommand ProcessAI()
	{
		if (HasMultiTargetObserve())
			return new BattleCommand(this, SelectAllTargets(), Skills["SwingMace"]);
		if (HasObserveTarget(out PartyMember observe))
			return new BattleCommand(this, observe, Skills["SHAttack"]);
		
		if (Stage >= 1)
		{
			if (Roll() < 46)
				goto attack;
			if (Roll() < 46)
				goto insult;
			goto mace;
		}

		switch (CurrentEmotion.Id)
		{
			case "manic":
			case "ecstatic":
				if (Roll() < 46)
					goto attack;
				if (Roll() < 46)
					goto insult;
				goto mace;
			case "happy":
				if (Roll() < 36)
					goto attack;
				if (Roll() < 46)
					goto insult;
				goto mace;
			case "sad":
				if (Roll() < 36)
					goto attack;
				if (Roll() < 21)
					goto insult;
				if (Roll() < 31)
					goto mace;
				goto brag;
			case "angry":
				if (Roll() < 51)
					goto attack;
				if (Roll() < 31)
					goto insult;
				if (Roll() < 71)
					goto mace;
				goto brag;
			default:
				if (Roll() < 41)
					goto attack;
				if (Roll() < 31)
					goto insult;
				if (Roll() < 36)
					goto mace;
				goto brag;

		}
	attack:
		return new BattleCommand(this, SelectTarget(), Skills["SHAttack"]);
	insult:
		return new BattleCommand(this, SelectAllTargets(), Skills["SharpInsult"]);
	mace:
		return new BattleCommand(this, SelectAllTargets(), Skills["SwingMace"]);
	brag:
		return new BattleCommand(this, this, Skills["Brag"]);
	}

	
	private bool CharmPending, CharmDone, DonutPending, DonutDone;
	private PartyMember CharmingHero;
	public override async Task OnDefeat()
	{
		DialogueManager.Instance.QueueMessage(this, @"No...\! Is this...\![br]what they call defeat?");
		DialogueManager.Instance.QueueMessage(this, @"[br]I cannot accept this...\![br]I will not accept this!");
		DialogueManager.Instance.QueueMessage(this, "[br]You're all nothing but a bunch of lowly peasants!");
		await DialogueManager.Instance.WaitForDialogue();
	}

	public override async Task ProcessBattleConditions()
	{
		if (CurrentHP <= 0) return;

		BattleCommand current = BattleManager.Instance.GetCurrentCommand();
		if (!CharmDone && current.Actor is Hero hero &&
		    current.Action.Name is "CHARM" or "CAPTIVATE" or "MESMERIZE" or "SMILE")
		{
			CharmPending = true;
			CharmingHero = hero;
		}
		if (!DonutDone && current.Action.Name is "Donut")
			DonutPending = true;

		if (Stage > 3)
			return;
		
		if (IsBelowHP(0.8f) && Stage == 0)
		{
			DialogueManager.Instance.QueueMessage(this, @"It's pointless, you fools!\! You cannot dampen my positive energy!");
			await DialogueManager.Instance.WaitForDialogue();
			SetEmotionForced("happy");
			DialogueManager.Instance.QueueMessage("SWEETHEART became HAPPY!");
			DialogueManager.Instance.QueueMessage("SWEETHEART can no longer become SAD or ANGRY!");
			await DialogueManager.Instance.WaitForDialogue();
			LockEmotion("happy");
			Stage = 1;
		}
		
		if (IsBelowHP(0.65f) && Stage <= 1)
		{
			DialogueManager.Instance.QueueMessage(this, "You dare raise your fists at me!?");
			DialogueManager.Instance.QueueMessage(this, @"Fools!\! You should be grovelling on your knees!");
			await DialogueManager.Instance.WaitForDialogue();
			Stage = 2;
		}
		
		if (IsBelowHP(0.5f) && Stage <= 2)
		{
			UnlockEmotion();
			DialogueManager.Instance.QueueMessage(this, @"Oho!\! My beauty and grace is boundless and everlasting...");
			DialogueManager.Instance.QueueMessage(this, "It's a shame that you won't be able to enjoy it for much longer!");
			await DialogueManager.Instance.WaitForDialogue();
			SetEmotionForced("ecstatic");
			DialogueManager.Instance.QueueMessage("SWEETHEART became ECSTATIC!");
			await DialogueManager.Instance.WaitForDialogue();
			LockEmotion("happy");
			Stage = 3;
		}
		
		if (IsBelowHP(0.3f) && Stage <= 3)
		{
			UnlockEmotion();
			DialogueManager.Instance.QueueMessage(this, "Hmph! I see you are still standing.");
			DialogueManager.Instance.QueueMessage(this, "Cockroaches are resilient, I suppose!");
			DialogueManager.Instance.QueueMessage("[wave freq=10.0][font_size=40]OHOHOH[font_size=52]OHOHOHO!!");
			await DialogueManager.Instance.WaitForDialogue();
			SetEmotionForced("manic");
			DialogueManager.Instance.QueueMessage("SWEETHEART became MANIC!");
			await DialogueManager.Instance.WaitForDialogue();
			LockEmotion("happy");
			Stage = 4;
		}
	}

	public override async Task ProcessEndOfTurn()
	{
		if (CharmPending)
		{
			DialogueManager.Instance.QueueMessage("[wave freq=10.0][font_size=40]OH HERO![font_size=52]MY HERO!!");
			DialogueManager.Instance.QueueMessage("[wave freq=10.0][font_size=40]YOUR SMILE CHARMS MY HEART!");
			DialogueManager.Instance.QueueMessage("[wave freq=10.0][font_size=40]I WILL MAKE IT MINE!!");
			await DialogueManager.Instance.WaitForDialogue();

			if (!CharmingHero.IsToast)
				CharmingHero.Heal(100);
			CharmPending = false;
			CharmDone = true;
			// original event returns here, so a pending donut dialogue has to wait another turn
			return;
		}
		if (DonutPending)
		{
			DialogueManager.Instance.QueueMessage("How dare you eat a [color=#5bd863]DONUT[/color] in my presence!");
			await DialogueManager.Instance.WaitForDialogue();
			DonutPending = false;
			DonutDone = true;
		}
	}

	public override Stats GetBaseStats()
	{
		if (Stage >= 1)
			return new Stats(3300, 1650, 30, 25, 40, 30, 90) + AdjustedStats;
		return new Stats(3300, 1650, 30, 25, 40, 10, 90) + AdjustedStats;
	}

	public override async Task OnEndOfBattle(bool victory)
    {
        if (!victory)
		{
			DialogueManager.Instance.QueueMessage("[wave freq=10.0][font_size=40]OHOHOH[font_size=52]OHOHOHO!!");
            DialogueManager.Instance.QueueMessage(this, @"This was child's play!\! You're all nothing but a bunch of lowly peasants!");
            DialogueManager.Instance.QueueMessage(this, "[br]To [color=#64f7ed]THE DUNGEON[/color] with you!");
            await DialogueManager.Instance.WaitForDialogue();
        }

    }
}
