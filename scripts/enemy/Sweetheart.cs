using System.Threading.Tasks;
using Godot;

using OmoriSandbox.Battle;

namespace OmoriSandbox.Actors;
internal sealed class Sweetheart : Enemy
{
	public override string Name => "SWEETHEART";
	public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/sweetheart.tres");
	protected override Stats Stats => new(3300, 1650, 30, 25, 40, 10, 90);

	protected override string[] EquippedSkills => ["SHAttack", "SharpInsult", "SwingMace", "Brag"];

	private bool EmotionLocked = false;
	private int Stage = 0;

	public override bool IsStateValid(string state)
	{
		if (state == "toast")
			return true;

		if (EmotionLocked)
			return false;

		return state == "neutral" || state == "sad" || state == "happy"
			|| state == "angry" || state == "hurt";
	}


	public override BattleCommand ProcessAI()
	{
		if (HasMultiTargetObserve())
			return new BattleCommand(this, SelectAllTargets(), Skills["SwingMace"]);
		if (HasObserveTarget(out PartyMember observe))
			return new BattleCommand(this, observe, Skills["SHAttack"]);
		
		switch (CurrentState)
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



	public override async Task ProcessBattleConditions()
	{
		if (CurrentHP <= 0)
		{
            DialogueManager.Instance.QueueMessage(this, @"No...\! Is this...\![br]What they call defeat?");
            DialogueManager.Instance.QueueMessage(this, @"[br]I cannot accept this...\![br]I will not accept this!");
            DialogueManager.Instance.QueueMessage(this, "[br]You're all nothing but a bunch of lowly peasants!");
            await DialogueManager.Instance.WaitForDialogue();
			return;
        }

		if (Stage > 3)
			return;
		
		if (CurrentHP < 2640 && Stage == 0)
		{
			DialogueManager.Instance.QueueMessage(this, @"It's pointless, you fools!\! You cannot dampen my positive energy!");
			await DialogueManager.Instance.WaitForDialogue();
			ForceState("SweetheartHappy", "happy");
			DialogueManager.Instance.QueueMessage("SWEETHEART became HAPPY!");
			DialogueManager.Instance.QueueMessage("SWEETHEART can no longer become SAD or ANGRY!");
			await DialogueManager.Instance.WaitForDialogue();
			EmotionLocked = true;
			Stage = 1;
		}
		
		if (CurrentHP < 2145 && Stage <= 1)
		{
			DialogueManager.Instance.QueueMessage(this, "You dare raise your fists at me!?");
			DialogueManager.Instance.QueueMessage(this, @"Fools!\! You should be grovelling on your knees!");
			await DialogueManager.Instance.WaitForDialogue();
			Stage = 2;
		}
		
		if (CurrentHP < 1650 && Stage <= 2)
		{
			EmotionLocked = false;
			DialogueManager.Instance.QueueMessage(this, @"Oho!\! My beauty and grace is boundless and everlasting...");
			DialogueManager.Instance.QueueMessage(this, "It's a shame that you won't be able to enjoy it for much longer!");
			await DialogueManager.Instance.WaitForDialogue();
			ForceState("SweetheartEcstatic", "ecstatic");
			DialogueManager.Instance.QueueMessage("SWEETHEART became ECSTATIC!");
			await DialogueManager.Instance.WaitForDialogue();
			EmotionLocked = true;
			Stage = 3;
		}
		
		if (CurrentHP < 990 && Stage <= 3)
		{
			EmotionLocked = false;
			DialogueManager.Instance.QueueMessage(this, "Hmph! I see you are still standing.");
			DialogueManager.Instance.QueueMessage(this, "Cockroaches are resilient, I suppose!");
			DialogueManager.Instance.QueueMessage("[wave freq=10.0][font_size=40]OHOHOH[font_size=52]OHOHOHO!!");
			await DialogueManager.Instance.WaitForDialogue();
			ForceState("SweetheartManic", "manic");
			DialogueManager.Instance.QueueMessage("SWEETHEART became MANIC!");
			await DialogueManager.Instance.WaitForDialogue();
			EmotionLocked = true;
			Stage = 4;
		}
	}

	public override Task OnStartOfBattle()
	{
		OnStateChanged += (_, _) =>
		{
			if (CurrentState is "ecstatic" or "manic")
				BaseStats = new(3300, 1650, 30, 25, 40, 30, 90);
			else
				BaseStats = new(3300, 1650, 30, 25, 40, 10, 90);
		};
		return Task.CompletedTask;
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
