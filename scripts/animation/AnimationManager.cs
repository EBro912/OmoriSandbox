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

	private Dictionary<string, RPGMAnimatedSprite> Animations = [];

	private const float FPS = 15f;
	private const float ShakeFPS = 60f;
	private float FrameDuration = 1f / FPS;
	private float ShakeFrameDuration = 1f / ShakeFPS;
	private float FrameTimer = 0f;
	private float ShakeFrameTimer = 0f;
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
			RPGMAnimatedSprite animation = new(info.Name, info.Layer, GD.Load<Texture2D>($"res://assets/animations/{info.Name}.png"));
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
				animation.SetFrameSFX(sfx.Frame, sfx.Name);
			}
			foreach (ShakeInfo shake in info.Shake)
			{
				animation.SetFrameShake(shake.Frame, shake.Power, shake.Speed, shake.Duration);
			}
			Animations.Add(info.Name, animation);
			GD.Print("Loaded animation: " + info.Name);
		}
	}

	public override void _Process(double delta)
	{
		if (ShakeDuration > 0 || Shake != 0f)
		{
			ShakeFrameTimer += (float)delta;
			if (ShakeFrameTimer >= ShakeFrameDuration)
			{
				ShakeFrameTimer -= ShakeFrameDuration;
				UpdateShake();
			}
		}

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
				DrawTexture(ImageTexture.CreateFromImage(img), DrawPosition + new Vector2(frame.X, frame.Y));
				continue;
			}				
			DrawTexture(texture, DrawPosition + new Vector2(frame.X, frame.Y));
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
		if (CurrentAnimation.TryGetFrameSFX(CurrentFrame, out string sfx))
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
		Battleback.Position += new Vector2((float)Math.Round(Shake / 2f), 0f);
		if (ShakeDuration < 1)
			ResetShake();
	}

	private void InitShake(Shake shake)
	{
		Battleback.Position = Vector2.Zero;
		Shake = 0f;
		ShakePwr = shake.Power;
		ShakeSpd = shake.Speed;
		ShakeDuration = shake.Duration;
		ShakeFrameTimer = 0f;
	}

	private void ResetShake()
	{
		Battleback.Position = Vector2.Zero;
		Shake = 0f;
		ShakePwr = 0f;
		ShakeSpd = 0f;
		ShakeDuration = 0;
		ShakeFrameTimer = 0f;
	}

	public void PlayAnimation(string name, Actor target = null)
	{
		if (target == null)
		{
			StartAnimation(name, new Vector2(320, 240));
		}
		else
		{
			StartAnimation(name, target.CenterPoint);
		}
	}

	public Task WaitForAnimation(string name, Actor target = null)
	{
		TaskCompletionSource tcs = new();

		void Handle()
		{
			AnimationFinished -= Handle;
			tcs.SetResult();
		}

		PlayAnimation(name, target);
		AnimationFinished += Handle;
		return tcs.Task;
	}

	private void StartAnimation(string name, Vector2 position)
	{
		if (!Animations.TryGetValue(name, out RPGMAnimatedSprite animation))
		{
			GD.PrintErr("Unknown animation: " + name);
			return;
		}

		CurrentAnimation = animation;
		DrawPosition = position - new Vector2(96f, 96f);
		CurrentFrame = 0;
		FrameTimer = 0f;
		IsPlaying = true;

		if (CurrentAnimation.TryGetFrameSFX(0, out string sfx))
		{
			AudioManager.Instance.PlaySFX(sfx);
		}

		QueueRedraw();
	}
}

class AnimationInfo
{
	public string Name;
	public int Layer;
	public float[][][] Frames;
	public SFXInfo[] SFX;
	public ShakeInfo[] Shake;
}

class SFXInfo
{
	public int Frame;
	public string Name;
	public int Volume;
}

class ShakeInfo
{
	public int Frame;
	public int Power;
	public int Speed;
	public int Duration;
}
