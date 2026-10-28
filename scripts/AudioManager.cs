using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Animation;
using OmoriSandbox.Editor;

namespace OmoriSandbox;

/// <summary>
/// Handles all audio playback, including SFX and BGM.
/// </summary>
public partial class AudioManager : Node
{
	[Export] internal AudioStreamPlayer BGM { get; private set; }
	[Export] internal AudioStreamPlayer BGS { get; private set; }

	private readonly List<AudioStreamPlayer> AudioPlayers = [];

	private readonly Dictionary<string, AudioStreamOggVorbis> SFXDictionary = [];
	private readonly SortedDictionary<string, AudioStreamOggVorbis> BGMDictionary = [];
	private readonly Dictionary<string, double> BGMLoopOffsets = [];
	private readonly SortedDictionary<string, AudioStreamOggVorbis> BGSDictionary = [];
	private readonly Dictionary<string, double> BGSLoopOffsets = [];
	/// <summary>
	/// The name of the BGM that was started last, or an empty string if none has played yet.
	/// </summary>
	public string CurrentBGM { get; private set; } = "";

	/// <summary>
	/// The name of the BGS that is playing, or an empty string if none is.
	/// </summary>
	public string CurrentBGS { get; private set; } = "";

	/// <summary>
	/// The current pitch of the BGM, including a pitch fade in progress.
	/// </summary>
	public float CurrentBGMPitch => BGM.PitchScale;

	public static AudioManager Instance { get; private set; }

	// only one instance of a sound can play at once
	private Dictionary<string, AudioStreamPlayer> PlayingSounds = [];
	
	// reusable tween so multiple pitch tweens don't fight over the BGM
	private Tween PitchTween;
	private Tween VolumeTween;
	private Tween BGSFadeTween;

	public override void _EnterTree()
	{
		Instance = this;
	}

	internal void Init()
	{
		if (SFXDictionary.Count > 0)
		{
			GD.PrintErr("Attempting to re-init AudioManager, ignoring!");
			return;
		}

		foreach (Node node in GetChildren())
		{
			AudioStreamPlayer player = node as AudioStreamPlayer;
			AudioPlayers.Add(player);
			player.Finished += () => OnSFXFinish(player);
		}
		// preload animation sfx
		int failedPreloads = 0;
		foreach (RPGMAnimatedSprite animation in AnimationManager.Instance.GetAllAnimations())
		{
			foreach (List<SFX> sfxList in animation.AllSFX)
			{
				foreach (SFX sfx in sfxList)
				{
					// we're technically checking if the file exists twice here,
					// but doing so avoids console spam
					if (ResourceLoader.Exists("res://audio/sfx/" + sfx.Name + ".ogg"))
					{
						SFXDictionary.TryAdd(sfx.Name, ResourceLoader.Load<AudioStreamOggVorbis>("res://audio/sfx/" + sfx.Name + ".ogg"));
					}
					else
					{
						failedPreloads++;
					}
				}
			}
		}
		GD.Print($"Preloaded {SFXDictionary.Count} animation SFX. ({failedPreloads} failures)");
		
		// preload bgm
		foreach (string bgm in ResourceLoader.ListDirectory("res://audio/bgm"))
		{
			AudioStreamOggVorbis stream = ResourceLoader.Load<AudioStreamOggVorbis>("res://audio/bgm/" + bgm);
			stream.Loop = true;
			stream.LoopOffset = 0;
			RegisterBGMLoopOffset(bgm.GetBaseName(), stream);
			BGMDictionary.Add(bgm.GetBaseName(), stream);
		}

		GD.Print($"Preloaded {BGMDictionary.Count} vanilla BGM.");

		foreach (string bgs in ResourceLoader.ListDirectory("res://audio/bgs"))
		{
			AudioStreamOggVorbis stream = ResourceLoader.Load<AudioStreamOggVorbis>("res://audio/bgs/" + bgs);
			stream.Loop = true;
			stream.LoopOffset = 0;
			RegisterLoopOffset(BGSLoopOffsets, bgs.GetBaseName(), stream);
			BGSDictionary.Add(bgs.GetBaseName(), stream);
		}

		GD.Print($"Preloaded {BGSDictionary.Count} vanilla BGS.");
	}

