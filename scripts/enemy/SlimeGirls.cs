using Godot;
using System.Threading.Tasks;

using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;
internal sealed class SlimeGirls : Enemy
{
	public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/slimegirls.tres");

	public override string Name => "SLIME GIRLS";
	public override Vector2 InfoBoxOffset => new(0, -235);
	public override bool InfoBoxCursorAboveBox => true;

	protected override Stats Stats => new(5700, 1750, 57, 32, 52, 10, 95);

	protected override string[] EquippedSkills => ["ComboAttack", "StrangeGas", "Dynamite", "StingRay", "Swap", "Chainsaw", "SlimeUltimateAttack", "SGSelfAngry"];

	public override bool IsEmotionValid(Emotion emotion)
	{
		return emotion.Id is "neutral" or "sad" or "happy" or "angry";
	}

	private int Stage = 0;

	public override BattleCommand ProcessAI()
	{
		if (HasMultiTargetObserve())
			return new BattleCommand(this, SelectAllTargets(), Skills["Dynamite"]);
        
		if (HasObserveTarget(out PartyMember observe))
			return new BattleCommand(this, observe, Skills["ComboAttack"]);
		
        switch (CurrentEmotion.Id)
		{
			case "happy":
				if (Roll() < 16)
					goto combo;
				if (Roll() < 21)
					goto gas;
				if (Roll() < 16)
					goto dynamite;
				if (Roll() < 21)
					goto stingray;
				goto chainsaw;
			case "sad":
				if (Roll() < 16)
					goto combo;
				if (Roll() < 16)
					goto gas;
				if (Roll() < 21)
					goto dynamite;
				if (Roll() < 16)
					goto stingray;
				goto chainsaw;
			case "angry":
				if (Roll() < 21)
					goto combo;
				if (Roll() < 16)
					goto gas;
				if (Roll() < 21)
					goto dynamite;
				if (Roll() < 16)
					goto stingray;
				goto chainsaw;
			default:
				if (Roll() < 16)
					goto combo;
				if (Roll() < 16)
					goto gas;
				if (Roll() < 16)
					goto dynamite;
				if (Roll() < 16)
					goto stingray;
				goto chainsaw;
		}

	combo:
		return new BattleCommand(this, SelectTarget(), Skills["ComboAttack"]);
	gas:
		return new BattleCommand(this, SelectAllTargets(), Skills["StrangeGas"]);
	dynamite:
		return new BattleCommand(this, SelectAllTargets(), Skills["Dynamite"]);
	stingray:
		return new BattleCommand(this, SelectTarget(), Skills["StingRay"]);
	chainsaw:
		return new BattleCommand(this, SelectTarget(), Skills["Chainsaw"]);
	}

	public override async Task ProcessBattleConditions()
	{
		if (CurrentHP <= 0)
			return;

		if (Stage > 2)
			return;

		if (IsBelowHP(0.75f) && Stage == 0)
		{
			DialogueManager.Instance.QueueMessage("MEDUSA", CenterPoint, @"Hmph...\! You kids are more resilient than expected.");
			DialogueManager.Instance.QueueMessage("MARINA", CenterPoint, @"You know what that means.\! It's time to get serious!");
			DialogueManager.Instance.QueueMessage("MOLLY", CenterPoint, "Oh...[br]I'm having so much fun~!");
			await DialogueManager.Instance.WaitForDialogue();
			BattleManager.Instance.ForceCommand(this, this, Skills["SGSelfAngry"]);
			Stage = 1;
		}
		
		if (IsBelowHP(0.5f) && Stage <= 1)
		{
			DialogueManager.Instance.QueueMessage("MARINA", CenterPoint, "Hey, MEDUSA![br]Are you thinkin' what I'm thinkin'?");
			DialogueManager.Instance.QueueMessage("MEDUSA", CenterPoint, @"Yes, sister...\! I think it's about time we switched things up.");
			DialogueManager.Instance.QueueMessage("MOLLY", CenterPoint, @"Just relax, children...\![br]This won't hurt a bit~");
			await DialogueManager.Instance.WaitForDialogue();
			BattleManager.Instance.ForceCommand(this, SelectAllTargets(), Skills["Swap"]);
			Stage = 2;
		}
	}

	public override Task ProcessEndOfTurn()
	{
		if (CurrentHP > 0 && IsBelowHP(0.25f) && Stage <= 2)
		{
			BattleManager.Instance.ForceCommand(this, SelectAllTargets(), Skills["SlimeUltimateAttack"]);
			Stage = 3;
		}
		return Task.CompletedTask;
	}

	public override async Task OnDefeat()
	{
		DialogueManager.Instance.QueueMessage("MARINA", CenterPoint, "You kids... are a lot tougher than you look.");
		DialogueManager.Instance.QueueMessage("MOLLY", CenterPoint, @"Hmph...\! This is much more trouble than it's worth.");
		DialogueManager.Instance.QueueMessage("MEDUSA", CenterPoint, @"Sigh...\! What a predicament...\! How will we feed HUMPHREY now?");
		await DialogueManager.Instance.WaitForDialogue();
	}

    public override async Task OnEndOfBattle(bool victory)
    {
        if (!victory)
		{
            DialogueManager.Instance.QueueMessage("MARINA", CenterPoint, "Now you belong to us!");
            DialogueManager.Instance.QueueMessage("MOLLY", CenterPoint, @"Hush, hush, darlings...[br]Don't cry...\! You'll get used to your new life soon~");
            DialogueManager.Instance.QueueMessage("MEDUSA", CenterPoint, @"It's time to take apart the small one.\! Let's get started, dolls.");
            await DialogueManager.Instance.WaitForDialogue();
        }
    }
}
