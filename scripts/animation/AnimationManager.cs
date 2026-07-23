using Godot;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using OmoriSandbox.Actors;
using OmoriSandbox.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OmoriSandbox.Animation;

/// <summary>
/// Handles all animation related functionality, including playing animations and screen shake.
/// </summary>
public partial class AnimationManager : Node
{
	/// <summary>
	/// Fired whenever all animations finish playing.
	/// </summary>
	[Signal]
	public delegate void AnimationFinishedEventHandler();

	[Export] private TextureRect Battleback;
	[Export] private AnimatedSprite2D ReleaseEnergy;
	[Export] private AnimatedSprite2D ReleaseEnergyBasil;
	[Export] private AnimatedSprite2D RedHands;
	[Export] private AnimatedSprite2D FlowerCrown;
	[Export] private ColorRect Photograph;
	[Export] private TextureRect Snaley;
	[Export] private TextureRect HumphreySwarm;
	[Export] private TextureRect Encore;
	[Export] private TextureRect Cherish;
	[Export] private AnimatedSprite2D HumphreySwallow;
	[Export] private AnimatedSprite2D HumphreyFaceSwallow;
	[Export] private PackedScene PerfectheartOverlaySprite;
	[Export] private Node2D PerfectheartOverlayParent;
	[Export] private Node2D FullScreenEffectNode;
	[Export] private ColorRect ScreenTint;

	private Dictionary<int, RPGMAnimatedSprite> Animations = [];

	private const float FPS = 15f;
	private float FrameDuration = 1f / FPS;
	private float FrameTimer = 0f;
	private List<PlayingAnimation> PlayingAnimations = [];

	private float Shake = 0f;
	private float ShakePwr = 0f;
	private float ShakeSpd = 0f;
	private int ShakeDuration = 0;
	private float ShakeDirection = -1f;

	public static AnimationManager Instance { get; private set; }

	public override void _EnterTree()
	{
		Instance = this;
	}

	internal void Init()
	{
		string data = FileAccess.GetFileAsString("res://animations/animations.json");
		List<AnimationInfo> animationData = JsonConvert.DeserializeObject<List<AnimationInfo>>(data);
		foreach (AnimationInfo info in animationData)
		{
			bool missingTexture = string.IsNullOrWhiteSpace(info.Texture);
			bool missingAltTexture = string.IsNullOrWhiteSpace(info.AltTexture);

			if (missingTexture && missingAltTexture)
				continue;

			RPGMAnimatedSprite animation = new(info.Id, info.Layer,
				missingTexture ? null : ResourceLoader.Load<Texture2D>($"res://assets/animations/{info.Texture}.png"),
				missingAltTexture
					? null
					: ResourceLoader.Load<Texture2D>($"res://assets/animations/{info.AltTexture}.png"));

			foreach (float[][] frame in info.Frames)
			{
				List<Frame> frames = [];
				foreach (float[] f in frame)
				{
					frames.Add(new Frame((int)f[0], f[1], f[2], f[3], f[4], f[5] == 1, f[6],
					f.Length > 7 ? (int)f[7] : 0));
				}

				animation.CreateFrame(frames);
			}

			foreach (SFXInfo sfx in info.SFX)
			{
				animation.SetFrameSFX(sfx.Frame, new SFX(sfx.Name, sfx.Pitch, sfx.Volume));
			}

			foreach (ShakeInfo shake in info.Shake)
			{
				animation.SetFrameShake(shake.Frame, shake.Power, shake.Speed, shake.Duration);
			}

			if (!Animations.TryAdd(info.Id, animation))
			{
				GD.PrintErr("Unable to add animation ID " + info.Id + ", is there a duplicate?");
			}
		}

		GD.Print($"Loaded {Animations.Count} animations");
	}