	internal void PlaySFX(SFX sfx)
	{
		PlaySFX(sfx.Name, sfx.Pitch / 100f, sfx.Volume / 100f);
	}

	/// <summary>
	/// Plays SFX with the given <paramref name="name"/>. 
	/// </summary>
	/// <remarks>
	/// If an SFX of the same <paramref name="name"/> is already playing, it will be restarted.
	/// </remarks>
	/// <param name="name">The name of the SFX to play.</param>
	/// <param name="pitch">The pitch to play the SFX at.</param>
	/// <param name="volume">The volume to play the SFX at.</param>
	public void PlaySFX(string name, float pitch = 1f, float volume = 1f)
	{
		if (!SFXDictionary.TryGetValue(name, out AudioStreamOggVorbis stream)) {
			stream = ResourceLoader.Load<AudioStreamOggVorbis>("res://audio/sfx/" + name + ".ogg");
			if (stream == null)
			{
				GD.PrintErr("Unknown SFX: " + name);
				return;
			}
			SFXDictionary.Add(name, stream);
		}
		
		if (PlayingSounds.TryGetValue(name, out AudioStreamPlayer existing))
		{
			existing.Stream = stream;
			existing.PitchScale = pitch;
			existing.VolumeLinear = volume;
			existing.Play();
			return;
		}

		foreach (AudioStreamPlayer player in AudioPlayers)
		{
			if (player.Playing)
				continue;
			player.Stream = stream;
			player.PitchScale = pitch;
			player.VolumeLinear = volume;
			player.Play();
			PlayingSounds.Add(name, player);
			return;
		}

		GD.Print("Overloaded! We ran out of AudioStreams!");
	}

	/// <summary>
	/// Plays BGM with the given <paramref name="name"/>.
	/// </summary>
	/// <remarks>
	/// At the enemy level, use the <see cref="OmoriSandbox.Actors.Enemy.ChangeBGM"/> method to respect the <see cref="OmoriSandbox.Actors.Enemy.CanChangeBGM"/> setting.
	/// </remarks>
	/// <param name="name">The name of the BGM to play.</param>
	public void PlayBGM(string name)
	{
		PlayBGM(name, 1f, 1f);
	}

	/// <summary>
	/// Plays BGM with the given <paramref name="name"/> and desired parameters.
	/// </summary>
	/// <remarks>
	/// At the enemy level, use the <see cref="OmoriSandbox.Actors.Enemy.ChangeBGM"/> method to respect the <see cref="OmoriSandbox.Actors.Enemy.CanChangeBGM"/> setting.
	/// </remarks>
	/// <param name="name">The name of the BGM to play.</param>
	/// <param name="volume">The volume to play the BGM at, from 0.001 to 3.0.</param>
	/// <param name="pitch">The pitch to play the BGM at, from 0.1 to 2.0.</param>
	public void PlayBGM(string name, float volume, float pitch)
	{
		if (BGMLocked)
		{
			if (SettingsMenuManager.Instance.LogDebug)
				GD.Print($"Ignored PlayBGM(\"{name}\"), the preset locks the BGM.");
			BGM.VolumeDb = Mathf.LinearToDb(Math.Clamp(volume, 0.001f, 3f));
			return;
		}
		ForcePlayBGM(name, volume, pitch);
	}

	internal bool BGMLocked { get; set; }

