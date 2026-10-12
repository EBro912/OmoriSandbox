using System;
using Godot;
using OmoriSandbox.Editor;

namespace OmoriSandbox.Menu;

internal sealed class MenuRepeat
{
	private static readonly (string Action, Vector2I Direction)[] Directions =
	[
		("MenuUp", Vector2I.Up),
		("MenuDown", Vector2I.Down),
		("MenuLeft", Vector2I.Left),
		("MenuRight", Vector2I.Right)
	];

	private static readonly string[] OtherActions = ["Accept", "Back", "SwitchSides"];

	private string HeldAction;
	private Vector2I HeldDirection;
	private double HeldTime;
	private double NextRepeat;

	public Vector2I Direction { get; private set; }
	public bool Fresh { get; private set; }

	public void Update(double delta)
	{
		Direction = Vector2I.Zero;
		Fresh = false;

		foreach ((string action, Vector2I direction) in Directions)
		{
			if (!Input.IsActionJustPressed(action))
				continue;
			HeldAction = action;
			HeldDirection = direction;
			HeldTime = 0;
			NextRepeat = SettingsMenuManager.Instance.SelectionHoldTime;
			Direction = direction;
			Fresh = true;
			return;
		}

		foreach (string action in OtherActions)
		{
			if (Input.IsActionJustPressed(action))
				HeldAction = null;
		}

		if (HeldAction == null)
			return;
		if (!Input.IsActionPressed(HeldAction))
		{
			HeldAction = null;
			return;
		}

		double step = Math.Min(delta, 2.0 / 60.0 * Engine.TimeScale);
		HeldTime += step;
		if (HeldTime + step / 2 < NextRepeat)
			return;
		NextRepeat += SettingsMenuManager.Instance.SelectionChangeSpeed;
		Direction = HeldDirection;
	}
}
