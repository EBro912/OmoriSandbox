using Godot;
using OmoriSandbox.Actors;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OmoriSandbox.Editor;

namespace OmoriSandbox;

/// <summary>
/// Handles displaying messages in the battle log during battles.
/// </summary>
public partial class BattleLogManager : Control
{
	/// <summary>
	/// Fired whenever the battle log has finished logging messages.
	/// </summary>
	[Signal] public delegate void FinishedLoggingEventHandler();

	[Export] private PackedScene LogLine;
	[Export] private RichTextLabel ImmediateLabel;
	[Export] private Font Font;
	[Export] private Sprite2D Icon;

	private readonly Queue<string> MessageQueue = [];
	private readonly Queue<string> LineQueue = [];
	private readonly List<Control> ActiveLines = [];
	private const int HEIGHT = 26;
	private const int MAX_WIDTH = 35;
	private readonly Vector2I NO_ICON = new(335, 78);
	private readonly Vector2I WITH_ICON = new(255, 78);
	private const int ICON_SIZE = 108;
	
	/// <summary>
	/// Returns true when the battle log is busy with messages.
	/// </summary>
	public bool IsProcessingMessage { get; private set; } = false;
	public static BattleLogManager Instance { get; private set; }

	public override void _EnterTree()
	{
		Instance = this;
	}

	/// <summary>
	/// Queues a message to be displayed in the battle log that shows the names of actors.
	/// </summary>
	/// <remarks>
	/// You can use [actor] and [target] in the message string to replace them with <paramref name="self"/> and <paramref name="target"/> respectively.
	/// </remarks>
	/// <param name="self">The actor to replace [actor] with in the <paramref name="message"/>.</param>
	/// <param name="target">The actor to replace [target] with in the <paramref name="message"/>.</param>
	/// <param name="message">The message to display. Occurences of the \n character will split the message up into different logs.</param>
	public void QueueMessage(Actor self, Actor target, string message)
	{
		QueueMessage(ParseMessage(self, target, message));
	}

	/// <summary>
	/// Queues a message to be displayed in the battle log that shows the names of the actor.
	/// </summary>
	/// <remarks>
	/// You can use [actor] in the message string to replace it with <paramref name="actor"/>.
	/// </remarks>
	/// <param name="actor">The actor to replace [actor] with in the <paramref name="message"/>.</param>
	/// <param name="message">The message to display. Occurences of the \n character will split the message up into different logs.</param>
	public void QueueMessage(Actor actor, string message)
	{
		QueueMessage(ParseMessage(actor, null, message));
	}

	/// <summary>
	/// Queues a message to be displayed in the battle log.
	/// </summary>
	/// <param name="message">The message to display. Occurrences of the \n character will split the message up into different logs.</param>
	public void QueueMessage(string message)
	{
		MessageQueue.Enqueue(message);

		if (!IsProcessingMessage)
			ProcessMessage();
	}

	/// <summary>
	/// Immediately shows a message in the battle log.<br/>
	/// Accepts Godot BBCode for formatting.
	/// </summary>
	/// <param name="message">The message to display.</param>
	public void ShowMessage(string message)
	{
		ImmediateLabel.Size = NO_ICON;
		Icon.Visible = false;
		ImmediateLabel.Text = message;
	}

	/// <summary>
	/// Immediately shows a message in the battle log, as well as an item icon from the specified <see cref="spritesheetPath"/>.<br/>
	/// Accepts Godot BBCode for formatting.
	/// </summary>
	/// <param name="message">The message to display.</param>
	/// <param name="spritesheetPath">The path to the spritesheet to use.</param>
	/// <param name="index">The atlas index of the sprite.</param>
	public void ShowMessageWithIcon(string message, string spritesheetPath, int index)
	{
		ImmediateLabel.Size = WITH_ICON;
		Icon.Visible = true;
		Icon.Texture = ResourceLoader.Load<Texture2D>(spritesheetPath);
		int columns = Icon.Texture.GetWidth() / ICON_SIZE;
		int column = index % columns;
		int row = index / columns;
		Icon.RegionEnabled = true;
		Icon.RegionRect = new Rect2(column * ICON_SIZE, row * ICON_SIZE, ICON_SIZE, ICON_SIZE);
		ImmediateLabel.Text = message;
	}

	/// <summary>
	/// Immediately shows a message in the battle log, clearing any queued or active messages first.
	/// </summary>
	/// <param name="message">The message to display.</param>
	public void ClearAndShowMessage(string message)
	{
		ClearBattleLog();
		ShowMessage(message);
	}

	/// <summary>
	/// Immediately shows a message in the battle log, clearing any queued or active messages first.<br/>
	/// Also shows an item icon from the specified <see cref="spritesheetPath"/>.
	/// </summary>
	/// <param name="message"></param>
	/// <param name="spritesheetPath"></param>
	/// <param name="index"></param>
	public void ClearAndShowMessageWithIcon(string message, string spritesheetPath, int index)
	{
		ClearBattleLog();
		ShowMessageWithIcon(message, spritesheetPath, index);
	}

	/// <summary>
	/// Immediately shows a message in the battle log, clearing any queued or active messages first.
	/// </summary>
	/// <remarks>
	/// You can use [actor] and [target] in the message string to replace them with <paramref name="self"/> and <paramref name="target"/> respectively.
	/// </remarks>
	/// <param name="self">The actor representing the sender of the message.</param>
	/// <param name="target">The actor representing the recipient of the message.</param>
	/// <param name="message">The raw message to be formatted and displayed.</param>
	public void ClearAndShowMessage(Actor self, Actor target, string message)
	{
		ClearAndShowMessage(ParseMessage(self, target, message));
	}