	internal void ForcePlayBGM(string name, float volume, float pitch)
	{
		if (!TryGetBGM(name, out AudioStreamOggVorbis stream))
		{
			GD.PrintErr("Unknown BGM: " + name);
			return;
		}
		
		// prevent people from blowing out their eardrums
		volume = Math.Clamp(volume, 0.001f, 3f);
		pitch = Math.Clamp(pitch, 0.1f, 2f);
		
		stream.LoopOffset = BGMLoopOffsets.GetValueOrDefault(name);
		CurrentBGM = name;
		BGM.Stream = stream;
		BGM.PitchScale = pitch;
		BGM.VolumeDb = Mathf.LinearToDb(volume);
		BGM.Play();
	}

	/// <summary>
	/// Tries to get the BGM with the given <paramref name="name"/>, built in or from a mod.
	/// </summary>
	/// <remarks>
	/// The returned stream is the one the game plays, not a copy, so changing it changes how that BGM plays.
	/// </remarks>
	/// <param name="name">The name of the BGM, as used by <c>PlayBGM</c>.</param>
	/// <param name="stream">The BGM's audio stream, if it exists.</param>
	/// <returns>Whether a BGM with that name exists. False, with a null stream, if <paramref name="name"/> is null.</returns>
	public bool TryGetBGM(string name, out AudioStreamOggVorbis stream)
	{
		if (name == null)
		{
			stream = null;
			return false;
		}

		if (BGMDictionary.TryGetValue(name, out stream))
			return true;
		
		if (ResourceLoader.Exists("res://audio/bgm/" + name + ".ogg"))
		{
			stream = ResourceLoader.Load<AudioStreamOggVorbis>("res://audio/bgm/" + name + ".ogg");
			stream.Loop = true;
			RegisterBGMLoopOffset(name, stream);
			BGMDictionary.Add(name, stream);
			return true;
		}

		return false;
	}

	/// <summary>
	/// Stops the currently playing BGM.
	/// </summary>
	/// <remarks>
	/// At the enemy level, use the <see cref="OmoriSandbox.Actors.Enemy.StopBGM"/> method to respect the <see cref="OmoriSandbox.Actors.Enemy.CanChangeBGM"/> setting.
	/// </remarks>
	public void StopBGM()
	{
		if (BGMLocked)
		{
			if (SettingsMenuManager.Instance.LogDebug)
				GD.Print("Ignored StopBGM(), the preset locks the BGM.");
			return;
		}
		BGM.Stop();
	}

	/// <summary>
	/// Plays the background sound (BGS) with the given <paramref name="name"/>, which plays alongside the BGM.
	/// </summary>
	/// <remarks>
	/// If that BGS is already playing, only its volume and pitch change. Does nothing while the preset locks the BGM.
	/// At the enemy level, use the <see cref="OmoriSandbox.Actors.Enemy.ChangeBGS"/> method to respect the <see cref="OmoriSandbox.Actors.Enemy.CanChangeBGM"/> setting.
	/// </remarks>
	/// <param name="name">The name of the BGS to play.</param>
	/// <param name="volume">The volume to play the BGS at, from 0.001 to 3.0.</param>
	/// <param name="pitch">The pitch to play the BGS at, from 0.1 to 2.0.</param>
	public void PlayBGS(string name, float volume = 1f, float pitch = 1f)
	{
		if (BGMLocked)
		{
			if (SettingsMenuManager.Instance.LogDebug)
				GD.Print($"Ignored PlayBGS(\"{name}\"), the preset locks the BGM.");
			return;
		}
		ForcePlayBGS(name, volume, pitch);
	}

	internal void ForcePlayBGS(string name, float volume, float pitch)
	{
		if (!TryGetBGS(name, out AudioStreamOggVorbis stream))
		{
			GD.PrintErr("Unknown BGS: " + name);
			return;
		}

		volume = Math.Clamp(volume, 0.001f, 3f);
		pitch = Math.Clamp(pitch, 0.1f, 2f);

		if (CurrentBGS != name)
		{
			KillBGSFade();
			stream.LoopOffset = BGSLoopOffsets.GetValueOrDefault(name);
			CurrentBGS = name;
			BGS.Stream = stream;
			BGS.PitchScale = pitch;
			BGS.VolumeDb = Mathf.LinearToDb(volume);
			BGS.Play();
			return;
		}
		BGS.PitchScale = pitch;
		BGS.VolumeDb = Mathf.LinearToDb(volume);
	}

