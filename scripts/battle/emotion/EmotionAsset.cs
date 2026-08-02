using Godot;
using Path = System.IO.Path;

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

	/// <summary>
	/// A custom above-head label texture, if any. Takes priority over <see cref="LabelAtlasRow"/>.
	/// </summary>
	public Texture2D LabelTexture { get; private init; }

	/// <summary>
	/// A custom back portrait texture, if any. Takes priority over <see cref="FaceAtlasCell"/>.
	/// </summary>
	public Texture2D FaceTexture { get; private init; }

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
	/// Creates an asset that only changes the battlecard back portrait, leaving the
	/// above-head emotion label untouched (it keeps showing the actor's current emotion).
	/// </summary>
	/// <param name="faceX">The column of the back portrait atlas.</param>
	/// <param name="faceY">The row of the back portrait atlas.</param>
	public static EmotionAsset FaceOnly(int faceX, int faceY)
	{
		return new EmotionAsset
		{
			FaceAtlasCell = new Vector2I(faceX, faceY)
		};
	}

	/// <summary>
	/// Creates an asset with custom textures, loaded from the mods folder.<br/>
	/// Must be a full path from the mod's folder.<br/>
	/// Example: <c>MyMod/sprites/smug_label.png</c>.<br/>
	/// Recommended sizes: 98x22 for the label, 100x100 for the portrait.
	/// </summary>
	/// <param name="labelPath">The path to the above-head label texture, or null for no label override.</param>
	/// <param name="facePath">The path to the back portrait texture, or null for no portrait override.</param>
	public static EmotionAsset FromModTextures(string labelPath, string facePath)
	{
		return new EmotionAsset
		{
			LabelTexture = LoadModTexture(labelPath),
			FaceTexture = LoadModTexture(facePath)
		};
	}

	/// <summary>
	/// Creates an asset with custom textures loaded from the mods folder, taking regions into
	/// a spritesheet.<br/>
	/// Paths must be full paths from the mods folder, e.g. <c>MyMod/sprites/emotions.png</c>;
	/// both may point at the same sheet.<br/>
	/// Recommended region sizes: 98x22 for the label, 100x100 for the portrait.
	/// </summary>
	/// <param name="labelPath">The path to the spritesheet holding the above-head label, or null for no label override.</param>
	/// <param name="labelRegion">The region of the label within its sheet, or null to use the whole texture.</param>
	/// <param name="facePath">The path to the spritesheet holding the back portrait, or null for no portrait override.</param>
	/// <param name="faceRegion">The region of the portrait within its sheet, or null to use the whole texture.</param>
	public static EmotionAsset FromModTextures(string labelPath, Rect2? labelRegion, string facePath, Rect2? faceRegion)
	{
		return new EmotionAsset
		{
			LabelTexture = CreateRegionTexture(LoadModTexture(labelPath), labelRegion),
			FaceTexture = CreateRegionTexture(LoadModTexture(facePath), faceRegion)
		};
	}

	private static Texture2D CreateRegionTexture(Texture2D texture, Rect2? region)
	{
		if (texture == null || region == null)
			return texture;
		return new AtlasTexture { Atlas = texture, Region = region.Value };
	}

	private static Texture2D LoadModTexture(string path)
	{
		if (path == null)
			return null;
		if (string.IsNullOrWhiteSpace(path) || path.Contains("..") || path.Contains("://") || Path.IsPathRooted(path))
		{
			GD.PushError($"Invalid emotion asset path '{path}' (path traversal not allowed)");
			return null;
		}
		if (!FileAccess.FileExists("user://mods/" + path))
		{
			GD.PushError("Failed to find emotion asset at path: user://mods/" + path);
			return null;
		}
		return ImageTexture.CreateFromImage(Image.LoadFromFile("user://mods/" + path));
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
	/// Only overrides the back portrait, the above-head label keeps showing the current emotion.
	/// </summary>
	public static readonly EmotionAsset PlotArmor = FaceOnly(3, 2);
}
