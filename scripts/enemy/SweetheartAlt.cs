using System.Threading.Tasks;
using Godot;

using OmoriSandbox.Battle;

namespace OmoriSandbox.Actors;
internal sealed class SweetheartAlt : Enemy
{
	public override string Name => "SWEETHEART";
	public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/sweetheart.tres");
	protected override Stats Stats => new(7600, 3800, 90, 70, 130, 20, 90);

	protected override string[] EquippedSkills => ["SHAttack", "SharpInsult", "SwingMace", "Brag"];

	private bool EmotionLocked = false;
	private int Stage = 0;

	public override bool IsStateValid(string state)
	{
		if (state == "toast")
			return true;

		if (EmotionLocked)
			return false;

		return state is "neutral" or "sad" or "happy" or "angry" or "hurt";
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
				if (Roll() < 41)
					goto insult;
				goto mace;
			case "happy":
				if (Roll() < 36)
					goto attack;
				if (Roll() < 46)
					goto insult;
				if (Roll() < 61)
					goto mace;
				goto brag;
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


	private bool UsedCharm = false;
	private bool UsedDonut = false;
	public override async Task OnDefeat()
	{
		DialogueManager.Instance.QueueMessage(this, @"No...\! Is this...\![br]What they call defeat?");
		DialogueManager.Instance.QueueMessage(this, @"[br]I cannot accept this...\![br]I will not accept this!");
		DialogueManager.Instance.QueueMessage(this, "[br]You're all nothing but a bunch of lowly peasants!");
		await DialogueManager.Instance.WaitForDialogue();
	}

	public override async Task ProcessBattleConditions()
	{
		if (CurrentHP <= 0) return;
		
		BattleCommand current = BattleManager.Instance.GetCurrentCommand();
		if (!UsedCharm)
			UsedCharm = current.Actor is Hero &&
			            current.Action.Name is "CHARM" or "CAPTIVATE" or "MESMERIZE" or "SMILE";
		if (!UsedDonut)
			UsedDonut = current.Action.Name is "Donut";

		if (Stage > 3)
			return;
		
		if (CurrentHP < 6080 && Stage == 0)
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
		
		if (CurrentHP < 4940 && Stage <= 1)
		{
			DialogueManager.Instance.QueueMessage(this, "You dare raise your fists at me!?");
			DialogueManager.Instance.QueueMessage(this, @"Fools!\! You should be grovelling on your knees!");
			await DialogueManager.Instance.WaitForDialogue();
			Stage = 2;
		}
		
		if (CurrentHP < 3800 && Stage <= 2)
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
		
		if (CurrentHP < 2280 && Stage <= 3)
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
	
	public override async Task ProcessEndOfTurn()
	{
		if (UsedCharm)
		{
			DialogueManager.Instance.QueueMessage("[wave freq=10.0][font_size=40]OH HERO![font_size=52]MY HERO!!");
			DialogueManager.Instance.QueueMessage("[wave freq=10.0][font_size=40]YOUR SMILE CHARMS MY HEART!");
			DialogueManager.Instance.QueueMessage("[wave freq=10.0][font_size=40]I WILL MAKE IT MINE!");
			await DialogueManager.Instance.WaitForDialogue();
		}
		UsedCharm = false;
		if (UsedDonut)
		{
			DialogueManager.Instance.QueueMessage("How dare you eat a [color=#5bd863]DONUT[/color] in my presence!");
			await DialogueManager.Instance.WaitForDialogue();
		}
		UsedDonut = false;
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