	/// <summary>
	/// Fades the BGS out over the given number of <paramref name="seconds"/>, then stops it.
	/// </summary>
	/// <remarks>
	/// <see cref="CurrentBGS"/> is empty from the moment the fade starts, and a fade with no BGS playing does nothing.
	/// Does nothing while the preset locks the BGM.
	/// At the enemy level, use the <see cref="OmoriSandbox.Actors.Enemy.FadeOutBGS"/> method to respect the <see cref="OmoriSandbox.Actors.Enemy.CanChangeBGM"/> setting.
	/// </remarks>
	/// <param name="seconds">How long the fade takes, in seconds.</param>
	public void FadeOutBGS(float seconds)
	{
		if (BGMLocked)
		{
			if (SettingsMenuManager.Instance.LogDebug)
				GD.Print($"Ignored FadeOutBGS({seconds}), the preset locks the BGM.");
			return;
		}
		ForceFadeOutBGS(seconds);
	}

	internal void ForceFadeOutBGS(float seconds)
	{
		StartBGSFade(seconds);
	}

	/// <summary>
	/// Fades the BGS out over the given number of <paramref name="seconds"/>, then stops it, and waits for the fade to finish.
	/// </summary>
	/// <remarks>
	/// Completes at once if no BGS is playing or while the preset locks the BGM.
	/// At the enemy level, use the <see cref="OmoriSandbox.Actors.Enemy.WaitForFadeOutBGS"/> method to respect the <see cref="OmoriSandbox.Actors.Enemy.CanChangeBGM"/> setting.
	/// </remarks>
	/// <param name="seconds">How long the fade takes, in seconds.</param>
	public Task WaitForFadeOutBGS(float seconds)
	{
		if (BGMLocked)
		{
			if (SettingsMenuManager.Instance.LogDebug)
				GD.Print($"Ignored WaitForFadeOutBGS({seconds}), the preset locks the BGM.");
			return Task.CompletedTask;
		}
		return WaitForForceFadeOutBGS(seconds);
	}

	internal async Task WaitForForceFadeOutBGS(float seconds)
	{
		if (!StartBGSFade(seconds))
			return;
		Tween tween = BGSFadeTween;
		if (tween != null)
			await ToSignal(tween, Tween.SignalName.Finished);
	}

	private bool StartBGSFade(float seconds)
	{
		if (CurrentBGS == "")
			return false;
		CurrentBGS = "";
		KillBGSFade();
		if (seconds <= 0f)
		{
			BGS.Stop();
			return true;
		}
		Tween tween = CreateTween();
		BGSFadeTween = tween;
		tween.TweenMethod(Callable.From<float>(value => BGS.VolumeLinear = value), BGS.VolumeLinear, 0f, seconds);
		tween.TweenCallback(Callable.From(BGS.Stop));
		return true;
	}

	private void KillBGSFade()
	{
		if (BGSFadeTween == null)
			return;
		Tween tween = BGSFadeTween;
		BGSFadeTween = null;
		tween.EmitSignal(Tween.SignalName.Finished);
		tween.Kill();
	}

	/// <summary>
	/// Stops the BGS at once.
	/// </summary>
	/// <remarks>
	/// Does nothing while the preset locks the BGM.
	/// At the enemy level, use the <see cref="OmoriSandbox.Actors.Enemy.StopBGS"/> method to respect the <see cref="OmoriSandbox.Actors.Enemy.CanChangeBGM"/> setting.
	/// </remarks>
	public void StopBGS()
	{
		if (BGMLocked)
		{
			if (SettingsMenuManager.Instance.LogDebug)
				GD.Print("Ignored StopBGS(), the preset locks the BGM.");
			return;
		}
		ForceStopBGS();
	}