	internal void LoadModded(string root, string dir, ModLoadReport report)
	{
		string data = FileAccess.GetFileAsString($"user://mods/{root}/{dir}/Animations.json");
		List<RPGMakerAnimation> animationData = JsonConvert.DeserializeObject<List<RPGMakerAnimation>>(data);
		if (animationData == null)
		{
			report.Error("animations", "Animations.json", "Failed to parse file");
			return;
		}
		RegEx layerRegex = new();
		layerRegex.Compile("<[a-z]{5}:(\\d)>");
		int total = 0;
		int attempted = 0;
		foreach (RPGMakerAnimation animation in animationData)
		{
			try
			{
				bool missingTexture = string.IsNullOrEmpty(animation.Animation1Name);
				bool missingAltTexture = string.IsNullOrEmpty(animation.Animation2Name);
				if (missingTexture && missingAltTexture)
					continue;

				attempted++;
				if (Animations.ContainsKey(animation.Id))
				{
					report.Warn("animations", animation.Name, $"Skipping modded animation, ID {animation.Id} is already taken");
					report.CountSkipped();
					continue;
				}

				int layer = 0;
				RegExMatch match = layerRegex.Search(animation.Name);
				if (match != null)
					int.TryParse(match.GetString(1), out layer);

				Texture2D texture = null;
				Texture2D altTexture = null;

				if (!missingTexture)
				{
					texture = LoadModTexture(root, dir, animation.Animation1Name);
					if (texture == null)
					{
						report.Error("animations", animation.Name,
							$"Unable to find texture {animation.Animation1Name}.png for modded animation {animation.Id}");
						report.CountSkipped();
						continue;
					}
				}

				if (!missingAltTexture)
				{
					altTexture = LoadModTexture(root, dir, animation.Animation2Name);
					if (altTexture == null)
					{
						report.Error("animations", animation.Name,
							$"Unable to find alt texture {animation.Animation2Name}.png for modded animation {animation.Id}");
						report.CountSkipped();
						continue;
					}
				}

				RPGMAnimatedSprite anim = new(animation.Id, layer, texture, altTexture);
				foreach (float[][] frame in animation.Frames)
				{
					List<Frame> frames = [];
					frames.AddRange(frame.Select(f => new Frame((int)f[0], f[1], f[2], f[3], f[4], f[5] == 1, f[6],
					f.Length > 7 ? (int)f[7] : 0)));
					anim.CreateFrame(frames);
				}

				foreach (Timings timing in animation.Timings)
				{
					if (timing.Se == null)
						continue;
					if (timing.Se.Name == "ft_doShake")
						anim.SetFrameShake(timing.Frame, timing.FlashColor[0], timing.FlashColor[1], timing.FlashDuration);
					else
					{
						if (!LoadModSFX(root, dir, timing.Se.Name))
							report.Warn("animations", animation.Name,
								$"Unable to find SFX {timing.Se.Name}.ogg for modded animation {animation.Id}");
						anim.SetFrameSFX(timing.Frame, new SFX(timing.Se.Name, timing.Se.Pitch, timing.Se.Volume));
					}
				}

				Animations.Add(animation.Id, anim);
				report.CountLoaded();
				total++;
			}
			catch (Exception ex)
			{
				report.Error("animations", animation.Name ?? animation.Id.ToString(), $"Failed to convert: {ex.Message}");
				report.CountSkipped();
			}
		}

		GD.Print($"{root}: Converted {total}/{attempted} RPGM animations");
	}

