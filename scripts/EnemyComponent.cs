using System;
using Godot;
using OmoriSandbox.Actors;
using OmoriSandbox.Battle;
using OmoriSandbox.Editor;

namespace OmoriSandbox;

/// <summary>
/// The component attached to an enemy <see cref="Node"/> in the scene.
/// </summary>
public partial class EnemyComponent : Node
{
	private Enemy Enemy;
	private EnemyInfoBox InfoBox;

	private Timer HurtTimer = new()
	{
		Autostart = false,
		OneShot = true
	};
	
	/// <summary>
	/// The <see cref="Actors.Enemy"/> actor this component is attached to.
	/// </summary>
	public Enemy Actor => Enemy;

	internal void SetEnemy(Enemy enemy, string initialState, bool fallsOffScreen, bool grayscaleOnDefeat, int layer, Stats adjustedStats = default)
	{
		Enemy = enemy;
		AnimatedSprite2D sprite = GetNode<AnimatedSprite2D>("../Sprite");
		Enemy.Init(sprite, initialState, fallsOffScreen, grayscaleOnDefeat, layer, adjustedStats);
		if (SettingsMenuManager.Instance.ShowMoreInfo)
			InfoBox = ResourceLoader.Load<PackedScene>("res://scenes/enemy_infobox_moreinfo.tscn")
				.Instantiate<EnemyMoreInfoBox>();
		else
			InfoBox = ResourceLoader.Load<PackedScene>("res://scenes/enemy_infobox.tscn")
				.Instantiate<EnemyInfoBox>();
		AddChild(InfoBox);
		InfoBox.SetEnemy(Enemy);
		ShowInfoBox(false);
		
		Enemy.CenterPoint = GetParent<Node2D>().GlobalPosition;
		InfoBox.Position = Enemy.CenterPoint + new Vector2(0, -30);
		Enemy.OnDamaged += Damaged;
		Enemy.OnRevived += Revived;
		HurtTimer.Timeout += () => Enemy.SetHurt(false);
		AddChild(HurtTimer);
	}

	internal void ShowInfoBox(bool show)
	{
		InfoBox.Show(show);
	}

	private void Damaged(object sender, EventArgs e)
	{
		Enemy.SetHurt(true);
		HurtTimer.Start(0.75d);
	}

	private void Revived(object sender, EventArgs e)
	{
		if (IsInstanceValid(this))
			BattleManager.Instance.OnEnemyRevived(this);
	}
}
