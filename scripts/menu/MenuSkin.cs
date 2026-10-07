using Godot;

namespace OmoriSandbox.Menu;

/// <summary>
/// Button art for the battle menus: ATTACK, SKILL, SNACK and TOY, FIGHT and RUN, and optionally the one-column ATTACK and SKILL used by Itemless Mode.
/// </summary>
/// <remarks>
/// The buttons are drawn where the base game draws them, so each region should match the built-in buttons' size:
/// 179x39 for ATTACK, SKILL, SNACK and TOY, 360x39 for FIGHT and RUN, and 360x40 for the one-column ATTACK and SKILL.<br/>
/// Register a skin with <c>Mod.RegisterMenuSkin</c>, or put a sheet in your mod's <c>menuskins</c> folder (see <see cref="FromSheet"/>).
/// A skin without one-column art uses the Dreamworld one-column art in Itemless Mode.
/// </remarks>
public sealed class MenuSkin
{
	/// <summary>The texture holding ATTACK, SKILL, SNACK and TOY.</summary>
	public Texture2D CommandsTexture { get; }
	/// <summary>The ATTACK region of <see cref="CommandsTexture"/>.</summary>
	public Rect2 Attack { get; }
	/// <summary>The SKILL region of <see cref="CommandsTexture"/>.</summary>
	public Rect2 Skill { get; }
	/// <summary>The SNACK region of <see cref="CommandsTexture"/>.</summary>
	public Rect2 Snack { get; }
	/// <summary>The TOY region of <see cref="CommandsTexture"/>.</summary>
	public Rect2 Toy { get; }
	/// <summary>The texture holding FIGHT and RUN.</summary>
	public Texture2D PartyCommandsTexture { get; }
	/// <summary>The FIGHT region of <see cref="PartyCommandsTexture"/>.</summary>
	public Rect2 Fight { get; }
	/// <summary>The RUN region of <see cref="PartyCommandsTexture"/>.</summary>
	public Rect2 Run { get; }
	/// <summary>The texture holding the one-column ATTACK and SKILL, or null if the skin has none.</summary>
	public Texture2D OneColumnTexture { get; private set; }
	/// <summary>The one-column ATTACK region of <see cref="OneColumnTexture"/>.</summary>
	public Rect2 OneColumnAttack { get; private set; }
	/// <summary>The one-column SKILL region of <see cref="OneColumnTexture"/>.</summary>
	public Rect2 OneColumnSkill { get; private set; }
	/// <summary>Whether the skin has its own one-column ATTACK and SKILL art.</summary>
	public bool HasOneColumnCommands => OneColumnTexture != null;

	internal bool ForcesItemless { get; init; }
	internal bool BlackspaceEnergyBar { get; init; }

	/// <summary>
	/// Creates a skin from the battle command and party command art.
	/// </summary>
	/// <param name="commandsTexture">The texture holding ATTACK, SKILL, SNACK and TOY.</param>
	/// <param name="attack">The ATTACK region.</param>
	/// <param name="skill">The SKILL region.</param>
	/// <param name="snack">The SNACK region.</param>
	/// <param name="toy">The TOY region.</param>
	/// <param name="partyCommandsTexture">The texture holding FIGHT and RUN.</param>
	/// <param name="fight">The FIGHT region.</param>
	/// <param name="run">The RUN region.</param>
	public MenuSkin(Texture2D commandsTexture, Rect2 attack, Rect2 skill, Rect2 snack, Rect2 toy,
		Texture2D partyCommandsTexture, Rect2 fight, Rect2 run)
	{
		CommandsTexture = commandsTexture;
		Attack = attack;
		Skill = skill;
		Snack = snack;
		Toy = toy;
		PartyCommandsTexture = partyCommandsTexture;
		Fight = fight;
		Run = run;
	}

	/// <summary>
	/// Adds the one-column ATTACK and SKILL art used by Itemless Mode.
	/// </summary>
	/// <param name="texture">The texture holding both buttons.</param>
	/// <param name="attack">The ATTACK region.</param>
	/// <param name="skill">The SKILL region.</param>
	/// <returns>This skin.</returns>
	public MenuSkin WithOneColumnCommands(Texture2D texture, Rect2 attack, Rect2 skill)
	{
		OneColumnTexture = texture;
		OneColumnAttack = attack;
		OneColumnSkill = skill;
		return this;
	}

	/// <summary>
	/// Creates a skin from one sheet in the <c>menuskins</c> folder layout.
	/// </summary>
	/// <remarks>
	/// The sheet is 360 pixels wide and stacks the base game's sheets from top to bottom:
	/// the 2x2 battle commands (rows 0-79), FIGHT and RUN (rows 80-159) and, optionally, the one-column ATTACK and SKILL (rows 160-239).
	/// So it is either 360x160 or 360x240.
	/// </remarks>
	/// <param name="sheet">The sheet texture.</param>
	/// <returns>The skin, or null if the sheet isn't 360x160 or 360x240.</returns>
	public static MenuSkin FromSheet(Texture2D sheet)
	{
		if (sheet == null || sheet.GetWidth() != 360 || (sheet.GetHeight() != 160 && sheet.GetHeight() != 240))
			return null;
		MenuSkin skin = new(sheet, new Rect2(0, 0, 179, 39), new Rect2(181, 0, 179, 39),
			new Rect2(0, 41, 179, 39), new Rect2(181, 41, 179, 39),
			sheet, new Rect2(0, 80, 360, 39), new Rect2(0, 121, 360, 39));
		if (sheet.GetHeight() == 240)
			skin.WithOneColumnCommands(sheet, new Rect2(0, 160, 360, 40), new Rect2(0, 200, 360, 40));
		return skin;
	}
}