	internal void LoadDeltaPatch(string root, string dir, ModLoadReport report)
	{
		string data = FileAccess.GetFileAsString($"user://mods/{root}/{dir}/Animations.jsond");
		if (string.IsNullOrEmpty(data))
		{
			report.Error("animations", "Animations.jsond", "Failed to read file");
			return;
		}

		JArray ops = JArray.Parse(data);
		RegEx layerRegex = new();
		layerRegex.Compile("<[a-z]{5}:(\\d)>");

		// group operations by animation ID
		Dictionary<int, List<JObject>> grouped = [];
		foreach (JToken op in ops)
		{
			string[] segments = op["path"].ToString().TrimStart('/').Split('/');
			if (int.TryParse(segments[0], out int id))
			{
				if (!grouped.TryGetValue(id, out List<JObject> list))
				{
					list = [];
					grouped[id] = list;
				}

				list.Add((JObject)op);
			}
		}

		int total = 0;
		int attempted = 0;
		foreach (var (id, operations) in grouped)
		{
			try
			{
				string name = null;
				string textureName = null;
				string altTextureName = null;
				SortedDictionary<int, List<float[]>> frameData = [];
				Dictionary<int, SortedDictionary<int, float[]>> frameCellData = [];
				List<Timings> timingData = [];

				foreach (JObject op in operations)
				{
					string[] segments = op["path"].ToString().TrimStart('/').Split('/');
					if (segments.Length < 2) continue;

					switch (segments[1])
					{
						case "name":
							name = op["value"].ToString();
							break;
						case "animation1Name":
							textureName = op["value"].ToString();
							break;
						case "animation2Name":
							altTextureName = op["value"].ToString();
							break;
						case "frames" when segments.Length == 3:
						{
							int frameIdx = int.Parse(segments[2]);
							float[][] cells = op["value"].ToObject<float[][]>();
							frameData[frameIdx] = [..cells];
							break;
						}
						case "frames" when segments.Length == 4:
						{
							int frameIdx = int.Parse(segments[2]);
							int cellIdx = int.Parse(segments[3]);
							float[] cell = op["value"].ToObject<float[]>();
							if (!frameCellData.TryGetValue(frameIdx, out SortedDictionary<int, float[]> cellDict))
							{
								cellDict = [];
								frameCellData[frameIdx] = cellDict;
							}

							cellDict[cellIdx] = cell;
							break;
						}
						case "timings" when segments.Length == 3:
							timingData.Add(op["value"].ToObject<Timings>());
							break;
					}
				}

				// skip blank entries
				if (frameData.Count == 0 && frameCellData.Count == 0 && timingData.Count == 0
				    && textureName == null && altTextureName == null)
					continue;

				attempted++;
				if (Animations.ContainsKey(id))
				{
					report.Warn("animations", id.ToString(), $"Skipping delta-patched animation, ID {id} is already taken");
					report.CountSkipped();
					continue;
				}

				int layer = 0;
				if (name != null)
				{
					RegExMatch match = layerRegex.Search(name);
					if (match != null)
						int.TryParse(match.GetString(1), out layer);
				}

				Texture2D texture = null;
				Texture2D altTexture = null;

				if (textureName != null)
				{
					texture = LoadModTexture(root, dir, textureName);
					if (texture == null)
					{
						report.Error("animations", id.ToString(),
							$"Unable to find texture {textureName}.png for delta-patched animation {id}");
						report.CountSkipped();
						continue;
					}
				}

				if (altTextureName != null)
				{
					altTexture = LoadModTexture(root, dir, altTextureName);
					if (altTexture == null)
					{
						report.Error("animations", id.ToString(),
							$"Unable to find alt texture {altTextureName}.png for delta-patched animation {id}");
						report.CountSkipped();
						continue;
					}
				}

				if (texture == null && altTexture == null)
				{
					report.Warn("animations", id.ToString(), $"Skipping delta-patched animation {id}, it has no textures");
					report.CountSkipped();
					continue;
				}

				RPGMAnimatedSprite anim = new(id, layer, texture, altTexture);

				int maxFrame = 0;
				if (frameData.Count > 0)
					maxFrame = Math.Max(maxFrame, frameData.Keys.Max() + 1);
				if (frameCellData.Count > 0)
					maxFrame = Math.Max(maxFrame, frameCellData.Keys.Max() + 1);

				for (int i = 0; i < maxFrame; i++)
				{
					List<Frame> frames;
					if (frameData.TryGetValue(i, out List<float[]> cells))
						frames = cells.Select(f => new Frame((int)f[0], f[1], f[2], f[3], f[4], f[5] == 1, f[6],
							f.Length > 7 ? (int)f[7] : 0)).ToList();
					else
						frames = [];

					if (frameCellData.TryGetValue(i, out SortedDictionary<int, float[]> cellPatches))
					{
						foreach (var (cellIdx, cellData) in cellPatches)
						{
							Frame newFrame = new((int)cellData[0], cellData[1], cellData[2], cellData[3], cellData[4],
								cellData[5] == 1, cellData[6], cellData.Length > 7 ? (int)cellData[7] : 0);
							while (frames.Count <= cellIdx)
								frames.Add(new Frame());
							frames[cellIdx] = newFrame;
						}
					}

					anim.CreateFrame(frames);
				}

				foreach (Timings timing in timingData)
				{
					if (timing.Se == null)
						continue;
					if (timing.Se.Name == "ft_doShake")
						anim.SetFrameShake(timing.Frame, timing.FlashColor[0], timing.FlashColor[1], timing.FlashDuration);
					else
					{
						if (!LoadModSFX(root, dir, timing.Se.Name))
							report.Warn("animations", id.ToString(),
								$"Unable to find SFX {timing.Se.Name}.ogg for modded animation {id}");
						anim.SetFrameSFX(timing.Frame, new SFX(timing.Se.Name, timing.Se.Pitch, timing.Se.Volume));
					}
				}

				Animations.Add(id, anim);
				report.CountLoaded();
				total++;
			}
			catch (Exception ex)
			{
				report.Error("animations", id.ToString(), $"Failed to apply delta patch: {ex.Message}");
				report.CountSkipped();
			}
		}

		GD.Print($"{root}: Loaded {total}/{attempted} delta-patched animations");
	}

	private Texture2D LoadModTexture(string root, string dir, string name)
	{
		string path = $"user://mods/{root}/{dir}/{name}.png";
		if (FileAccess.FileExists(path))
			return ImageTexture.CreateFromImage(Image.LoadFromFile(path));
		path = $"res://assets/animations/{name}.png";
		if (ResourceLoader.Exists(path))
			return ResourceLoader.Load<Texture2D>(path);
		return null;
	}

	private bool LoadModSFX(string root, string dir, string name)
	{
		string path = $"user://mods/{root}/{dir}/{name}.ogg";
		if (FileAccess.FileExists(path))
			return AudioManager.Instance.LoadCustomSFX(path);
		return ResourceLoader.Exists($"res://audio/sfx/{name}.ogg");
	}

