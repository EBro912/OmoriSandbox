using System.Collections.Generic;
using Godot;
using OmoriSandbox.Actors;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Modifier;
using OmoriSandbox.Editor;

namespace OmoriSandbox;

// renders an actor's state icons into a container, reusing existing ones if possible
internal static class StateIconRenderer
{
	internal static void Render(Container container, Actor actor)
	{
		List<(Texture2D Texture, string Tooltip, bool Blinking)> desired = [];
		bool blinkExpiring = SettingsMenuManager.Instance.BlinkExpiringStateIcons;
		foreach (StatModifier modifier in actor.StatModifiers.Values)
		{
			foreach (StateIcon icon in modifier.GetStateIcons() ?? [])
			{
				string tooltip = modifier.TurnsLeft > -1
					? icon.Description + "\nTurns Left: " + modifier.TurnsLeft
					: icon.Description;
				bool blinking = blinkExpiring && modifier.TurnsLeft is > -1 and <= 1;
				if (Database.TryGetStateIcon(icon.AssetName, out Texture2D texture))
					desired.Add((texture, tooltip, blinking));
				else
					GD.PrintErr("Unknown state icon texture: " + icon.AssetName);
			}
		}

		int existing = container.GetChildCount();
		for (int i = 0; i < desired.Count; i++)
		{
			if (i < existing)
			{
				StateIconRect rect = container.GetChild<StateIconRect>(i);
				rect.Texture = desired[i].Texture;
				rect.TooltipText = desired[i].Tooltip;
				rect.Blinking = desired[i].Blinking;
			}
			else
			{
				container.AddChild(new StateIconRect
				{
					Texture = desired[i].Texture,
					TooltipText = desired[i].Tooltip,
					Blinking = desired[i].Blinking
				});
			}
		}

		for (int i = existing - 1; i >= desired.Count; i--)
			container.GetChild(i).Free();
	}
}
