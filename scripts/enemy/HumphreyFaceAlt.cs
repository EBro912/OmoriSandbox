using System;
using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Animation;
using OmoriSandbox.Battle;

namespace OmoriSandbox.Actors;

internal sealed class HumphreyFaceAlt : Enemy
{
	public override string Name => "HUMPHREY";
	public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/humphrey_face.tres");
	protected override Stats Stats => new(10000, 3000, 110, 50, 115, 10, 95);
	protected override string[] EquippedSkills => ["HUFChomp", "HUFDoNothing", "HUFSwallow"];
	
	public override bool IsStateValid(string state)
	{
		return state is "neutral" or "sad" or "happy" or "angry" or "toast";
	}
	
	public override BattleCommand ProcessAI()
	{
		if (HasObserveTarget(out PartyMember observe))
			return new BattleCommand(this, observe, Skills["HUFChomp"]);
		
		switch (CurrentState)
		{
			case "angry":
				if (Roll() < 76)
					goto chomp;
				goto nothing;
			case "sad":
				if (Roll() < 41)
					goto chomp;
				goto nothing;
			case "happy":
				if (Roll() < 51)
					goto chomp;
				goto nothing;
			default:
				if (Roll() < 56)
					goto chomp;
				goto nothing;
		}
		
		chomp:
			return new BattleCommand(this, SelectTarget(), Skills["HUFChomp"]);
		nothing:
			return new BattleCommand(this, SelectTarget(), Skills["HUFDoNothing"]);
	}

	private readonly string[] Messages =
	[
		@"[wave freq=10.0]It doesn't matter how quick or how slow...\| The more you struggle, the deeper we'll go![/wave]",
		@$"[wave freq=10.0]Just relax... There's nothing to fear.\| Hey {BattleManager.Instance.GetPartyMember(0)?.Name.ToUpper()}... is it getting stuffy in here?[/wave]",
		@"[wave freq=10.0]Cooking meat is very fun!\| Should you be rare, medium-rare, medium, or well done?[/wave]",
		@"[wave freq=10.0]It's pointless to squirm. Give up, my friend.\| I'm afraid this cycle will never end.[/wave]",
        @"[wave freq=10.0]There's no need to squirm. Ignorance is bliss.\| How many times must we do this?[/wave]"
	];

	private int MessageIndex = 0;
	private bool SkipFirst = true;
	
	public override async Task ProcessEndOfTurn()
	{
		if (SkipFirst)
		{
			SkipFirst = false;
			return;
		}
		
		DialogueManager.Instance.QueueMessage(this, Messages[MessageIndex]);
		await DialogueManager.Instance.WaitForDialogue();
		MessageIndex++;
		if (MessageIndex >= Messages.Length)
			MessageIndex = 0;
		BattleManager.Instance.ForceCommand(this, SelectAllTargets(), Skills["HUFSwallow"]);
	}

	public override async Task ProcessBattleConditions()
	{
		if (CurrentHP <= 0)
		{
			DialogueManager.Instance.QueueMessage(this, @"[wave freq=10.0]Feel free to struggle, 'cuz no matter what...\| You'll never be able to escape my gut!");
			await DialogueManager.Instance.WaitForDialogue();
		}
	}
}
