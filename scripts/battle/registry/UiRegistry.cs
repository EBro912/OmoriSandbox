using Godot;
using OmoriSandbox.Menu;

namespace OmoriSandbox.Battle.Registry;

using static Database;

internal static class UiRegistry
{
	internal static void Register()
	{
		#region STATE ICONS
		foreach (string stateIcon in ResourceLoader.ListDirectory("res://assets/stateicons"))
		{
			Texture2D texture = ResourceLoader.Load<Texture2D>("res://assets/stateicons/" + stateIcon);
			AddStateIcon(stateIcon.GetBaseName(), texture);
		}
		#endregion

		#region MENU SKINS
		Texture2D battleAtlas = ResourceLoader.Load<Texture2D>("res://assets/atlases/battleATLAS.png");
		Texture2D oneColumnArt = ResourceLoader.Load<Texture2D>("res://assets/system/BattleCommands_DW_ATK_SKILL.png");
		AddBuiltInMenuSkin("Dreamworld", new MenuSkin(battleAtlas,
				new Rect2(654, 131, 179, 39), new Rect2(835, 131, 179, 39), new Rect2(654, 172, 179, 39), new Rect2(835, 172, 179, 39),
				battleAtlas, new Rect2(654, 377, 360, 39), new Rect2(654, 418, 360, 39))
			.WithOneColumnCommands(oneColumnArt, new Rect2(1, 1, 360, 40), new Rect2(1, 41, 360, 40)));
		AddBuiltInMenuSkin("Faraway", new MenuSkin(battleAtlas,
				new Rect2(654, 213, 179, 39), new Rect2(835, 213, 179, 39), new Rect2(654, 254, 179, 39), new Rect2(835, 254, 179, 39),
				battleAtlas, new Rect2(654, 458, 360, 41), new Rect2(654, 499, 360, 40))
			.WithOneColumnCommands(oneColumnArt, new Rect2(1, 1, 360, 40), new Rect2(1, 41, 360, 40)));
		AddBuiltInMenuSkin("Blackspace", new MenuSkin(battleAtlas,
				new Rect2(654, 131, 179, 39), new Rect2(835, 131, 179, 39), new Rect2(654, 172, 179, 39), new Rect2(835, 172, 179, 39),
				battleAtlas, new Rect2(654, 295, 360, 39), new Rect2(654, 335, 360, 40))
			{ ForcesItemless = true, BlackspaceEnergyBar = true }
			.WithOneColumnCommands(battleAtlas, new Rect2(292, 480, 360, 40), new Rect2(292, 520, 360, 40)));
		#endregion

		#region WINDOW SKINS
		WindowSkins.Add(DefaultWindowSkin, new WindowSkin(ResourceLoader.Load<Texture2D>("res://assets/system/Window.png"), false));
		WindowSkinOrder.Add(DefaultWindowSkin);
		#endregion
	}
}