	/// <summary>
	/// Immediately shows a message in the battle log, clearing any queued or active messages first.<br/>
	/// Also shows an item icon from the specified <see cref="spritesheetPath"/>.
	/// </summary>
	/// <remarks>
	/// You can use [actor] and [target] in the message string to replace them with <paramref name="self"/> and <paramref name="target"/> respectively.
	/// </remarks>
	/// <param name="self"></param>
	/// <param name="target"></param>
	/// <param name="message"></param>
	/// <param name="spritesheetPath"></param>
	/// <param name="index"></param>
	public void ClearAndShowMessageWithIcon(Actor self, Actor target, string message, string spritesheetPath, int index)
	{
		ClearAndShowMessageWithIcon(ParseMessage(self, target, message), spritesheetPath, index);
	}

	/// <summary>
	/// Clears the battle log of all queued and active messages.
	/// </summary>
	/// <remarks>
	/// See <see cref="ClearAndShowMessage(string)"/> for a shorthand to clear the log and show a message.
	/// </remarks>
	public void ClearBattleLog()
	{
		MessageQueue.Clear();
		LineQueue.Clear();
		ActiveLines.ForEach(x => x.QueueFree());
		ActiveLines.Clear();
		ImmediateLabel.Text = "";
		Icon.Visible = false;
		IsProcessingMessage = false;
	}

	/// <summary>
	/// Waits for the battle log to finish logging messages.
	/// </summary>
	/// <remarks>
	/// This method should be avoided unless you know what you're doing.
	/// In most situations, battle log waiting will be handled for you automatically.
	/// </remarks>
	public async Task WaitForBattleLog()
	{
		if (!IsProcessingMessage)
			return;
		
		await ToSignal(this, SignalName.FinishedLogging);
	}

	private string ParseMessage(Actor self, Actor target, string message)
	{
		return message.Replace("[actor]", self == null ? "" : self.Name.ToUpper()).Replace("[target]", target == null ? "" : target.Name.ToUpper());
	}

	private async void ProcessMessage()
	{
		IsProcessingMessage = true;
		while (MessageQueue.Count > 0)
		{
			string next = MessageQueue.Dequeue();
			List<string> wrapped = WordWrap(next);

			// if the message was auto-wrapped, ignore \n
			if (wrapped.Count > 1)
			{
				foreach (string wrap in wrapped)
					LineQueue.Enqueue(wrap.Replace('\n', ' '));
			}
			else
			{
				// otherwise, respect \n
				foreach (string line in next.Split('\n'))
					LineQueue.Enqueue(line);
			}

			await ProcessLines();
		}

		IsProcessingMessage = false;
		EmitSignal(SignalName.FinishedLogging);
	}

	private async Task ProcessLines()
	{
		while (LineQueue.Count > 0)
		{
			while (ActiveLines.Count >= 3)
			{
				MoveOffScreen(ActiveLines[0]);
				ActiveLines.RemoveAt(0);
			}

			Control newLine = LogLine.Instantiate<Control>();
			newLine.GetNode<Label>("Label").Text = LineQueue.Dequeue();
			newLine.Position = new Vector2(11, ActiveLines.Count * HEIGHT);
			newLine.Modulate = new Color(1f, 1f, 1f, 0f);
			AddChild(newLine);
			ActiveLines.Add(newLine);

			Tween tween = GetTree().CreateTween();
			tween.TweenProperty(newLine, "modulate:a", 1f, 0.15f);

			for (int i = 0; i < ActiveLines.Count; i++)
			{
				Control line = ActiveLines[i];
				Vector2 target = new(11, i * HEIGHT);
				Tween repositionTween = GetTree().CreateTween();
				repositionTween.TweenProperty(line, "position", target, 0.15f)
							   .SetTrans(Tween.TransitionType.Sine);
			}

			await ToSignal(GetTree().CreateTimer(MessageDelay), "timeout");
		}
	}

	private List<string> WordWrap(string text)
	{
		List<string> result = [];

		if (string.IsNullOrWhiteSpace(text))
		{
			return result;
		}

		string[] words = text.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
		StringBuilder line = new(words[0]);

		for (int i = 1; i < words.Length; i++)
		{
			string word = words[i];
			if ((line.Length + 1 + word.Length) <= MAX_WIDTH)
			{
				line.Append(' ').Append(word);
			}
			else
			{
				result.Add(line.ToString());
				line.Clear();
				line.Append(word);
			}
		}
		result.Add(line.ToString());
		return result;
	}

	private void MoveOffScreen(Control line)
	{
		Tween tween = GetTree().CreateTween();
		tween.TweenProperty(line, "position", new Vector2(11, -HEIGHT), 0.15f)
			.SetTrans(Tween.TransitionType.Sine);
		tween.TweenProperty(line, "modulate:a", 0f, 0.15f)
			.SetTrans(Tween.TransitionType.Sine);
		tween.TweenCallback(Callable.From(() => line.QueueFree()));
	}

	private float MessageDelay => SettingsMenuManager.Instance.BattlelogSpeed switch
	{
		1 => 0.8f,
		2 => 0.6f,
		4 => 0.25f,
		5 => 0.15f,
		_ => 0.4f
	};
}
