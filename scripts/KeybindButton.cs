using Godot;

namespace OmoriSandbox;

internal partial class KeybindButton : Control
{
    [Export] public string AssociatedAction { get; private set; }
    [Export] public Key DefaultKey { get; private set; }
    public Key CurrentKey { get; private set; }
    public InputEvent CurrentControllerBinding { get; private set; }
    public InputEvent DefaultControllerBinding { get; private set; }
    private bool WaitingForInput = false;
    private bool CapturingController;
    private InputEvent InputToRelease;
    private const double WaitTimeout = 10d;
    private double WaitTime = 0;

    // only one keybind may capture input at a time
    private static KeybindButton CurrentlyCapturing;
    private static ulong SuppressedFrame = ulong.MaxValue;
    internal static bool IsCapturing => CurrentlyCapturing != null || SuppressedFrame == Engine.GetProcessFrames();

    private Button KeyButton;
    private Button ControllerButton;

    public override void _Ready()
    {
        GetNode<Label>("ActionLabel").Text = AssociatedAction;
        KeyButton = GetNode<Button>("KeyButton");
        ControllerButton = GetNode<Button>("ControllerButton");
        foreach (InputEvent input in InputMap.ActionGetEvents(AssociatedAction))
        {
            if (ControllerBinding.IsController(input))
            {
                DefaultControllerBinding = ControllerBinding.Normalize(input);
                break;
            }
        }
        Reset();
        KeyButton.Pressed += () => StartCapture(false);
        ControllerButton.Pressed += () => StartCapture(true);
        VisibilityChanged += () =>
        {
            if (!IsVisibleInTree())
                CancelCapture();
        };
    }

    private void StartCapture(bool controller)
    {
        if (WaitingForInput && CapturingController == controller)
        {
            CancelCapture();
            return;
        }
        CurrentlyCapturing?.CancelCapture();
        CurrentlyCapturing = this;
        WaitingForInput = true;
        CapturingController = controller;
        WaitTime = 0;
        (controller ? ControllerButton : KeyButton).Text = "...";
    }

    private void CancelCapture()
    {
        WaitingForInput = false;
        InputToRelease = null;
        WaitTime = 0;
        RefreshLabels();
        if (CurrentlyCapturing == this)
        {
            CurrentlyCapturing = null;
            SuppressedFrame = Engine.GetProcessFrames();
        }
    }

    public override void _ExitTree() => CancelCapture();

    public override void _Process(double delta)
    {
        if (!WaitingForInput)
            return;

        WaitTime += delta;
        if (WaitTime >= WaitTimeout)
            CancelCapture();
    }

    public override void _Input(InputEvent @event)
    {
        if (!WaitingForInput)
            return;

        if (@event is InputEventKey or InputEventJoypadButton or InputEventJoypadMotion)
            GetViewport().SetInputAsHandled();

        // keep consuming a captured press until it is released
        if (InputToRelease != null)
        {
            bool released = InputToRelease switch
            {
                InputEventKey key => @event is InputEventKey next && next.Keycode == key.Keycode && !next.Pressed,
                InputEventJoypadButton button => @event is InputEventJoypadButton next &&
                    next.Device == button.Device && next.ButtonIndex == button.ButtonIndex && !next.Pressed,
                InputEventJoypadMotion motion => @event is InputEventJoypadMotion next &&
                    next.Device == motion.Device && next.Axis == motion.Axis &&
                    (Mathf.Abs(next.AxisValue) < InputMap.ActionGetDeadzone(AssociatedAction) ||
                     Mathf.Sign(next.AxisValue) != Mathf.Sign(motion.AxisValue)),
                _ => false
            };
            if (released)
                CancelCapture();
            return;
        }

        if (@event is InputEventKey { Pressed: true, Echo: false } keyEvent)
        {
            if (CapturingController)
            {
                if (keyEvent.Keycode == Key.Escape)
                    CancelCapture();
                else if (keyEvent.Keycode == Key.Delete)
                {
                    SetControllerBinding(null);
                    CancelCapture();
                }
            }
            else if (keyEvent.Keycode != Key.Unknown)
            {
                SetKey(keyEvent.Keycode);
                InputToRelease = @event;
            }
        }
        else if (CapturingController && (@event is InputEventJoypadButton { Pressed: true } ||
                 @event is InputEventJoypadMotion motion && Mathf.Abs(motion.AxisValue) >= 0.75f))
        {
            SetControllerBinding(@event);
            InputToRelease = @event;
        }
    }

    private void UpdateKeybind()
    {
        foreach (InputEvent ev in InputMap.ActionGetEvents(AssociatedAction))
        {
            if (ev is InputEventKey)
                InputMap.ActionEraseEvent(AssociatedAction, ev);
        }
        InputMap.ActionAddEvent(AssociatedAction, new InputEventKey
        {
            Device = -1,
            Keycode = CurrentKey
        });
    }

    public void SetKey(Key key)
    {
        CurrentKey = key;
        KeyButton.Text = OS.GetKeycodeString(CurrentKey);
        UpdateKeybind();
    }

    public void Reset()
    {
        CancelCapture();
        SetKey(DefaultKey);
        SetControllerBinding(DefaultControllerBinding);
    }

    public void SetControllerBinding(InputEvent binding)
    {
        CurrentControllerBinding = ControllerBinding.Normalize(binding);
        ControllerBinding.UpdateAction(AssociatedAction, CurrentControllerBinding);
        RefreshLabels();
    }

    public void LoadControllerBinding(Variant value)
    {
        SetControllerBinding(ControllerBinding.TryDeserialize(value, out InputEvent binding) ? binding : DefaultControllerBinding);
    }

    public string GetBindingDisplayName()
    {
        string key = OS.GetKeycodeString(CurrentKey);
        return CurrentControllerBinding == null ? key : $"{key} / {ControllerBinding.DisplayName(CurrentControllerBinding)}";
    }

    private void RefreshLabels()
    {
        if (KeyButton != null)
            KeyButton.Text = OS.GetKeycodeString(CurrentKey);
        if (ControllerButton != null)
            ControllerButton.Text = ControllerBinding.DisplayName(CurrentControllerBinding);
    }
}
