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

	internal override void SetEnemy(Enemy enemy)
	{
		base.SetEnemy(enemy);
		JuiceBar.MaxValue = Enemy.BaseStats.MaxJuice;
		JuiceBar.Value = Enemy.CurrentJuice;
		HPLabel.Text = $"{Enemy.CurrentHP}/{Enemy.BaseStats.MaxHP}";
		JuiceLabel.Text = $"{Enemy.CurrentJuice}/{Enemy.BaseStats.MaxJuice}";
		ATKLabel.Text = $"ATK: {Enemy.BaseStats.ATK}";
		SPDLabel.Text = $"SPD: {Enemy.BaseStats.SPD}";
		DEFLabel.Text = $"DEF: {Enemy.BaseStats.DEF}";
		LCKLabel.Text = $"LCK: {Enemy.BaseStats.LCK}";
		HITLabel.Text = $"HIT: {Enemy.BaseStats.HIT}";
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
	}
}
