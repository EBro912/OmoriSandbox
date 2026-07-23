using Godot;
using OmoriSandbox.Actors;
using OmoriSandbox.Battle;

namespace OmoriSandbox;

public partial class EnemyMoreInfoBox : EnemyInfoBox
{
	[Export] private Label HPLabel;
	[Export] private TextureProgressBar JuiceBar;
	[Export] private Label JuiceLabel;
	[Export] private Label ATKLabel;
	[Export] private Label SPDLabel;
	[Export] private Label DEFLabel;
	[Export] private Label LCKLabel;
	[Export] private Label HITLabel;
	[Export] private StateAnimator Animator;

	internal override void SetEnemy(Enemy enemy)
	{
		base.SetEnemy(enemy);
		Stats stats = Enemy.CurrentStats;
		JuiceBar.MaxValue = stats.MaxJuice;
		JuiceBar.Value = Enemy.CurrentJuice;
		HPLabel.Text = $"{Enemy.CurrentHP}/{stats.MaxHP}";
		JuiceLabel.Text = $"{Enemy.CurrentJuice}/{stats.MaxJuice}";
		ATKLabel.Text = $"ATK: {stats.ATK}";
		SPDLabel.Text = $"SPD: {stats.SPD}";
		DEFLabel.Text = $"DEF: {stats.DEF}";
		LCKLabel.Text = $"LCK: {stats.LCK}";
		HITLabel.Text = $"HIT: {stats.HIT}";
		Animator.SetStateAtlas(Enemy.CurrentState);
		
		// shift the emotion sprite relative to the size of the name
		// keeps the sprite centered on the infobox
		Sprite2D state = Animator.EmotionSprite;
		float widthDelta = Infobox.Size.X - Infobox.CustomMinimumSize.X;
		state.Position = new Vector2(widthDelta / 2f, state.Position.Y);
	}

	internal override void Show(bool show)
	{
		base.Show(show);
		Stats stats = Enemy.CurrentStats;
		HPLabel.Text = $"{Enemy.CurrentHP}/{stats.MaxHP}";
		JuiceBar.Value = Enemy.CurrentJuice;
		JuiceLabel.Text = $"{Enemy.CurrentJuice}/{stats.MaxJuice}";
		ATKLabel.Text = $"ATK: {stats.ATK}";
		SPDLabel.Text = $"SPD: {stats.SPD}";
		DEFLabel.Text = $"DEF: {stats.DEF}";
		LCKLabel.Text = $"LCK: {stats.LCK}";
		HITLabel.Text = $"HIT: {stats.HIT}";
		Animator.SetStateAtlas(Enemy.CurrentState);
	}
}
