using System;
using Godot;

namespace OmoriSandbox;

internal partial class StateIconRect : TextureRect
{
	internal bool Blinking;

	public override void _PhysicsProcess(double delta)
	{
		float alpha = Blinking ? (float)(0.75 + Math.Sin(Engine.GetPhysicsFrames() * 0.2) * 0.25) : 1f;
		Modulate = Modulate with { A = alpha };
	}
}
