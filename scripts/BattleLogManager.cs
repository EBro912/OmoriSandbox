using Godot;
using System.Collections.Generic;

public partial class BattleLogManager : Node
{
    [Signal] public delegate void FinishedLoggingEventHandler();

    [Export] public PackedScene LogLine;
    [Export] public Label ImmediateLabel;

    private readonly Queue<string> MessageQueue = [];
    private readonly List<Control> ActiveLines = [];
    private const int HEIGHT = 26;
    public bool IsProcessingMessage { get; private set; } = false;
    public static BattleLogManager Instance { get; private set; }

    public override void _EnterTree()
    {
        Instance = this;
    }

    public void QueueMessage(Actor self, Actor target, string message)
    {
        QueueMessage(ParseMessage(self, target, message));
    }

    public void QueueMessage(string message)
    {
        MessageQueue.Enqueue(message);

        if (!IsProcessingMessage)
            ProcessMessage();
    }

    public void ShowMessage(string message)
    {
        ImmediateLabel.Text = message;
    }

    public void ShowMessage(Actor self, Actor target, string message)
    {
        ImmediateLabel.Text = ParseMessage(self, target, message);
    }

    public void ClearAndShowMessage(string message)
    {
        ClearBattleLog();
        ShowMessage(message);
    }

    public void ClearAndShowMessage(Actor self, Actor target, string message)
    {
        ClearAndShowMessage(ParseMessage(self, target, message));
    }

    public void ClearBattleLog()
    {
        MessageQueue.Clear();
        ActiveLines.ForEach(x => x.QueueFree());
        ActiveLines.Clear();
        ImmediateLabel.Text = "";
        IsProcessingMessage = false;
    }

    public static string ParseMessage(Actor self, Actor target, string message)
    {
        return message.Replace("[actor]", self.Name.ToUpper()).Replace("[target]", target == null ? "" : target.Name.ToUpper());
    }

    private async void ProcessMessage()
    {
        if (MessageQueue.Count == 0)
        {
            IsProcessingMessage = false;
            EmitSignal(SignalName.FinishedLogging);
            return;
        }

        IsProcessingMessage = true;
        string next = MessageQueue.Dequeue();

        string[] lines = next.Split('\n');

        while (ActiveLines.Count >= 3)
        {
            MoveOffScreen(ActiveLines[0]);
            ActiveLines.RemoveAt(0);
        }

        for (int i = 1; i < lines.Length; i++)
        {
            QueueMessage(lines[i]);
        }

        Control newLine = LogLine.Instantiate<Control>();
        newLine.GetNode<Label>("Label").Text = lines[0];
        newLine.Position = new Vector2(11, ActiveLines.Count * HEIGHT);
        newLine.Modulate = new Color(1f, 1f, 1f, 0f);
        AddChild(newLine);
        ActiveLines.Add(newLine);

        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(newLine, "modulate:a", 1f, 0.2f);

        for (int i = 0; i < ActiveLines.Count; i++)
        {
            Control line = ActiveLines[i];
            Vector2 target = new(11, i * HEIGHT);
            Tween repositionTween = GetTree().CreateTween();
            repositionTween.TweenProperty(line, "position", target, 0.2f)
                           .SetTrans(Tween.TransitionType.Sine);
        }

        await ToSignal(GetTree().CreateTimer(0.4f), "timeout");

        ProcessMessage();
    }

    private void MoveOffScreen(Control line)
    {
        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(line, "position", new Vector2(11, -HEIGHT), 0.2f)
            .SetTrans(Tween.TransitionType.Sine);
        tween.TweenProperty(line, "modulate:a", 0f, 0.2f)
            .SetTrans(Tween.TransitionType.Sine);
        tween.TweenCallback(Callable.From(() => line.QueueFree()));
    }

}