	internal void ForceStopBGS()
	{
		KillBGSFade();
		CurrentBGS = "";
		BGS.Stop();
	}

	/// <summary>
	/// Tries to get the BGS with the given <paramref name="name"/>.
	/// </summary>
	/// <remarks>
	/// The returned stream is the one the game plays, not a copy, so changing it changes how that BGS plays.
	/// </remarks>
	/// <param name="name">The name of the BGS, as used by <c>PlayBGS</c>.</param>
	/// <param name="stream">The BGS's audio stream, if it exists.</param>
	/// <returns>Whether a BGS with that name exists. False, with a null stream, if <paramref name="name"/> is null.</returns>
	public bool TryGetBGS(string name, out AudioStreamOggVorbis stream)
	{
		if (name == null)
		{
			stream = null;
			return false;
		}
		return BGSDictionary.TryGetValue(name, out stream);
	}

	/// <summary>
	/// The names of every BGS.
	/// </summary>
	public IEnumerable<string> GetAllBGS()
	{
		return BGSDictionary.Keys;
	}

	/// <summary>
	/// Pauses the currently playing BGM.
	/// </summary>
	public void PauseBGM(bool paused)
	{
		if (BGMLocked)
		{
			if (SettingsMenuManager.Instance.LogDebug)
				GD.Print($"Ignored PauseBGM({paused}), the preset locks the BGM.");
			return;
		}
		BGM.StreamPaused = paused;
	}

	/// <summary>
	/// Seeks the BGM to the given time in seconds.
	/// </summary>
	/// <param name="seconds">The time to seek to, in seconds.</param>
	public void SeekBGM(float seconds)
	{
		BGM.Seek(seconds);
	}

	/// <summary>
	/// Sets the loop offset for the BGM.
	/// </summary>
	/// <param name="seconds">The time offset to loop to, in seconds. 0 or less uses the track's own loop point. Values greater than the length of the track will be ignored.</param>
	public void SetBGMLoopOffset(double seconds)
	{
		if (BGMLocked)
		{
			if (SettingsMenuManager.Instance.LogDebug)
				GD.Print($"Ignored SetBGMLoopOffset({seconds}), the preset locks the BGM.");
			return;
		}
		ForceSetBGMLoopOffset(seconds);
	}

	internal void ForceSetBGMLoopOffset(double seconds)
	{
		if (BGM.Stream is AudioStreamOggVorbis stream)
		{
			if (seconds <= 0)
				seconds = BGMLoopOffsets.GetValueOrDefault(CurrentBGM);
			if (seconds < stream.GetLength())
				stream.LoopOffset = seconds;
		}
	}

	/// <summary>
	/// The current playback position of the BGM, in seconds.
	/// </summary>
	public float GetBGMPosition()
	{
		return BGM.GetPlaybackPosition() + (float)AudioServer.GetTimeSinceLastMix();
	}

	/// <summary>
	/// The point the given BGM loops back to by default, in seconds, or 0 if it has none.
	/// </summary>
	/// <remarks>
	/// This is the track's own loop point, read from its <c>LOOPSTART</c> tag. <see cref="SetBGMLoopOffset"/> changes the loop point of the BGM that's playing.
	/// A LoopOffset of 0 will fall back to the <c>LOOPSTART</c> tag, if one exists.
	/// </remarks>
	/// <param name="name">The name of the BGM, as used by <c>PlayBGM</c>.</param>
	/// <returns>The loop point in seconds, or 0 if the BGM has none, doesn't exist or <paramref name="name"/> is null.</returns>
	public double GetDefaultBGMLoopOffset(string name)
	{
		return name != null ? BGMLoopOffsets.GetValueOrDefault(name) : 0;
	}

