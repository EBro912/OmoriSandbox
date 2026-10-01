namespace OmoriSandbox.Battle;

/// <summary>
/// What an action records in <see cref="Actors.Actor.LastTarget"/> when it starts.
/// </summary>
public enum LastTargetRecord
{
	/// <summary>
	/// The action's last target. This is the default.
	/// </summary>
	Target,
	/// <summary>
	/// The user itself.
	/// </summary>
	User,
	/// <summary>
	/// Nothing. The previous record stays.
	/// </summary>
	None
}
