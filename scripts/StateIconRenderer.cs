using Godot;
using OmoriSandbox.Actors;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Modifier;
using System.Collections.Generic;

namespace OmoriSandbox;

// renders an actor's state icons into a container, reusing existing ones if possible
internal static class StateIconRenderer
{
	internal static void Render(Container container, Actor actor)
	{
		List<(Texture2D Texture, string Tooltip)> desired = [];
		foreach (StatModifier modifier in actor.StatModifiers.Values)
		{
			foreach (StateIcon icon in modifier.GetStateIcons())
			{
				string tooltip = modifier.TurnsLeft > -1
					? icon.Description + "\nTurns Left: " + modifier.TurnsLeft
					: icon.Description;
				if (Database.TryGetStateIcon(icon.AssetName, out Texture2D texture))
					desired.Add((texture, tooltip));
				else
					GD.PrintErr("Unknown state icon texture: " + icon.AssetName);
			}
		}

		int existing = container.GetChildCount();
		for (int i = 0; i < desired.Count; i++)
		{
			if (i < existing)
			{
				TextureRect rect = container.GetChild<TextureRect>(i);
				rect.Texture = desired[i].Texture;
				rect.TooltipText = desired[i].Tooltip;
			}
			else
			{
				container.AddChild(new TextureRect
				{
					Texture = desired[i].Texture,
					TooltipText = desired[i].Tooltip
				});
			}
		}

		for (int i = existing - 1; i >= desired.Count; i--)
			container.GetChild(i).Free();
	}
}
