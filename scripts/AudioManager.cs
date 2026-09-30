using System;
using Godot;
using OmoriSandbox.Animation;
using OmoriSandbox.Editor;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;

namespace OmoriSandbox;

/// <summary>
/// Handles all audio playback, including SFX and BGM.
/// </summary>
public partial class AudioManager : Node
{
	[Export] internal AudioStreamPlayer BGM { get; private set; }

	private readonly List<AudioStreamPlayer> AudioPlayers = [];

	private readonly Dictionary<string, AudioStreamOggVorbis> SFXDictionary = [];
	private readonly SortedDictionary<string, AudioStreamOggVorbis> BGMDictionary = [];
	private readonly Dictionary<string, double> BGMLoopOffsets = [];
	private string CurrentBGM = "";

	public static AudioManager Instance { get; private set; }

	// only one instance of a sound can play at once
	private Dictionary<string, AudioStreamPlayer> PlayingSounds = [];
	
	// reusable tween so multiple pitch tweens don't fight over the BGM
	private Tween PitchTween;

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
	/// <param name="name">The name of the BGM to play.</param>
	public void PlayBGM(string name)
	{
		PlayBGM(name, 1f, 1f);
	}

	/// <summary>
	/// Plays BGM with the given <paramref name="name"/> and desired parameters.
	/// </summary>
	/// <param name="name">The name of the BGM to play.</param>
	/// <param name="volume">The volume to play the BGM at, from 0.001 to 3.0.</param>
	/// <param name="pitch">The pitch to play the BGM at, from 0.1 to 2.0.</param>
	public void PlayBGM(string name, float volume, float pitch)
	{
		if (BGMLocked)
		{
			if (SettingsMenuManager.Instance.LogDebug)
				GD.Print($"Ignored PlayBGM(\"{name}\"), the preset locks the BGM.");
			BGM.VolumeDb = Mathf.LinearToDb(volume);
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

	internal bool TryGetBGM(string name, out AudioStreamOggVorbis stream)
	{
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

	internal float GetBGMPosition()
	{
		return BGM.GetPlaybackPosition() + (float)AudioServer.GetTimeSinceLastMix();
	}

	internal double GetDefaultBGMLoopOffset(string name)
	{
		return BGMLoopOffsets.GetValueOrDefault(name);
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
		double loopStart = ReadLoopStart(stream);
		if (loopStart > 0 && loopStart < stream.GetLength())
			stream.LoopOffset = loopStart;
		BGMLoopOffsets[name] = stream.LoopOffset;
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
	/// <param name="volume">The volume to fade the BGM to, from 0.001 to 3.0.</param>
	/// <param name="seconds">How long it should take for the BGM to fade, in seconds.</param>
	public void FadeBGMTo(float volume, float seconds = 1f)
	{
		volume = Math.Clamp(volume, 0.001f, 3f);
		float target = Mathf.LinearToDb(volume);
		if (seconds == 0f)
			BGM.VolumeDb = target;
		else
		{
			Tween tween = CreateTween();
			tween.TweenProperty(BGM, "volume_db", target, seconds);
		}
	}

	/// <summary>
	/// Fades the BGM to the given <paramref name="volume"/> over the given number of <paramref name="seconds"/> and waits for it to finish.
	/// </summary>
	/// <param name="volume">The volume to fade the BGM to, from 0.001 to 3.0.</param>
	/// <param name="seconds">How long it should take for the BGM to fade, in seconds.</param>
	public async Task WaitForFadeBGMTo(float volume, float seconds = 1f)
	{
		volume = Math.Clamp(volume, 0.001f, 3f);
		float target = Mathf.LinearToDb(volume);
		if (seconds == 0f)
			BGM.VolumeDb = target;
		else
		{
			Tween tween = CreateTween();
			tween.TweenProperty(BGM, "volume_db", target, seconds);
			await ToSignal(tween, Tween.SignalName.Finished);
		}
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

	internal IEnumerable<string> GetAllBGM()
	{
		return BGMDictionary.Keys;
	}

	internal void Reset()
	{
		PlayingSounds.Clear();
		PitchTween?.Kill();
		PitchTween = null;
		BGM.Stop();
		BGM.PitchScale = 1f;
		BGM.VolumeDb = Mathf.LinearToDb(1f);
		foreach (AudioStreamPlayer player in AudioPlayers)
		{
			player.Stop();
			player.Stream = null;
			player.PitchScale = 1f;
			player.VolumeLinear = 1f;
		}
	}
}
