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
		HPBar.MaxValue = Enemy.BaseStats.HP;
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
		// this may need to be optimized, not the best practice to fully replace nodes
		foreach (Node child in StateIcons.GetChildren())
			child.Free();
		
		foreach (StatModifier modifier in Enemy.StatModifiers.Values)
		{
			StateIcon[] icons = modifier.GetStateIcons();
			foreach (StateIcon icon in icons)
			{
				string tooltip = modifier.TurnsLeft > -1 ? icon.Description + "\nTurns Left: " + modifier.TurnsLeft : icon.Description;
				if (Database.TryGetStateIcon(icon.AssetName, out Texture2D texture))
				{
					TextureRect rect = new()
					{
						Texture = texture,
						TooltipText = tooltip
					};
					StateIcons.AddChild(rect);
				}
				else
				{
					GD.PrintErr("Unknown state icon texture: " + icon.AssetName);
				}
			}
		}
	}
}
