using Godot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class AnimationManager : Node2D
{
	// TODO: handle ZIndex

	[Signal]
	public delegate void AnimationFinishedEventHandler();

	private TextureRect Battleback;

	private Dictionary<int, RPGMAnimatedSprite> Animations = [];

	private const float FPS = 15f;
	private float FrameDuration = 1f / FPS;
	private float FrameTimer = 0f;
	private int CurrentFrame = 0;
	private RPGMAnimatedSprite CurrentAnimation;
	private Vector2 DrawPosition = Vector2.Zero;
	public bool IsPlaying { get; private set; } = false;

	private float Shake = 0f;
	private float ShakePwr = 0f;
	private float ShakeSpd = 0f;
	private int ShakeDuration = 0;
	private float ShakeDirection = -1f;

	public override void _Ready()
	{
		Battleback = GetNode<TextureRect>("../../UI/Battleback");

		string data = FileAccess.GetFileAsString("res://animations/animations.json");
		List<AnimationInfo> animationData = JsonConvert.DeserializeObject<List<AnimationInfo>>(data);
		foreach (AnimationInfo info in animationData)
		{
			RPGMAnimatedSprite animation;

			if (!string.IsNullOrWhiteSpace(info.AltTexture))
				animation = new(info.Id, info.Layer, GD.Load<Texture2D>($"res://assets/animations/{info.Texture}.png"), GD.Load<Texture2D>($"res://assets/animations/{info.AltTexture}.png"));
			else
				animation = new(info.Id, info.Layer, GD.Load<Texture2D>($"res://assets/animations/{info.Texture}.png"));

			foreach (float[][] frame in info.Frames)
			{
				List<Frame> frames = [];
				foreach (float[] f in frame)
				{
					frames.Add(new Frame((int)f[0], f[1], f[2], f[3], f[4], f[5] == 1, f[6]));
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
			GD.Print("Loaded animation: " + info.Id);
		}
	}

	public override void _Process(double delta)
	{
		if (!IsPlaying || CurrentAnimation == null)
			return;

		FrameTimer += (float)delta;

		if (FrameTimer >= FrameDuration)
		{
			FrameTimer -= FrameDuration;
			NextFrame();
			QueueRedraw();
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

	public override void _Draw()
	{
		if (!IsPlaying || CurrentAnimation == null)
			return;

		foreach (Frame frame in CurrentAnimation.GetFrame(CurrentFrame))
		{
			AtlasTexture texture = CurrentAnimation.GetTextureAt(frame.Pattern);
			if (frame.Mirror)
			{
				// TODO: dont use GetImage here
				Image img = texture.GetImage();
				img.FlipX();
				DrawTexture(ImageTexture.CreateFromImage(img), DrawPosition + new Vector2(frame.X, frame.Y), new Color(1f, 1f, 1f, frame.Opacity / 255f));
				continue;
			}				
			DrawTexture(texture, DrawPosition + new Vector2(frame.X, frame.Y), new Color(1f, 1f, 1f, frame.Opacity / 255f));
		}
	}

	private void NextFrame()
	{
		CurrentFrame++;
		if (CurrentFrame >= CurrentAnimation.FrameCount)
		{
			IsPlaying = false;
			CurrentAnimation = null;
			EmitSignal(SignalName.AnimationFinished);
			return;
		}
		if (CurrentAnimation.TryGetFrameSFX(CurrentFrame, out SFX sfx))
		{
			AudioManager.Instance.PlaySFX(sfx);
		}
		if (CurrentAnimation.TryGetFrameShake(CurrentFrame, out Shake shake))
		{
			InitShake(shake);
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
	}

	private void InitShake(Shake shake)
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

	public void PlayAnimation(int id, Actor target = null)
	{
		if (target == null)
		{
			StartAnimation(id, new Vector2(320, 240));
		}
		else
		{
			StartAnimation(id, target.CenterPoint);
		}
	}

	public Task WaitForAnimation(int id, Actor target = null)
	{
		TaskCompletionSource tcs = new();

		void Handle()
		{
			AnimationFinished -= Handle;
			tcs.SetResult();
		}

		PlayAnimation(id, target);
		AnimationFinished += Handle;
		return tcs.Task;
	}

	// TODO: support multiple animations on the same frame
	private void StartAnimation(int id, Vector2 position)
	{
		if (!Animations.TryGetValue(id, out RPGMAnimatedSprite animation))
		{
			GD.PrintErr("Unknown animation: " + id);
			return;
		}

		CurrentAnimation = animation;
		DrawPosition = position - new Vector2(96f, 96f);
		// hack fix for the headbutt curtain animation
		if (id == 30)
			DrawPosition += new Vector2(6f, 0f);
		CurrentFrame = 0;
		FrameTimer = 0f;
		IsPlaying = true;

		switch (animation.Layer)
		{
			case 0:
				ZIndex = 10;
				break;
			case 2:
				ZIndex = -1;
				break;
			case 3:
				ZIndex = -4;
				break;
		}

		if (CurrentAnimation.TryGetFrameSFX(0, out SFX sfx))
		{
			AudioManager.Instance.PlaySFX(sfx);
		}

		QueueRedraw();
	}
}

class AnimationInfo
{
	public int Id;
	public int Layer;
	public string Texture;
	public string AltTexture;
	public float[][][] Frames;
	public SFXInfo[] SFX;
	public ShakeInfo[] Shake;
}

class SFXInfo
{
	public int Frame;
	public string Name;
	public float Pitch;
	public float Volume;
}

class ShakeInfo
{
	public int Frame;
	public int Power;
	public int Speed;
	public int Duration;
}
