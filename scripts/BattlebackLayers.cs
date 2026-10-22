using System.Collections.Generic;
using System.Linq;
using Godot;

namespace OmoriSandbox;

internal partial class BattlebackLayers : Node2D
{
	private const int FadeFrames = 20;

	[Export] private PackedScene LayerScene;

	private readonly List<BattlebackDisplayComponent> Slots = [null, null, null];
	private readonly Dictionary<BattlebackDisplayComponent, Tween> Fades = [];
	private readonly HashSet<BattlebackDisplayComponent> PendingRemovals = [];
	private Rect2? DisplayBounds;

	internal IReadOnlyList<string> Names() => Slots.Select(layer => layer?.CurrentName ?? "").ToList();

	internal void ConfigureDisplay(Rect2 bounds)
	{
		DisplayBounds = bounds;
		foreach (BattlebackDisplayComponent layer in Slots)
			layer?.ConfigureDisplay(bounds);
	}

	internal void Add(int id, string name)
	{
		id = Mathf.Max(1, id);
		if (id <= 2)
			return;
		while (Slots.Count <= id)
			Slots.Add(null);
		if (Slots[id] != null)
			return;
		BattlebackDisplayComponent layer = LayerScene.Instantiate<BattlebackDisplayComponent>();
		layer.Modulate = new Color(layer.Modulate, 0f);
		AddChild(layer);
		if (DisplayBounds is { } bounds)
			layer.ConfigureDisplay(bounds);
		layer.SetBattleback(name);
		Slots[id] = layer;
		UpdateDrawOrder();
		FadeLayer(layer, 1f, FadeFrames);
	}

	internal void Remove(int id)
	{
		BattlebackDisplayComponent layer = LayerAt(id);
		if (layer == null)
			return;
		PendingRemovals.Add(layer);
		FadeLayer(layer, 0f, FadeFrames);
	}

	internal void Change(int id, string name)
	{
		LayerAt(id)?.SetBattleback(name);
	}

	internal void Fade(int id, float opacity, int frames)
	{
		if (Mathf.Max(1, id) == 1)
		{
			GameManager.Instance.FadeBaseBattleback(opacity, frames);
			return;
		}
		BattlebackDisplayComponent layer = LayerAt(id);
		if (layer != null)
			FadeLayer(layer, opacity, frames);
	}

	internal void SetScroll(int id, Vector2 pixelsPerSecond)
	{
		BattlebackDisplayComponent layer = LayerAt(id);
		if (layer != null)
			layer.ScrollSpeedOverride = pixelsPerSecond;
	}

	internal void Clear()
	{
		foreach (Tween fade in Fades.Values)
			fade.Kill();
		Fades.Clear();
		PendingRemovals.Clear();
		foreach (BattlebackDisplayComponent layer in Slots)
			layer?.QueueFree();
		Slots.RemoveRange(3, Slots.Count - 3);
		GameManager.Instance.FadeBaseBattleback(1f, 0);
	}

	private BattlebackDisplayComponent LayerAt(int id)
	{
		id = Mathf.Max(1, id);
		return id > 2 && id < Slots.Count ? Slots[id] : null;
	}

	private void FadeLayer(BattlebackDisplayComponent layer, float opacity, int frames)
	{
		if (Fades.Remove(layer, out Tween running))
			running.Kill();
		if (frames <= 0)
		{
			layer.Modulate = new Color(layer.Modulate, opacity);
			FinishRemoval(layer);
			return;
		}
		Tween fade = layer.CreateTween();
		fade.TweenProperty(layer, "modulate:a", opacity, frames / 60f);
		fade.TweenCallback(Callable.From(() => FinishRemoval(layer)));
		Fades[layer] = fade;
	}

	private void FinishRemoval(BattlebackDisplayComponent layer)
	{
		if (layer.Modulate.A > 0f || !PendingRemovals.Remove(layer))
			return;
		Fades.Remove(layer);
		// VANILLA BUG: removing a layer shifts every higher layer down a slot, so a later removal by id can miss its layer
		Slots.RemoveAt(Slots.IndexOf(layer));
		layer.QueueFree();
		UpdateDrawOrder();
	}

	private void UpdateDrawOrder()
	{
		int index = 0;
		foreach (BattlebackDisplayComponent layer in Slots)
		{
			if (layer != null)
				MoveChild(layer, index++);
		}
	}
}
