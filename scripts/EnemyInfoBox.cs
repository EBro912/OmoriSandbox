using Godot;
using OmoriSandbox.Actors;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Modifier;
using OmoriSandbox.Editor;

namespace OmoriSandbox;

public partial class EnemyInfoBox : Control
{
	protected Enemy Enemy;

	[Export] protected NinePatchRect Infobox;
	[Export] private Label NameLabel;
	[Export] private TextureProgressBar HPBar;
	[Export] private FlowContainer StateIcons;
	
	internal virtual float BoxBottomOffset => 28.5f;
	internal virtual float CursorAboveAnchor => -BoxBottomOffset;
	
	internal void SetCursorAboveBox(bool above)
	{
		Sprite2D cursor = GetNode<Sprite2D>("Cursor");
		cursor.Position = new Vector2(0, above ? CursorAboveAnchor - 10.5f : BoxBottomOffset + 10.5f);
		cursor.FlipH = !above;
	}

	internal virtual void SetEnemy(Enemy enemy)
	{
		Enemy = enemy;
		HPBar.MaxValue = Enemy.CurrentStats.MaxHP;
		HPBar.Value = Enemy.CurrentHP;
		NameLabel.Text = Enemy.Name;
		float width = Mathf.Max(Infobox.CustomMinimumSize.X, NameLabel.GetMinimumSize().X + 15);
		Infobox.Size = new Vector2(width, Infobox.Size.Y);
		Infobox.Position = new Vector2(-width / 2f, Infobox.Position.Y);
	}

	internal virtual void Show(bool show)
	{
		HPBar.MaxValue = Enemy.CurrentStats.MaxHP;
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
