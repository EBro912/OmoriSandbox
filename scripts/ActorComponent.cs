using Godot;
using OmoriSandbox.Actors;

namespace OmoriSandbox;

/// <summary>
/// The physical node in the scene for an actor in a battle, either an <see cref="EnemyComponent"/> or a <see cref="PartyMemberComponent"/>.
/// </summary>
public abstract partial class ActorComponent : Node
{
	/// <summary>
	/// The actor this component is attached to.
	/// </summary>
	public abstract Actor Actor { get; }

	internal abstract void ShowDamageNumber(int damage, DamageType type, bool critical);

	/// <inheritdoc/>
	public override void _ExitTree()
	{
		Actor?.Unbind(this);
	}
}