	internal bool LoadCustomBGM(string path)
	{
		string name = path.GetFile().GetBaseName();
		if (BGMDictionary.ContainsKey(name))
		{
			GD.PushWarning($"BGM '{name}' already loaded, skipping.");
			return true;
		}
		AudioStreamOggVorbis stream = AudioStreamOggVorbis.LoadFromFile(path);
		if (stream == null)
		{
			GD.PushError($"Failed to load BGM file: {path}");
			return false;
		}
		stream.Loop = true;
		RegisterBGMLoopOffset(name, stream);
		return BGMDictionary.TryAdd(name, stream);
	}

	internal bool LoadCustomSFX(string path)
	{
		string name = path.GetFile().GetBaseName();
		if (SFXDictionary.ContainsKey(name))
		{
			GD.PushWarning($"SFX '{name}' already loaded, skipping.");
			return true;
		}
		AudioStreamOggVorbis stream = AudioStreamOggVorbis.LoadFromFile(path);
		if (stream == null)
		{
			GD.PushError($"Failed to load SFX file: {path}");
			return false;
		}
		return SFXDictionary.TryAdd(name, stream);
	}

	internal bool HasSFX(string name)
	{
		return SFXDictionary.ContainsKey(name);
	}

	private void RegisterBGMLoopOffset(string name, AudioStreamOggVorbis stream)
	{
		RegisterLoopOffset(BGMLoopOffsets, name, stream);
	}

	private static void RegisterLoopOffset(Dictionary<string, double> offsets, string name, AudioStreamOggVorbis stream)
	{
		double loopStart = ReadLoopStart(stream);
		if (loopStart > 0 && loopStart < stream.GetLength())
			stream.LoopOffset = loopStart;
		offsets[name] = stream.LoopOffset;
	}

	private static double ReadLoopStart(AudioStreamOggVorbis stream)
	{
		foreach (Variant page in stream.PacketSequence.PacketData)
		{
			foreach (Variant entry in page.AsGodotArray())
			{
				byte[] packet = entry.AsByteArray();
				if (packet.Length < 7 || packet[0] != 3 || Encoding.ASCII.GetString(packet, 1, 6) != "vorbis")
					continue;
				string comments = Encoding.ASCII.GetString(packet);
				double rate = stream.PacketSequence.SamplingRate;
				long loopLength = ReadTag(comments, "LOOPLENGTH=");
				return loopLength > 0 && rate > 0 ? ReadTag(comments, "LOOPSTART=") / rate : 0;
			}
		}
		return 0;
	}

	private static long ReadTag(string comments, string key)
	{
		int start = comments.IndexOf(key, StringComparison.Ordinal);
		if (start < 0)
			return 0;
		start += key.Length;
		int end = start;
		while (end < comments.Length && char.IsAsciiDigit(comments[end]))
			end++;
		return long.TryParse(comments[start..end], out long value) ? value : 0;
	}

	/// <summary>
	/// Fades the BGM to the given <paramref name="volume"/> over the given number of <paramref name="seconds"/>.
	/// </summary>
	/// <remarks>
	/// At the enemy level, use the <see cref="OmoriSandbox.Actors.Enemy.FadeBGM"/> method to respect the <see cref="OmoriSandbox.Actors.Enemy.CanChangeBGM"/> setting.
	/// </remarks>
	/// <param name="volume">The volume to fade the BGM to, from 0.001 to 3.0.</param>
	/// <param name="seconds">How long it should take for the BGM to fade, in seconds.</param>
	public void FadeBGMTo(float volume, float seconds = 1f)
	{
		volume = Math.Clamp(volume, 0.001f, 3f);
		float target = Mathf.LinearToDb(volume);
		if (VolumeTween != null)
		{
			VolumeTween.EmitSignal(Tween.SignalName.Finished);
			VolumeTween.Kill();
			VolumeTween = null;
		}
		if (seconds == 0f)
			BGM.VolumeDb = target;
		else
		{
			VolumeTween = CreateTween();
			VolumeTween.TweenProperty(BGM, "volume_db", target, seconds);
		}
	}

