using System;
using Godot;
using OmoriSandbox.Actors;
using OmoriSandbox.Battle;
using OmoriSandbox.Editor;

namespace OmoriSandbox;

/// <summary>
/// The component attached to an enemy <see cref="Node"/> in the scene.
/// </summary>
public partial class EnemyComponent : ActorComponent
{
	private static readonly Color SelectionBlink = new(1f, 1f, 1f, 64f / 255f);
	private static readonly Color NoBlend = new(0f, 0f, 0f, 0f);

	private EnemyInfoBox InfoBox;
	private bool Selected;
	private double SelectedTime;
	private Color BlinkColor = NoBlend;
	private float FeetY;

	private Timer HurtTimer = new()
	{
		Autostart = false,
		OneShot = true
	};
	
	/// <summary>
	/// The <see cref="Actors.Enemy"/> actor this component is attached to.
	/// </summary>
	public override Enemy Actor => Enemy;
	private Enemy Enemy;

	internal void SetEnemy(Enemy enemy, string initialState, bool fallsOffScreen, bool grayscaleOnDefeat, int layer, Stats adjustedStats = default)
	{
		enemy.Bind(this);
		Enemy = enemy;
		AnimatedSprite2D sprite = GetNode<AnimatedSprite2D>("../Sprite");
		Actor.Init(sprite, initialState, fallsOffScreen, grayscaleOnDefeat, layer, adjustedStats);
		if (SettingsMenuManager.Instance.ShowMoreInfo)
			InfoBox = ResourceLoader.Load<PackedScene>("res://scenes/enemy_infobox_moreinfo.tscn")
				.Instantiate<EnemyMoreInfoBox>();
		else
			InfoBox = ResourceLoader.Load<PackedScene>("res://scenes/enemy_infobox.tscn")
				.Instantiate<EnemyInfoBox>();
		AddChild(InfoBox);
		InfoBox.SetEnemy(Actor);
		ShowInfoBox(false);
		
		RefreshInfoBox();
		Actor.OnDamaged += Damaged;
		Actor.OnRevived += Revived;
		HurtTimer.Timeout += () => Actor.SetHurt(false);
		AddChild(HurtTimer);
	}

	internal Vector2 DamageNumberPosition => Actor.CenterPoint + new Vector2(0f, FeetY) + Actor.DamageNumberOffset + new Vector2(-1f, -85f);

	internal override void ShowDamageNumber(int damage, DamageType type, bool critical)
	{
		BattleManager.Instance.SpawnDamageNumber(damage, DamageNumberPosition, type, critical);
	}

	internal void RefreshInfoBox()
	{
		AnimatedSprite2D sprite = GetNode<AnimatedSprite2D>("../Sprite");
		Actor.CenterPoint = GetParent<Node2D>().GlobalPosition;
		// in vanilla, the box + offset is drawn relative to the actor's "feet"
		FeetY = sprite.SpriteFrames.GetFrameTexture(sprite.Animation, 0).GetHeight() * sprite.GlobalScale.Y / 2f;
		InfoBox.Position = Actor.CenterPoint + new Vector2(Actor.InfoBoxOffset.X, Actor.InfoBoxOffset.Y + FeetY);
		InfoBox.SetCursorAboveBox(Actor.InfoBoxCursorAboveBox);
		InfoBox.RefreshName();
	}

	internal void ShowInfoBox(bool show)
	{
		if (show)
			InfoBox.Position = Actor.CenterPoint + new Vector2(Actor.InfoBoxOffset.X, Actor.InfoBoxOffset.Y + FeetY);
		InfoBox.Show(show);
		switch (show)
		{
			case false when Selected:
				SetBlink(NoBlend);
				break;
			case true when !Selected:
				SelectedTime = 0d;
				break;
		}

		Selected = show && Actor.BlinksWhenSelected;
	}

	/// <inheritdoc/>
	public override void _Process(double delta)
	{
		if (!Selected)
			return;
		SelectedTime += delta;
		int frame = (int)Math.Round(SelectedTime * 60d);
		SetBlink(frame % 30 < 15 ? SelectionBlink : NoBlend);
	}

	private void SetBlink(Color color)
	{
		BlinkColor = color;
		Actor.SetBlendColor(color);
	}

	/// <inheritdoc/>
	public override void _ExitTree()
	{
		if (Enemy != null)
		{
			Enemy.OnDamaged -= Damaged;
			Enemy.OnRevived -= Revived;
		}
		base._ExitTree();
	}

	private void Damaged(object sender, EventArgs e)
	{
		Actor.SetHurt(true);
		HurtTimer.Start(0.75d);
	}

	private void Revived(object sender, EventArgs e)
	{
		if (IsInstanceValid(this))
			BattleManager.Instance.OnEnemyRevived(this);
	}
}
