using Godot;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using OmoriSandbox.Actors;
using OmoriSandbox.Extensions;
using OmoriSandbox.Menu;
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

	[Export] private Node2D BattlebackRoot;
	[Export] private Node2D EnemyAnimations;
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
	private ColorRect ScreenTone;
	private ShaderMaterial ScreenToneMaterial;
	private Tween ScreenToneTween;
	private int ScreenToneRequests;
	private Vector4 CurrentTone;
	private ShaderMaterial UIToneMaterial;
	private ShaderMaterial UIToneAddMaterial;
	private ShaderMaterial UIDimMaterial;
	private bool BelowUI;
	private bool UIToneDimmed;
	internal ShaderMaterial UIToneGrayMaterial { get; private set; }

	private Dictionary<int, RPGMAnimatedSprite> Animations = [];
	
	internal const float FPS = 15f;
	private float FrameDuration = 1f / FPS;
	private List<PlayingAnimation> PlayingAnimations = [];
	private readonly List<VideoStreamPlayer> PlayingVideos = [];
	private Tween ScreenTintTween;

	private float Shake = 0f;
	private float ShakePwr = 0f;
	private float ShakeSpd = 0f;
	private int ShakeDuration = 0;
	private float ShakeDirection = 1f;

	public static AnimationManager Instance { get; private set; }

	public override void _EnterTree()
	{
		Instance = this;
	}

	internal void Init()
	{
		CreateScreenTone();
		CreateUITone();
		string data = FileAccess.GetFileAsString("res://animations/animations.json");
		List<AnimationInfo> animationData = JsonConvert.DeserializeObject<List<AnimationInfo>>(data);
		foreach (AnimationInfo info in animationData)
		{
			bool missingTexture = string.IsNullOrWhiteSpace(info.Texture);
			bool missingAltTexture = string.IsNullOrWhiteSpace(info.AltTexture);

			if (missingTexture && missingAltTexture)
				continue;

			RPGMAnimatedSprite animation = new(info.Id, info.Layer, info.Position,
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

			foreach (int[] flash in info.Flash)
			{
				animation.SetFrameFlash(flash[0], flash[1], flash[2..6], flash[6]);
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
		layerRegex.Compile(@"(?i)<Layer:\s*(\d+)>");
		int total = 0;
		int attempted = 0;
		foreach (RPGMakerAnimation animation in animationData)
		{
			if (animation == null)
			{
				report.Warn("animations", "Animations.json", "Skipping a null entry");
				continue;
			}
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

				int layer = 3;
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

				RPGMAnimatedSprite anim = new(animation.Id, layer, animation.Position, texture, altTexture);
				foreach (float[][] frame in animation.Frames)
				{
					List<Frame> frames = [];
					frames.AddRange(frame.Select(f => new Frame((int)f[0], f[1], f[2], f[3], f[4], f[5] == 1, f[6],
					f.Length > 7 ? (int)f[7] : 0)));
					anim.CreateFrame(frames);
				}

				foreach (Timings timing in animation.Timings)
				{
					if (timing.Se?.Name == "ft_doShake")
					{
						anim.SetFrameShake(timing.Frame, timing.FlashColor[0], timing.FlashColor[1], timing.FlashDuration);
						continue;
					}
					if (timing.FlashScope is 1 or 2 or 3)
						anim.SetFrameFlash(timing.Frame, timing.FlashScope, timing.FlashColor, timing.FlashDuration);
					if (timing.Se == null)
						continue;
					if (!LoadModSFX(root, dir, timing.Se.Name))
						report.Warn("animations", animation.Name,
							$"Unable to find SFX {timing.Se.Name}.ogg for modded animation {animation.Id}");
					anim.SetFrameSFX(timing.Frame, new SFX(timing.Se.Name, timing.Se.Pitch, timing.Se.Volume));
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
		layerRegex.Compile(@"(?i)<Layer:\s*(\d+)>");

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
				int position = 1;
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
						case "position":
							position = op["value"].ToObject<int>();
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

				int layer = 3;
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

				RPGMAnimatedSprite anim = new(id, layer, position, texture, altTexture);

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
					if (timing.Se?.Name == "ft_doShake")
					{
						anim.SetFrameShake(timing.Frame, timing.FlashColor[0], timing.FlashColor[1], timing.FlashDuration);
						continue;
					}
					if (timing.FlashScope is 1 or 2 or 3)
						anim.SetFrameFlash(timing.Frame, timing.FlashScope, timing.FlashColor, timing.FlashDuration);
					if (timing.Se == null)
						continue;
					if (!LoadModSFX(root, dir, timing.Se.Name))
						report.Warn("animations", id.ToString(),
							$"Unable to find SFX {timing.Se.Name}.ogg for modded animation {id}");
					anim.SetFrameSFX(timing.Frame, new SFX(timing.Se.Name, timing.Se.Pitch, timing.Se.Volume));
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
		if (AudioManager.Instance.HasSFX(name))
			return true;
		string path = $"user://mods/{root}/{dir}/{name}.ogg";
		if (FileAccess.FileExists(path))
			return AudioManager.Instance.LoadCustomSFX(path);
		string sfxDir = DirAccess.GetDirectoriesAt($"user://mods/{root}").FirstOrDefault(x => x.ToLower() == "sfx");
		if (sfxDir != null && FileAccess.FileExists($"user://mods/{root}/{sfxDir}/{name}.ogg"))
			return true;
		return ResourceLoader.Exists($"res://audio/sfx/{name}.ogg");
	}

	public override void _Process(double delta)
	{
		for (int i = PlayingAnimations.Count - 1; i >= 0; i--)
		{
			PlayingAnimations[i].FrameTimer += (float)delta;
			if (PlayingAnimations[i].FrameTimer < FrameDuration)
				continue;
			PlayingAnimations[i].FrameTimer -= FrameDuration;
			NextFrame(i);
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		// physics process runs at 60 fps, just like screen shake
		if (ShakeDuration > 0 || Shake != 0f)
		{
			UpdateShake();
		}
		
		BattlebackRoot.Position = new Vector2((float)Math.Round(Shake), 0);

		foreach (PlayingAnimation playing in PlayingAnimations)
		{
			playing.UpdateFlashes();
		}
	}

	private void NextFrame(int i)
	{
		// returns true if we're out of frames
		if (PlayingAnimations[i].AdvanceFrame())
		{
			PlayingAnimations[i].RestoreTarget();
			PlayingAnimations[i].EmitSignal(PlayingAnimation.SignalName.Finished);
			PlayingAnimations[i].QueueFree();
			PlayingAnimations.RemoveAt(i);
			if (PlayingAnimations.Count == 0)
				EmitSignal(SignalName.AnimationFinished);
			return;
		}

		if (PlayingAnimations[i].Animation.TryGetFrameSFX(PlayingAnimations[i].CurrentFrame, out List<SFX> sfx))
		{
			sfx.ForEach(AudioManager.Instance.PlaySFX);
		}

		if (PlayingAnimations[i].Animation.TryGetFrameShake(PlayingAnimations[i].CurrentFrame, out Shake shake))
		{
			InitShake(shake);
		}

		if (PlayingAnimations[i].Animation.TryGetFrameFlash(PlayingAnimations[i].CurrentFrame, out List<Flash> flashes))
		{
			flashes.ForEach(PlayingAnimations[i].StartFlash);
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
	/// Calling this method while a shake is already happening replaces its power, speed and duration,
	/// however the current offset carries over.
	/// </summary>
	public void InitShake(Shake shake)
	{
		ShakePwr = shake.Power;
		ShakeSpd = shake.Speed;
		ShakeDuration = shake.Duration;
	}

	private void ResetShake()
	{
		BattlebackRoot.Position = Vector2.Zero;
		Shake = 0f;
		ShakePwr = 0f;
		ShakeSpd = 0f;
		ShakeDuration = 0;
		ShakeDirection = 1f;
	}

	/// <summary>
	/// Plays an animation with the given <paramref name="id"/> centered on the given <paramref name="target"/>.<br/>
	/// Use <see cref="WaitForAnimation(int, Actor, int?)"/> if you want to wait for the animation to finish.
	/// </summary>
	/// <param name="id">The animation ID to play. Uses the same ID numbers as OMORI for all vanilla animations.</param>
	/// <param name="target">The <see cref="Actor"/> that this animation will play centered on.</param>
	/// <param name="layer">The z-index to draw the animation at, overriding the layer from its animation data. Null keeps the animation's own layer.</param>
	public void PlayAnimation(int id, Actor target, int? layer = null)
	{
		StartAnimation(id, target.CenterPoint, target is Enemy, target, layer: layer);
	}

	/// <summary>
	/// Plays an animation with the given <paramref name="id"/> centered on the given <paramref name="position"/>.<br/>
	/// Use <see cref="WaitForAnimation(int, Vector2, bool, int?)"/> if you want to wait for the animation to finish.
	/// </summary>
	/// <param name="id">The animation ID to play. Uses the same ID numbers as OMORI for all vanilla animations.</param>
	/// <param name="position">The coordinates to play the animation at.</param>
	/// <param name="targetsEnemy">Whether the animation should play on the enemy layer.</param>
	/// <param name="layer">The z-index to draw the animation at, overriding the layer from its animation data. Null keeps the animation's own layer.</param>
	public void PlayAnimation(int id, Vector2 position, bool targetsEnemy, int? layer = null)
	{
		StartAnimation(id, position, targetsEnemy, layer: layer);
	}

	/// <summary>
	/// Plays an animation with the given <paramref name="id"/> centered on the screen.<br/>
	/// Use <see cref="WaitForScreenAnimation(int, bool, int?)"/> if you want to wait for the animation to finish.
	/// </summary>
	/// <param name="id">The animation ID to play. Uses the same ID numbers as OMORI for all vanilla animations.</param>
	/// <param name="targetsEnemy">Whether this animation targets an enemy.<br/>
	/// Mainly used for animation layering, such as skill animations that target enemies and need to display underneath the UI.</param>
	/// <param name="layer">The z-index to draw the animation at, overriding the layer from its animation data. Null keeps the animation's own layer.</param>
	public void PlayScreenAnimation(int id, bool targetsEnemy, int? layer = null)
	{
		StartAnimation(id, new Vector2(320, 240), targetsEnemy, layer: layer);
	}

	/// <summary>
	/// Plays an animation with the given <paramref name="id"/> centered on the given <paramref name="target"/>, and waits for it to finish.<br/>
	/// Use <see cref="PlayAnimation(int, Actor, int?)"/> if you want the animation to play without waiting.
	/// </summary>
	/// <param name="id">The animation ID to play. Uses the same ID numbers as OMORI for all vanilla animations.</param>
	/// <param name="target">The <see cref="Actor"/> that this animation will play centered on.</param>
	/// <param name="layer">The z-index to draw the animation at, overriding the layer from its animation data. Null keeps the animation's own layer.</param>
	/// <returns>An awaitable <see cref="Task"/> that will complete whenever the animation finishes playing.</returns>
	public async Task WaitForAnimation(int id, Actor target, int? layer = null)
	{
		PlayingAnimation playing = StartAnimation(id, target.CenterPoint, target is Enemy, target, layer: layer);
		if (playing == null) return;
		await ToSignal(playing, PlayingAnimation.SignalName.Finished);
	}

	/// <summary>
	/// Plays an animation with the given <paramref name="id"/> centered on the given <paramref name="position"/> and waits for it to finish.<br/>
	/// Use <see cref="PlayAnimation(int, Vector2, bool, int?)"/> if you want the animation to play without waiting.
	/// </summary>
	/// <param name="id">The animation ID to play. Uses the same ID numbers as OMORI for all vanilla animations.</param>
	/// <param name="position">The coordinates to play the animation at.</param>
	/// <param name="targetsEnemy">Whether the animation should play on the enemy layer.</param>
	/// <param name="layer">The z-index to draw the animation at, overriding the layer from its animation data. Null keeps the animation's own layer.</param>
	public async Task WaitForAnimation(int id, Vector2 position, bool targetsEnemy, int? layer = null)
	{
		PlayingAnimation playing = StartAnimation(id, position, targetsEnemy, layer: layer);
		if (playing == null) return;
		await ToSignal(playing, PlayingAnimation.SignalName.Finished);
	}

	/// <summary>
	/// Plays an animation with the given <paramref name="id"/> centered on the screen, and waits for it to finish.<br/>
	/// Use <see cref="PlayScreenAnimation(int, bool, int?)"/> if you want the animation to play without waiting.
	/// </summary>
	/// <param name="id">The animation ID to play. Uses the same ID numbers as OMORI for all vanilla animations.</param>
	/// <param name="targetsEnemy">Whether this animation targets an enemy.<br/>
	/// Mainly used for animation layering, such as skill animations that target enemies and need to display underneath the UI.</param>
	/// <param name="layer">The z-index to draw the animation at, overriding the layer from its animation data. Null keeps the animation's own layer.</param>
	/// <returns>An awaitable <see cref="Task"/> that will complete whenever the animation finishes playing.</returns>
	public async Task WaitForScreenAnimation(int id, bool targetsEnemy, int? layer = null)
	{
		PlayingAnimation playing = StartAnimation(id, new Vector2(320, 240), targetsEnemy, layer: layer);
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
		sprite.SpriteFrames.SetAnimationLoopMode(animationName, SpriteFrames.LoopMode.None);
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
		sprite.SpriteFrames.SetAnimationLoopMode(animationName, SpriteFrames.LoopMode.None);
		AddChild(sprite);
		sprite.Play();
		await ToSignal(sprite, AnimatedSprite2D.SignalName.AnimationFinished);
		RemoveChild(sprite);
		sprite.QueueFree();
	}

	/// <summary>
	/// Plays an .ogv video.
	/// </summary>
	/// <param name="path">The path to the .ogv file, relative to user://mods.</param>
	/// <param name="layer">The layer to show the video on.</param>
	/// <param name="volume">The volume to play the video's audio at, from 0 to 2.</param>
	public void PlayVideo(string path, int layer = 0, float volume = 1f)
	{
		VideoStreamPlayer player = SpawnVideo(path, layer, volume);
		if (player == null)
			return;
		void Finished()
		{
			player.Finished -= Finished;
			PlayingVideos.Remove(player);
			player.QueueFree();
		}
		player.Finished += Finished;
	}

	/// <summary>
	/// Plays an .ogv video and waits for it to finish.
	/// </summary>
	/// <param name="path">The path to the .ogv file, relative to user://mods.</param>
	/// <param name="layer">The layer to show the video on.</param>
	/// <param name="volume">The volume to play the video's audio at, from 0 to 2.</param>
	public async Task WaitForVideo(string path, int layer = 0, float volume = 1f)
	{
		VideoStreamPlayer player = SpawnVideo(path, layer, volume);
		if (player == null)
			return;
		await ToSignal(player, VideoStreamPlayer.SignalName.Finished);
		if (IsInstanceValid(player))
		{
			PlayingVideos.Remove(player);
			player.QueueFree();
		}
	}

	/// <summary>
	/// Stops and removes all currently playing videos.
	/// </summary>
	public void StopAllVideos()
	{
		foreach (VideoStreamPlayer player in PlayingVideos.ToList())
		{
			player.Stop();
			// stop doesn't emit finished so call it ourselves
			player.EmitSignal(VideoStreamPlayer.SignalName.Finished);
			if (IsInstanceValid(player))
				player.QueueFree();
		}
		PlayingVideos.Clear();
	}

	private VideoStreamPlayer SpawnVideo(string path, int layer, float volume)
	{
		if (string.IsNullOrWhiteSpace(path) || path.Contains("..") || path.Contains("://") || System.IO.Path.IsPathRooted(path))
		{
			GD.PushError($"Invalid video path '{path}' (path traversal not allowed)");
			return null;
		}
		if (!FileAccess.FileExists("user://mods/" + path))
		{
			GD.PrintErr("Unknown video: " + path);
			return null;
		}
		if (!path.EndsWith(".ogv", StringComparison.OrdinalIgnoreCase))
			GD.PushWarning("Videos should be .ogv files, " + path + " may not play correctly!");

		VideoStreamPlayer player = new()
		{
			Stream = new VideoStreamTheora { File = "user://mods/" + path },
			Size = new Vector2(640, 480),
			Expand = true,
			Volume = Math.Clamp(volume, 0f, 2f),
			Bus = "SFX",
			ZIndex = layer
		};
		AddChild(player);
		PlayingVideos.Add(player);
		player.Play();
		if (!player.IsPlaying())
		{
			GD.PrintErr("Failed to play video: " + path);
			PlayingVideos.Remove(player);
			player.QueueFree();
			return null;
		}
		return player;
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


	/// <summary>
	/// Plays the full-screen effect of OMORI's special skills and waits for it to finish. The overlay fades in and out
	/// while the effect image zooms in over the battle.
	/// </summary>
	/// <param name="overlay">The <c>res://</c> path of the full-screen overlay texture.</param>
	/// <param name="effect">The <c>res://</c> path of the effect texture shown in the middle of the screen.</param>
	/// <remarks>
	/// If <paramref name="overlay"/> or <paramref name="effect"/> is null, an error is logged and nothing plays.
	/// </remarks>
	public async Task WaitForOmoriSpecialAnimation(string overlay, string effect)
	{
		if (overlay == null || effect == null)
		{
			GD.PushError("WaitForOmoriSpecialAnimation needs an overlay and an effect path.");
			return;
		}

		Sprite2D effectTex = new()
		{
			Texture = ResourceLoader.Load<Texture2D>(effect),
			Position = new Vector2(320f, 150f),
			Scale = new Vector2(2f, 2f),
			Modulate = Colors.Transparent,
			ZIndex = 20
		};
		FullScreenEffectNode.AddChild(effectTex);

		Sprite2D overlayTex = new()
		{
			Texture = ResourceLoader.Load<Texture2D>(overlay),
			Position = Vector2.Zero,
			Centered = false,
			Modulate = Colors.Transparent,
			ZIndex = 20
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
			Modulate = Colors.Transparent,
			ZIndex = 20
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
	/// Covers the battle with a color overlay above the enemies. Can be applied over a duration, in seconds.
	/// </summary>
	/// <remarks>
	/// If you would like to wait for the overlay to finish, use <see cref="WaitForTintOverlay"/>.
	/// To change the battle's colors like RPG Maker's Tint Screen instead of covering them, use <see cref="TintScreen"/>.
	/// </remarks>
	/// <param name="color">The Color to set the screen tint to. This includes alpha.</param>
	/// <param name="duration">The duration of the tint. If left as 0, the tint will be instant.</param>
	/// <param name="layer">The layer to show the screen tint on. Defaults to -4, just above the enemy layer.</param>
	public void TintOverlay(Color color, float duration = 0f, int layer = -4)
	{
		ScreenTint.ZIndex = layer;
		if (duration == 0f)
		{
			ScreenTintTween?.Kill();
			ScreenTint.Color = color;
		}
		else
			StartScreenTint(color, duration);
	}

	/// <summary>
	/// Covers the battle with a color overlay above the enemies and waits for it to finish.
	/// </summary>
	/// <param name="color">The Color to set the screen tint to. This includes alpha.</param>
	/// <param name="duration">The duration of the tint.</param>
	/// <param name="layer">The layer to show the screen tint on. Defaults to -4, just above the enemy layer.</param>
	/// <returns></returns>
	public async Task WaitForTintOverlay(Color color, float duration, int layer = -4)
	{
		ScreenTint.ZIndex = layer;
		Tween tween = StartScreenTint(color, duration);
		while (tween.IsValid() && tween.IsRunning())
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
	}

	private Tween StartScreenTint(Color color, float duration)
	{
		ScreenTintTween?.Kill();
		ScreenTintTween = GetTree().CreateTween();
		ScreenTintTween.TweenProperty(ScreenTint, "color", color, duration);
		return ScreenTintTween;
	}

	/// <summary>
	/// Whether <see cref="TintScreen"/> only changes the battlefield.
	/// </summary>
	/// <remarks>
	/// While this is false, the default, a tint also changes the menus, the battle log, the energy bar, the portraits and the party's animations and damage numbers.
	/// While it's true, they keep their colors and only the battleback, the enemies and their animations change. It turns off again when a battle starts or resets.
	/// </remarks>
	public bool ToneBelowUI
	{
		get => BelowUI;
		set
		{
			BelowUI = value;
			RefreshUITone();
		}
	}

	/// <summary>
	/// Changes the colors of the battle, including the party's menus, portraits and battle log. Can be applied over a duration, in seconds.
	/// </summary>
	/// <remarks>
	/// Works like RPG Maker's Tint Screen.
	/// To change only the battlefield, set <see cref="ToneBelowUI"/>. <c>TintScreen(0, 0, 0, 0)</c> restores the normal colors.
	/// If you would like to wait for the change to finish, use <see cref="WaitForTintScreen"/>.
	/// To cover the battle with a color instead, use <see cref="TintOverlay"/>.
	/// </remarks>
	/// <param name="red">How much red to add, from -255 to 255.</param>
	/// <param name="green">How much green to add, from -255 to 255.</param>
	/// <param name="blue">How much blue to add, from -255 to 255.</param>
	/// <param name="gray">How much color to remove, from 0 (none) to 255 (fully gray).</param>
	/// <param name="duration">The duration of the change. If left as 0, the change will be instant.</param>
	public void TintScreen(int red, int green, int blue, int gray, float duration = 0f)
	{
		int request = ++ScreenToneRequests;
		StopScreenToneTween();
		if (request != ScreenToneRequests)
			return;
		Vector4 target = new(Math.Clamp(red, -255, 255) / 255f, Math.Clamp(green, -255, 255) / 255f,
			Math.Clamp(blue, -255, 255) / 255f, Math.Clamp(gray, 0, 255) / 255f);
		if (duration <= 0f)
		{
			SetScreenTone(target);
			return;
		}
		ScreenToneTween = CreateTween();
		ScreenToneTween.TweenMethod(Callable.From<Vector4>(SetScreenTone), CurrentTone, target, duration);
	}

	/// <summary>
	/// Changes the colors of the battle, including the party's menus, portraits and battle log, and waits for the change to finish.
	/// </summary>
	/// <remarks>
	/// See <see cref="TintScreen"/>. If another tint starts before this one finishes, the wait ends early.
	/// </remarks>
	/// <param name="red">How much red to add, from -255 to 255.</param>
	/// <param name="green">How much green to add, from -255 to 255.</param>
	/// <param name="blue">How much blue to add, from -255 to 255.</param>
	/// <param name="gray">How much color to remove, from 0 (none) to 255 (fully gray).</param>
	/// <param name="duration">The duration of the change.</param>
	public async Task WaitForTintScreen(int red, int green, int blue, int gray, float duration)
	{
		TintScreen(red, green, blue, gray, duration);
		Tween tween = ScreenToneTween;
		if (tween != null)
			await ToSignal(tween, Tween.SignalName.Finished);
	}

	private void StopScreenToneTween()
	{
		Tween tween = ScreenToneTween;
		if (tween == null)
			return;
		ScreenToneTween = null;
		tween.Kill();
		tween.EmitSignal(Tween.SignalName.Finished);
	}

	private void SetScreenTone(Vector4 tone)
	{
		bool wasToned = CurrentTone != Vector4.Zero;
		CurrentTone = tone;
		ScreenToneMaterial.SetShaderParameter("tone", tone);
		UIToneMaterial.SetShaderParameter("tone", tone);
		UIToneAddMaterial.SetShaderParameter("tone", tone);
		UIToneGrayMaterial.SetShaderParameter("tone", tone);
		ScreenTone.Visible = tone != Vector4.Zero;
		if (ScreenTone.Visible != wasToned)
			RefreshUITone();
	}

	private void CreateScreenTone()
	{
		ScreenToneMaterial = new ShaderMaterial { Shader = GD.Load<Shader>("res://assets/screen_tone_shader.gdshader") };
		ScreenTone = new ColorRect
		{
			Name = "ScreenTone",
			Material = ScreenToneMaterial,
			ZIndex = -4,
			Visible = false,
			MouseFilter = Control.MouseFilterEnum.Ignore,
			Position = new Vector2(-1280f, -64f),
			Size = new Vector2(3200f, 608f)
		};
		FullScreenEffectNode.AddChild(ScreenTone);
		FullScreenEffectNode.MoveChild(ScreenTone, 0);
	}

	private void CreateUITone()
	{
		UIToneMaterial = new ShaderMaterial { Shader = GD.Load<Shader>("res://assets/ui_tone_shader.gdshader") };
		UIToneAddMaterial = new ShaderMaterial { Shader = GD.Load<Shader>("res://assets/ui_tone_add_shader.gdshader") };
		UIToneGrayMaterial = new ShaderMaterial { Shader = GD.Load<Shader>("res://assets/ui_tone_gray_shader.gdshader") };
		UIDimMaterial = new ShaderMaterial { Shader = GD.Load<Shader>("res://assets/ui_tone_shader.gdshader") };
		foreach ((CanvasItem root, bool _) in GetUIToneRoots())
			InheritUITone(root);
	}

	internal static void InheritUITone(Node root)
	{
		foreach (Node child in root.GetChildren())
		{
			if (child is CanvasItem item && item.Material == null && child is not CursorBounce)
				item.UseParentMaterial = true;
			InheritUITone(child);
		}
	}

	private bool IsUIToned(bool dims)
	{
		return CurrentTone != Vector4.Zero && !BelowUI && !(dims && UIToneDimmed);
	}

	private bool IsUIDimmed(bool dims)
	{
		return dims && UIToneDimmed && !BelowUI;
	}

	internal void ApplyUITone(CanvasItem root, bool dims = false)
	{
		root.UseParentMaterial = false;
		root.Material = IsUIDimmed(dims) ? UIDimMaterial : IsUIToned(dims) ? UIToneMaterial : null;
		RefreshGrayCursors(root);
	}

	private void ApplyAnimationTone(PlayingAnimation playing)
	{
		bool party = playing.ZIndex > -4;
		bool toned = IsUIToned(true) && party;
		playing.SetTone(IsUIDimmed(true) && party ? UIDimMaterial : toned ? UIToneMaterial : null, toned ? UIToneAddMaterial : null);
	}

	internal void SetUIDim(float rate)
	{
		float t = -100f / 255f * rate;
		UIDimMaterial.SetShaderParameter("tone", new Vector4(t, t, t, 0f));
		bool dimmed = rate > 0f;
		if (UIToneDimmed == dimmed)
			return;
		UIToneDimmed = dimmed;
		RefreshUITone();
	}

	private void RefreshUITone()
	{
		foreach ((CanvasItem root, bool dims) in GetUIToneRoots())
			ApplyUITone(root, dims);
		foreach (PlayingAnimation playing in PlayingAnimations)
		{
			if (playing.GetParent() == this)
				ApplyAnimationTone(playing);
		}
		if (BattleManager.Instance == null)
			return;
		foreach (Node child in BattleManager.Instance.GetChildren())
		{
			if (child is DamageNumber { Party: true } number)
				ApplyUITone(number, dims: true);
		}
	}

	private static IEnumerable<(CanvasItem Root, bool Dims)> GetUIToneRoots()
	{
		if (MenuManager.Instance != null)
		{
			foreach (CanvasItem menu in MenuManager.Instance.AllMenus)
				yield return (menu, false);
			yield return (MenuManager.Instance.EnergyDisplay, true);
		}
		if (BattleLogManager.Instance != null)
		{
			yield return (BattleLogManager.Instance, false);
			foreach (CanvasItem text in BattleLogManager.Instance.TextNodes)
				yield return (text, true);
		}
		if (BattleManager.Instance == null)
			yield break;
		foreach (PartyMember member in BattleManager.Instance.GetAllPartyMembers())
		{
			if (member.Component?.GetParent() is CanvasItem card)
				yield return (card, true);
		}
	}

	private static void RefreshGrayCursors(Node node)
	{
		foreach (Node child in node.GetChildren())
		{
			if (child is CursorBounce cursor)
				cursor.RefreshGray();
			RefreshGrayCursors(child);
		}
	}

	internal void ResetScreenEffects()
	{
		StopScreenToneTween();
		BelowUI = false;
		UIToneDimmed = false;
		SetScreenTone(Vector4.Zero);
		RefreshUITone();
		ScreenTintTween?.Kill();
		ScreenTintTween = null;
		ScreenTint.ZIndex = -4;
		ScreenTint.Color = ColorsExtension.TransparentBlack;
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
		tween.TweenCallback(Callable.From(() => TintScreen(-255, -255, -255, 0)));
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

	private PlayingAnimation StartAnimation(int id, Vector2 position, bool targetsEnemy, Actor target = null, int? layer = null)
	{
		if (!Animations.TryGetValue(id, out RPGMAnimatedSprite animation))
		{
			GD.PrintErr("Unknown animation: " + id);
			return null;
		}

		int index = -4;
		if (!targetsEnemy)
		{
			switch (animation.Layer)
			{
				case 0:
					index = 4;
					break;
				case 1:
					index = 6;
					break;
				case 2:
					index = 7;
					break;
				default:
					index = 11;
					break;
			}
		}

		if (animation.TryGetFrameSFX(0, out List<SFX> sfx))
		{
			sfx.ForEach(AudioManager.Instance.PlaySFX);
		}

		if (animation.TryGetFrameShake(0, out Shake shake))
		{
			InitShake(shake);
		}

		if (animation.Position == 3)
			position = new Vector2(320, 240);

		PlayingAnimation playing = new(animation, position, layer ?? index, target);
		if (targetsEnemy)
			EnemyAnimations.AddChild(playing);
		else
		{
			AddChild(playing);
			ApplyAnimationTone(playing);
		}
		PlayingAnimations.Add(playing);
		if (animation.TryGetFrameFlash(0, out List<Flash> flashes))
		{
			flashes.ForEach(playing.StartFlash);
		}
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
		if (animation.TryGetFrameFlash(0, out List<Flash> flashes))
		{
			flashes.ForEach(playing.StartFlash);
		}
		return playing;
	}

	internal void StopAllAnimations()
	{
		foreach (PlayingAnimation animation in PlayingAnimations)
		{
			animation.RestoreTarget();
			animation.EmitSignal(PlayingAnimation.SignalName.Finished);
			animation.QueueFree();
		}
		PlayingAnimations.Clear();
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
	public int Position;
	public string Texture;
	public string AltTexture;
	public float[][][] Frames;
	public SFXInfo[] SFX;
	public ShakeInfo[] Shake;
	public int[][] Flash;
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
