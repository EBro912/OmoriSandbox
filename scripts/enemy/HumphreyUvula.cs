using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class HumphreyUvula : Enemy
{
	public override string Name => "HUMPHREY";
	public override Vector2 InfoBoxOffset => new(0, -115);
	public override bool InfoBoxCursorAboveBox => true;
	public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/uvula.tres");
	protected override Stats Stats => new(1, 1, 1, 1, 900, 1, 95);
	protected override string[] EquippedSkills => ["HUUDoNothing"];
	
	public override bool IsEmotionValid(Emotion emotion) => emotion.Id is "neutral";

	public override BattleCommand ProcessAI() => new(this, SelectTarget(), Skills["HUUDoNothing"]);

	private static readonly string[] Taunts =
	[
		"You are nothing but meat, so please let me eat you.",
		@"Do not attack me, foodstuff.\! It is ineffectual!",
		"This is why I shouldn't play with my food.",
		@"Yum, yum, yum...\! It's time for grub!",
		@"The stronger prey on the weaker.\! Don't you know?\! That's biology!",
		@"Please wait to be digested.\! There is a queue, so you'll have to wait.",
		@"Do you see anyone else struggling?\! Behave yourselves, foodstuff!",
	];
	private int Taunt = 0;
	private bool TransformTurn = true;
	private Tween BreatheTween;

	public override Task OnStartOfBattle()
	{
		Breathe();
		return base.OnStartOfBattle();
	}

	public override async Task ProcessEndOfTurn()
	{
		if (CurrentHP <= 0)
			return;
		// don't start taunting until after the initial transformation turn
		if (TransformTurn)
		{
			TransformTurn = false;
			return;
		}
		DialogueManager.Instance.QueueMessage(this, Taunts[Taunt]);
		await DialogueManager.Instance.WaitForDialogue();
		Taunt = (Taunt + 1) % Taunts.Length;
	}

	public override Task OnDefeat()
	{
		BreatheTween.Stop();
		return base.OnDefeat();
	}

	private void Breathe()
	{
		// Godot scales from the center of the sprite, not the bottom
		// offset the sprite and the parent so it visually appears in the same position as where it spawned
		// but scales from the bottom
		float yOffset = -Sprite.SpriteFrames.GetFrameTexture("neutral", 0).GetHeight() / 2f;
		Sprite.Offset = new Vector2(Sprite.Offset.X, yOffset);
		Vector2 parentPos = Sprite.GetParent<Node2D>().GlobalPosition;
		Sprite.GetParent<Node2D>().GlobalPosition = new Vector2(parentPos.X, parentPos.Y - yOffset);
        
		BreatheTween = Sprite.CreateTween();
		BreatheTween.TweenProperty(Sprite, "scale:y", 1.04f, 1.75f).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
		BreatheTween.TweenInterval(0.25f);
		BreatheTween.TweenProperty(Sprite, "scale:y", 1f, 1.75f).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
		BreatheTween.SetLoops();
	}
}
