using Godot;

namespace OmoriSandbox.Battle.Emotions;

/// <summary>
/// The assets shown for an emotion or animation override, including the above-head emotion label and the battlecard back portrait.
/// </summary>
public sealed class EmotionAsset
{
	/// <summary>
	/// The row into the vanilla above-head label atlas, if any.
	/// </summary>
	public int? LabelAtlasRow { get; private init; }

	/// <summary>
	/// The column and row into the vanilla back portrait atlas, if any.
	/// </summary>
	public Vector2I? FaceAtlasCell { get; private init; }

	private EmotionAsset() { }

	/// <summary>
	/// Creates an asset pointing into the built-in label and portrait atlases.
	/// </summary>
	/// <param name="labelRow">The row of the above-head label atlas.</param>
	/// <param name="faceX">The column of the back portrait atlas.</param>
	/// <param name="faceY">The row of the back portrait atlas.</param>
	public static EmotionAsset Vanilla(int labelRow, int faceX, int faceY)
	{
		return new EmotionAsset
		{
			LabelAtlasRow = labelRow,
			FaceAtlasCell = new Vector2I(faceX, faceY)
		};
	}

	/// <summary>
	/// The asset shown for toast actors.
	/// </summary>
	public static readonly EmotionAsset Toast = Vanilla(1, 1, 0);

	/// <summary>
	/// The asset shown for actors during the victory screen.
	/// </summary>
	public static readonly EmotionAsset Victory = Vanilla(0, 0, 0);

	/// <summary>
	/// The asset shown while Plot Armor is active.
	/// </summary>
	public static readonly EmotionAsset PlotArmor = Vanilla(1, 3, 2);
}