	public override void _Process(double delta)
	{
		if (PlayingAnimations.Count == 0)
			return;

		FrameTimer += (float)delta;

		if (FrameTimer >= FrameDuration)
		{
			FrameTimer -= FrameDuration;
			NextFrame();
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		// physics process runs at 60 fps, just like screen shake
		if (ShakeDuration > 0 || Shake != 0f)
		{
			UpdateShake();
		}

		float x = 0f;
		x += (float)Math.Round(Shake) - 640f;
		Battleback.Position = new Vector2(x, 0);
	}

	private void NextFrame()
	{
		for (int i = PlayingAnimations.Count - 1; i >= 0; i--)
		{
			// returns true if we're out of frames
			if (PlayingAnimations[i].AdvanceFrame())
			{
				PlayingAnimations[i].EmitSignal(PlayingAnimation.SignalName.Finished);
				PlayingAnimations[i].QueueFree();
				PlayingAnimations.RemoveAt(i);
				if (PlayingAnimations.Count == 0)
				{
					FrameTimer = 0f;
					EmitSignal(SignalName.AnimationFinished);
					return;
				}

				continue;
			}

			if (PlayingAnimations[i].Animation.TryGetFrameSFX(PlayingAnimations[i].CurrentFrame, out List<SFX> sfx))
			{
				sfx.ForEach(AudioManager.Instance.PlaySFX);
			}

			if (PlayingAnimations[i].Animation.TryGetFrameShake(PlayingAnimations[i].CurrentFrame, out Shake shake))
			{
				InitShake(shake);
			}
		}
	}

	private void UpdateShake()
	{
		float delta = (ShakePwr * (2f * ShakeSpd) * ShakeDirection) / 5f;
		if (ShakeDuration <= 1 && Shake * (Shake + delta) < 0)
			Shake = 0;
		else
			Shake += delta;
		if (Shake > ShakePwr * 2f)
			ShakeDirection = -1;
		if (Shake < -ShakePwr * 2f)
			ShakeDirection = 1;
		ShakePwr *= 0.9f;
		ShakeDuration--;

		// check for any remaining shake and snap back to the neutral position
		if (ShakeDuration <= 0 && Math.Abs(delta) * 10f < 0.5f)
		{
			Shake = 0f;
			ShakePwr = 0f;
		}
	}

	/// <summary>
	/// Initializes a new screenshake that will begin on the next valid frame.
	/// Calling this method while a shake is already happening will stop the currently playing one.
	/// </summary>
	public void InitShake(Shake shake)
	{
		Battleback.Position = new Vector2(-640, 0);
		Shake = 0f;
		ShakePwr = shake.Power;
		ShakeSpd = shake.Speed;
		ShakeDuration = shake.Duration;
	}

	private void ResetShake()
	{
		Battleback.Position = new Vector2(-640, 0);
		Shake = 0f;
		ShakePwr = 0f;
		ShakeSpd = 0f;
		ShakeDuration = 0;
	}

	/// <summary>
	/// Plays an animation with the given <paramref name="id"/> centered on the given <paramref name="target"/>.<br/>
	/// Use <see cref="WaitForAnimation(int, Actor)"/> if you want to wait for the animation to finish.
	/// </summary>
	/// <param name="id">The animation ID to play. Uses the same ID numbers as OMORI for all vanilla animations.</param>
	/// <param name="target">The <see cref="Actor"/> that this animation will play centered on.</param>
	/// Mainly used for animation layering, such as skill animations that target enemies and need to display underneath the UI.</param>
	public void PlayAnimation(int id, Actor target)
	{
		StartAnimation(id, target.CenterPoint, target is Enemy);
	}

	/// <summary>
	/// Plays an animation with the given <paramref name="id"/> centered on the given <paramref name="position"/>.<br/>
	/// Use <see cref="WaitForAnimation(int, Vector2, bool)"/> if you want to wait for the animation to finish.
	/// </summary>
	/// <param name="id">The animation ID to play. Uses the same ID numbers as OMORI for all vanilla animations.</param>
	/// <param name="position">The coordinates to play the animation at.</param>
	/// <param name="targetsEnemy">Whether the animation should play on the enemy layer.</param>
	public void PlayAnimation(int id, Vector2 position, bool targetsEnemy)
	{
		StartAnimation(id, position, targetsEnemy);
	}

	/// <summary>
	/// Plays an animation with the given <paramref name="id"/> centered on the screen.<br/>
	/// Use <see cref="WaitForScreenAnimation(int, bool)"/> if you want to wait for the animation to finish.
	/// </summary>
	/// <param name="id">The animation ID to play. Uses the same ID numbers as OMORI for all vanilla animations.</param>
	/// <param name="targetsEnemy">Whether this animation targets an enemy.<br/>
	/// Mainly used for animation layering, such as skill animations that target enemies and need to display underneath the UI.</param>
	public void PlayScreenAnimation(int id, bool targetsEnemy)
	{
		StartAnimation(id, new Vector2(320, 240), targetsEnemy);
	}

	/// <summary>
	/// Plays an animation with the given <paramref name="id"/> centered on the given <paramref name="target"/>, and waits for it to finish.<br/>
	/// Use <see cref="PlayAnimation(int, Actor)"/> if you want the animation to play without waiting.
	/// </summary>
	/// <param name="id">The animation ID to play. Uses the same ID numbers as OMORI for all vanilla animations.</param>
	/// <param name="target">The <see cref="Actor"/> that this animation will play centered on.</param>
	/// Mainly used for animation layering, such as skill animations that target enemies and need to display underneath the UI.</param>
	/// <returns>An awaitable <see cref="Task"/> that will complete whenever the animation finishes playing.</returns>
	public async Task WaitForAnimation(int id, Actor target)
	{
		PlayingAnimation playing = StartAnimation(id, target.CenterPoint, target is Enemy);
		if (playing == null) return;
		await ToSignal(playing, PlayingAnimation.SignalName.Finished);
	}

	/// <summary>
	/// Plays an animation with the given <paramref name="id"/> centered on the given <paramref name="position"/> and waits for it to finish.<br/>
	/// Use <see cref="PlayAnimation(int, Vector2, bool)"/> if you want the animation to play without waiting.
	/// </summary>
	/// <param name="id">The animation ID to play. Uses the same ID numbers as OMORI for all vanilla animations.</param>
	/// <param name="position">The coordinates to play the animation at.</param>
	/// <param name="targetsEnemy">Whether the animation should play on the enemy layer.</param>
	public async Task WaitForAnimation(int id, Vector2 position, bool targetsEnemy)
	{
		PlayingAnimation playing = StartAnimation(id, position, targetsEnemy);
		if (playing == null) return;
		await ToSignal(playing, PlayingAnimation.SignalName.Finished);
	}

	/// <summary>
	/// Plays an animation with the given <paramref name="id"/> centered on the screen, and waits for it to finish.<br/>
	/// Use <see cref="PlayScreenAnimation(int, bool)"/> if you want the animation to play without waiting.
	/// </summary>
	/// <param name="id">The animation ID to play. Uses the same ID numbers as OMORI for all vanilla animations.</param>
	/// <param name="targetsEnemy">Whether this animation targets an enemy.<br/>
	/// Mainly used for animation layering, such as skill animations that target enemies and need to display underneath the UI.</param>
	/// <returns>An awaitable <see cref="Task"/> that will complete whenever the animation finishes playing.</returns>
	public async Task WaitForScreenAnimation(int id, bool targetsEnemy)
	{
		PlayingAnimation playing = StartAnimation(id, new Vector2(320, 240), targetsEnemy);
		if (playing == null) return;
		await ToSignal(playing, PlayingAnimation.SignalName.Finished);
	}

	/// <summary>
	/// Plays the given <paramref name="animationName"/> from the <see cref="SpriteFrames"/> at the specified <paramref name="position"/>.
	/// </summary>
	/// <param name="frames">The <see cref="SpriteFrames"/> to use.</param>
	/// <param name="animationName">The animation from the <see cref="SpriteFrames"/> to play.</param>
	/// <param name="position">The position to show the animation at.</param>
	/// <param name="layer">The layer to show the animation on.</param>
	public void PlaySpriteFrames(SpriteFrames frames, string animationName, Vector2 position, int layer = 0)
	{
		AnimatedSprite2D sprite = new()
		{
			SpriteFrames = frames,
			Animation = animationName,
			Position = position,
			ZIndex = layer
		};
		sprite.SpriteFrames.SetAnimationLoop(animationName, false);
		AddChild(sprite);
		sprite.Play();
		void Finished()
		{
			sprite.AnimationFinished -= Finished;
			RemoveChild(sprite);
			sprite.QueueFree();
		}
		sprite.AnimationFinished += Finished;
	}

	/// <summary>
	/// Plays the given <paramref name="animationName"/> from the <see cref="SpriteFrames"/> at the specified <paramref name="position"/>, and waits for it to finish.
	/// </summary>
	/// <param name="frames">The <see cref="SpriteFrames"/> to use.</param>
	/// <param name="animationName">The animation from the <see cref="SpriteFrames"/> to play.</param>
	/// <param name="position">The position to show the animation at.</param>
	/// <param name="layer">The layer to show the animation on.</param>
	public async Task WaitForPlaySpriteFrames(SpriteFrames frames, string animationName, Vector2 position,
		int layer = 0)
	{
		AnimatedSprite2D sprite = new()
		{
			SpriteFrames = frames,
			Animation = animationName,
			Position = position,
			ZIndex = layer
		};
		sprite.SpriteFrames.SetAnimationLoop(animationName, false);
		AddChild(sprite);
		sprite.Play();
		await ToSignal(sprite, AnimatedSprite2D.SignalName.AnimationFinished);
		RemoveChild(sprite);
		sprite.QueueFree();
	}

	/// <summary>
	/// Plays the Omori version of the Release Energy animation, and waits for it to finish.
	/// </summary>
	/// <returns>An awaitable <see cref="Task"/> that will complete whenever the animation finishes playing.</returns>
	public async Task WaitForReleaseEnergy()
	{
		ReleaseEnergy.Visible = true;
		ReleaseEnergy.Modulate = Colors.Transparent;
		AudioManager.Instance.PlaySFX("BA_release_energy", 1, 0.9f);
		ReleaseEnergy.Play();
		Tween tween = GetTree().CreateTween();
		tween.TweenProperty(ReleaseEnergy, "modulate:a", 1f, 0.5f);
		await ToSignal(ReleaseEnergy, AnimatedSprite2D.SignalName.AnimationFinished);
		ReleaseEnergy.Visible = false;
	}

	/// <summary>
	/// Plays the Basil version of the Release Energy animation, and waits for it to finish.
	/// </summary>
	/// <returns>An awaitable <see cref="Task"/> that will complete whenever the animation finishes playing.</returns>
	public  async Task WaitForReleaseEnergyBasil()
	{
		ReleaseEnergyBasil.Visible = true;
		ReleaseEnergyBasil.Modulate = Colors.Transparent;
		AudioManager.Instance.PlaySFX("BA_release_energy", 1, 0.9f);
		ReleaseEnergyBasil.Play();
		Tween tween = GetTree().CreateTween();
		tween.TweenProperty(ReleaseEnergyBasil, "modulate:a", 1f, 0.5f);
		tween.TweenInterval(2.5f);
		tween.TweenProperty(ReleaseEnergyBasil, "modulate:a", 0f, 0.5f);
		await ToSignal(tween, Tween.SignalName.Finished);
		ReleaseEnergyBasil.Visible = false;
	}

	/// <summary>
	/// Plays the Red Hands skill animation, and waits for it to finish.
	/// </summary>
	/// <returns>An awaitable <see cref="Task"/> that will complete whenever the animation finishes playing.</returns>
	public async Task WaitForRedHands()
	{
		RedHands.Visible = true;
		AudioManager.Instance.PlaySFX("SE_red_hands", 0.8f, 0.9f);
		RedHands.Play();
		await ToSignal(RedHands, AnimatedSprite2D.SignalName.AnimationFinished);
		RedHands.Visible = false;
	}

	/// <summary>
	/// Plays the Flower Crown skill animation, and waits for it to finish.
	/// </summary>
	/// <returns>An awaitable <see cref="Task"/> that will complete whenever the animation finishes playing.</returns>
	public async Task WaitForFlowerCrown()
	{
		FlowerCrown.Visible = true;
		AudioManager.Instance.PlaySFX("SE_red_hands", 0.8f, 0.9f);
		FlowerCrown.Play();
		await ToSignal(FlowerCrown, AnimatedSprite2D.SignalName.AnimationFinished);
		FlowerCrown.Visible = false;
	}


	internal async Task WaitForOmoriSpecialAnimation(string overlay, string effect)
	{
		Sprite2D effectTex = new()
		{
			Texture = ResourceLoader.Load<Texture2D>(effect),
			Position = new Vector2(320f, 150f),
			Scale = new Vector2(2f, 2f),
			Modulate = Colors.Transparent
		};
		FullScreenEffectNode.AddChild(effectTex);

		Sprite2D overlayTex = new()
		{
			Texture = ResourceLoader.Load<Texture2D>(overlay),
			Position = Vector2.Zero,
			Centered = false,
			Modulate = Colors.Transparent
		};
		FullScreenEffectNode.AddChild(overlayTex);

		Tween overlayTween = GetTree().CreateTween();
		overlayTween.TweenProperty(overlayTex, "modulate:a", 0.60f, 1f);
		overlayTween.TweenInterval(0.66f);
		overlayTween.TweenProperty(overlayTex, "modulate:a", 0f, 0.66f);

		Tween effectTween = GetTree().CreateTween();
		effectTween.TweenProperty(effectTex, "modulate:a", 1f, 1f);
		effectTween.Parallel().TweenProperty(effectTex, "position:y", 180f, 1f);
		effectTween.Parallel().TweenProperty(effectTex, "scale", new Vector2(0.65f, 0.65f), 1f);
		effectTween.TweenInterval(0.66f);
		effectTween.TweenProperty(effectTex, "modulate:a", 0f, 0.66f);
		effectTween.Parallel().TweenProperty(effectTex, "position:y", 150f, 0.66f);
		effectTween.Parallel().TweenProperty(effectTex, "scale", new Vector2(2f, 2f), 0.66f);
		effectTween.TweenInterval(0.33f);
		
		await ToSignal(effectTween, Tween.SignalName.Finished);
		overlayTex.QueueFree();
		effectTex.QueueFree();
	}

	internal async Task WaitForBasilSpecialAnimation(string effect, int animationId)
	{
		Sprite2D effectTex = new()
		{
			Texture = ResourceLoader.Load<Texture2D>(effect),
			Position = new Vector2(320f, 150f),
			Scale = new Vector2(2f, 2f),
			Modulate = Colors.Transparent
		};
		FullScreenEffectNode.AddChild(effectTex);

		Tween effectTween = GetTree().CreateTween();
		effectTween.TweenProperty(effectTex, "modulate:a", 1f, 1f);
		effectTween.Parallel().TweenProperty(effectTex, "position:y", 240f, 1f);
		effectTween.Parallel().TweenProperty(effectTex, "scale", new Vector2(1f, 1f), 1f);
		effectTween.TweenCallback(Callable.From(() => PlayScreenAnimation(animationId, true)));
		effectTween.TweenInterval(1.5f);
		effectTween.TweenProperty(effectTex, "modulate:a", 0f, 0.66f);
		effectTween.Parallel().TweenProperty(effectTex, "position:y", 150f, 0.66f);
		effectTween.Parallel().TweenProperty(effectTex, "scale", new Vector2(2f, 2f), 0.66f);
		effectTween.TweenInterval(0.33f);
		
		await ToSignal(effectTween, Tween.SignalName.Finished);
		effectTex.QueueFree();
	}

	/// <summary>
	/// Plays the Photograph animation. Mainly used by Basil skills.
	/// </summary>
	public void PlayPhotograph()
	{
		Photograph.Visible = true;
		Tween tween = GetTree().CreateTween();
		tween.TweenProperty(Photograph, "modulate:a", 0f, 1f);
		tween.TweenCallback(Callable.From(() =>
		{
			Photograph.Modulate = Colors.White;
			Photograph.Visible = false;
		}));
	}

	/// <summary>
	/// Applies a screen tint above the enemies. Can be applied over a duration, in seconds.
	/// </summary>
	/// <remarks>
	/// If you would like to wait for the screen tint to finish, use <see cref="WaitForTintScreen"/>.
	/// </remarks>
	/// <param name="color">The Color to set the screen tint to. This includes alpha.</param>
	/// <param name="duration">The duration of the tint. If left as 0, the tint will be instant.</param>
	/// <param name="layer">The layer to show the screen tint on. Defaults to -4, just above the enemy layer.</param>
	public void TintScreen(Color color, float duration = 0f, int layer = -4)
	{
		ScreenTint.ZIndex = layer;
		if (duration == 0f)
			ScreenTint.Color = color;
		else
		{
			Tween tween = GetTree().CreateTween();
			tween.TweenProperty(ScreenTint, "color", color, duration);
		}
	}

	/// <summary>
	/// Applies a screen tint above the enemies and waits for it to finish.
	/// </summary>
	/// <param name="color">The Color to set the screen tint to. This includes alpha.</param>
	/// <param name="duration">The duration of the tint.</param>
	/// <param name="layer">The layer to show the screen tint on. Defaults to -4, just above the enemy layer.</param>
	/// <returns></returns>
	public async Task WaitForTintScreen(Color color, float duration, int layer = -4)
	{
		ScreenTint.ZIndex = layer;
		Tween tween = GetTree().CreateTween();
		tween.TweenProperty(ScreenTint, "color", color, duration);
		await ToSignal(tween, Tween.SignalName.Finished);
	}

	internal Sprite2D SpawnPerfectheartOverlay(Vector2 position)
	{
		Sprite2D sprite = PerfectheartOverlaySprite.Instantiate<Sprite2D>();
		PerfectheartOverlayParent.AddChild(sprite);
		sprite.Modulate = Colors.Transparent;
		sprite.Position = position;
		Tween tween = sprite.CreateTween();
		tween.TweenProperty(sprite, "modulate:a", 1f, 1f);
		return sprite;
	}

	internal async Task WaitForSnaley()
	{
		Snaley.Visible = true;
		Snaley.Position = new Vector2(400f, 0f);
		Snaley.Modulate = Colors.Transparent;
		AudioManager.Instance.PlaySFX("BA_release_energy", volume: 0.9f);
		Tween tween = GetTree().CreateTween();
		tween.TweenProperty(Snaley, "position:x", 55f, 1.5f);
		tween.Parallel().TweenProperty(Snaley, "modulate:a", 1f, 1.5f);
		tween.TweenProperty(Snaley, "position:x", -200f, 1.5f);
		tween.Parallel().TweenProperty(Snaley, "modulate:a", 0f, 1.5f);
		tween.TweenInterval(2f);
		await ToSignal(tween, Tween.SignalName.Finished);
		Snaley.Visible = false;
	}

	internal async Task WaitForHumphreySwarm()
	{
		HumphreySwarm.Visible = true;
		HumphreySwarm.Position = new Vector2(0, -65);
		Tween tween = GetTree().CreateTween();
		tween.TweenCallback(Callable.From(() => AudioManager.Instance.PlaySFX("ba_goop", 0.9f, 0.9f))).SetDelay(0.33f);
		tween.TweenProperty(HumphreySwarm, "position:y", -450f, 0.5f);
		tween.TweenCallback(Callable.From(() => TintScreen(Colors.Black)));
		tween.TweenInterval(0.5f);
		await ToSignal(tween, Tween.SignalName.Finished);
		HumphreySwarm.Visible = false;
	}

	internal async Task WaitForHumphreySwallow()
	{
		HumphreySwallow.Visible = true;
		AudioManager.Instance.PlaySFX("SE_humphrey_burp", volume: 0.9f);
		HumphreySwallow.Play();
		await ToSignal(HumphreySwallow, AnimatedSprite2D.SignalName.AnimationFinished);
		HumphreySwallow.Visible = false;
	}
	
	internal async Task WaitForHumphreyFaceSwallow()
	{
		HumphreyFaceSwallow.Visible = true;
		AudioManager.Instance.PlaySFX("SE_humphrey_burp", volume: 0.9f);
		HumphreyFaceSwallow.Play();
		await ToSignal(HumphreyFaceSwallow, AnimatedSprite2D.SignalName.AnimationFinished);
		HumphreyFaceSwallow.Visible = false;
	}

	internal async Task WaitForEncore()
	{
		AudioManager.Instance.PlaySFX("SE_bs_realization", 0.9f, 0.9f);
		Encore.Modulate = Colors.Transparent;
		Encore.Visible = true;
		Tween tween = GetTree().CreateTween();
		tween.TweenProperty(Encore, "modulate:a", 1f, 1.5f);
		tween.TweenInterval(2f);
		tween.TweenProperty(Encore, "modulate:a", 0f, 1f);
		await ToSignal(tween, Tween.SignalName.Finished);
		Encore.Visible = false;
	}

	internal async Task PlayCherish(int index)
	{
		Cherish.Modulate = Colors.Transparent;
		Cherish.Visible = true;
		Tween tween = GetTree().CreateTween();
		tween.TweenProperty(Cherish, "modulate:a", 1f, 0.75f);
		await Wait.Seconds(1);
		string remember = "You remembered " + index switch
		{
			4 => "MARI's request.",
			3 => "BASIL's hope.",
			2 => "HERO's promise.",
			1 => "AUBREY's wish.",
			_ => "KEL's words."
		};
		DialogueManager.Instance.QueueMessage(remember);
		await DialogueManager.Instance.WaitForDialogue();
		await Wait.Seconds(1);
		switch (index)
		{
			case 4:
				string name = BattleManager.Instance.GetPartyMember(0).Name;
				DialogueManager.Instance.QueueMessage("MARI", $@"You'll forgive yourself...\|[br]Won't you... {name.ToUpper()}?");
				break;
			case 3:
				DialogueManager.Instance.QueueMessage("BASIL", @"Maybe one day...\| things can go back to the way they were before.");
				break;
			case 2:
				DialogueManager.Instance.QueueMessage("HERO", "Last time...");
				DialogueManager.Instance.QueueMessage("HERO", "We made the mistake of leaving each other when we needed each other the most.");
				DialogueManager.Instance.QueueMessage("HERO", @"This time...\| we'll stay together.");
				break;
			case 1:
				DialogueManager.Instance.QueueMessage("AUBREY", @"I hope you can find some peace...\| or you know...\| some happiness.");
				break;
			default:
				DialogueManager.Instance.QueueMessage("KEL", @"Friends...\! Friends are supposed to be there for each other.");
				break;
		}
		await DialogueManager.Instance.WaitForDialogue();
		tween = GetTree().CreateTween();
		tween.TweenProperty(Cherish, "modulate:a", 0f, 0.75f);
		await ToSignal(tween, Tween.SignalName.Finished);
		Cherish.Visible = false;
	}

	internal void DespawnAll()
	{
		foreach (Node child in PerfectheartOverlayParent.GetChildren())
			child.QueueFree();
	}

	private PlayingAnimation StartAnimation(int id, Vector2 position, bool targetsEnemy)
	{
		if (!Animations.TryGetValue(id, out RPGMAnimatedSprite animation))
		{
			GD.PrintErr("Unknown animation: " + id);
			return null;
		}

		int index = 0;
		switch (animation.Layer)
		{
			case 0:
				index = 10;
				break;
			case 2:
				index = -1;
				break;
			case 3:
				index = targetsEnemy ? -4 : 0;
				break;
		}

		if (animation.TryGetFrameSFX(0, out List<SFX> sfx))
		{
			sfx.ForEach(AudioManager.Instance.PlaySFX);
		}

		if (animation.TryGetFrameShake(0, out Shake shake))
		{
			InitShake(shake);
		}

		PlayingAnimation playing = new(animation, position, index);
		AddChild(playing);
		PlayingAnimations.Add(playing);
		return playing;
	}

	internal PlayingAnimation PreviewAnimation(int id, int layer)
	{
		if (!Animations.TryGetValue(id, out RPGMAnimatedSprite animation))
			return null;

		if (animation.TryGetFrameSFX(0, out List<SFX> sfx))
		{
			sfx.ForEach(AudioManager.Instance.PlaySFX);
		}

		if (animation.TryGetFrameShake(0, out Shake shake))
		{
			InitShake(shake);
		}

		PlayingAnimation playing = new(animation, new Vector2(320, 240), layer);
		PlayingAnimations.Add(playing);
		return playing;
	}

	internal void StopAllAnimations()
	{
		foreach (PlayingAnimation animation in PlayingAnimations)
		{
			animation.EmitSignal(PlayingAnimation.SignalName.Finished);
			animation.QueueFree();
		}
		PlayingAnimations.Clear();
		FrameTimer = 0f;
		ResetShake();
		EmitSignal(SignalName.AnimationFinished);
	}

	internal IEnumerable<RPGMAnimatedSprite> GetAllAnimations()
	{
		return Animations.Values;
	}
}

#pragma warning disable CS0649
internal class AnimationInfo
{
	public int Id;
	public int Layer;
	public string Texture;
	public string AltTexture;
	public float[][][] Frames;
	public SFXInfo[] SFX;
	public ShakeInfo[] Shake;
}

internal class SFXInfo
{
	public int Frame;
	public string Name;
	public float Pitch;
	public float Volume;
}

internal class ShakeInfo
{
	public int Frame;
	public int Power;
	public int Speed;
	public int Duration;
}
#pragma warning restore CS0649