	/// <summary>
	/// Fades the BGM to the given <paramref name="volume"/> over the given number of <paramref name="seconds"/> and waits for it to finish.
	/// </summary>
	/// <remarks>
	/// At the enemy level, use the <see cref="OmoriSandbox.Actors.Enemy.FadeBGM"/> method to respect the <see cref="OmoriSandbox.Actors.Enemy.CanChangeBGM"/> setting.
	/// </remarks>
	/// <param name="volume">The volume to fade the BGM to, from 0.001 to 3.0.</param>
	/// <param name="seconds">How long it should take for the BGM to fade, in seconds.</param>
	public async Task WaitForFadeBGMTo(float volume, float seconds = 1f)
	{
		FadeBGMTo(volume, seconds);
		Tween tween = VolumeTween;
		if (tween != null)
			await ToSignal(tween, Tween.SignalName.Finished);
	}

	/// <summary>
	/// Sets the pitch of the currently playing BGM, from 0.1 to 2.0.
	/// </summary>
	/// <param name="pitch">The pitch to set the BGM to, from 0.1 to 2.0.</param>
	public void SetBGMPitch(float pitch)
	{
		FadeBGMPitchTo(pitch, 0f);
	}

	/// <summary>
	/// Fades the BGM pitch to the given <paramref name="pitch"/> over the given number of <paramref name="seconds"/>.
	/// </summary>
	/// <param name="pitch">The pitch to fade the BGM to, from 0.1 to 2.0.</param>
	/// <param name="seconds">How long it should take for the pitch to change, in seconds.</param>
	public void FadeBGMPitchTo(float pitch, float seconds = 1f)
	{
		pitch = Math.Clamp(pitch, 0.1f, 2f);
		if (PitchTween != null)
		{
			// killed tweens don't emit finished so do it ourselves
			PitchTween.EmitSignal(Tween.SignalName.Finished);
			PitchTween.Kill();
			PitchTween = null;
		}
		if (seconds == 0f)
			BGM.PitchScale = pitch;
		else
		{
			PitchTween = CreateTween();
			PitchTween.TweenProperty(BGM, "pitch_scale", pitch, seconds);
		}
	}

	/// <summary>
	/// Fades the BGM pitch to the given <paramref name="pitch"/> over the given number of <paramref name="seconds"/> and waits for it to finish.
	/// </summary>
	/// <param name="pitch">The pitch to fade the BGM to, from 0.1 to 2.0.</param>
	/// <param name="seconds">How long it should take for the pitch to change, in seconds.</param>
	public async Task WaitForFadeBGMPitchTo(float pitch, float seconds = 1f)
	{
		FadeBGMPitchTo(pitch, seconds);
		Tween tween = PitchTween;
		if (tween != null)
			await ToSignal(tween, Tween.SignalName.Finished);
	}

	private void OnSFXFinish(AudioStreamPlayer player)
	{
		player.PitchScale = 1f;
		player.VolumeLinear = 1f;
		foreach ((string name, AudioStreamPlayer playing) in PlayingSounds)
		{
			if (playing == player)
			{
				PlayingSounds.Remove(name);
				break;
			}
		}
	}

	/// <summary>
	/// The names of every BGM, built in and from mods.
	/// </summary>
	public IEnumerable<string> GetAllBGM()
	{
		return BGMDictionary.Keys;
	}

	internal void Reset()
	{
		PlayingSounds.Clear();
		PitchTween?.Kill();
		PitchTween = null;
		VolumeTween?.Kill();
		VolumeTween = null;
		BGM.Stop();
		BGM.PitchScale = 1f;
		BGM.VolumeDb = Mathf.LinearToDb(1f);
		ForceStopBGS();
		BGS.PitchScale = 1f;
		BGS.VolumeDb = Mathf.LinearToDb(1f);
		foreach (AudioStreamPlayer player in AudioPlayers)
		{
			player.Stop();
			player.Stream = null;
			player.PitchScale = 1f;
			player.VolumeLinear = 1f;
		}
	}
}
