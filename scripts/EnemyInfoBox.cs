using Godot;
using OmoriSandbox.Actors;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Modifier;
using OmoriSandbox.Editor;

namespace OmoriSandbox;

public partial class EnemyInfoBox : Control
{
	protected Enemy Enemy;

	[Export] private NinePatchRect Infobox;
	[Export] private Label NameLabel;
	[Export] private TextureProgressBar HPBar;
	[Export] private FlowContainer StateIcons;
	
	internal virtual void SetEnemy(Enemy enemy)
	{
		Enemy = enemy;
		HPBar.MaxValue = Enemy.CurrentStats.HP;
		HPBar.Value = Enemy.CurrentHP;
		NameLabel.Text = Enemy.Name;
		float width = Mathf.Max(Infobox.CustomMinimumSize.X, NameLabel.GetMinimumSize().X + 15);
		Infobox.Size = new Vector2(width, Infobox.Size.Y);
		Infobox.Position = new Vector2(-width / 2f, Infobox.Position.Y);
	}

	internal virtual void Show(bool show)
	{
		HPBar.Value = Enemy.CurrentHP;
		if (SettingsMenuManager.Instance.ShowStateIcons)
			UpdateStateIcons();
		Visible = show;
	}
	
	private void UpdateStateIcons()
	{
		StateIconRenderer.Render(StateIcons, Enemy);
	}
}
