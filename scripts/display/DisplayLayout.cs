using System;
using System.Linq;
using Godot;

namespace OmoriSandbox;

internal partial class DisplayLayout : Node
{
    internal const int LogicalHeight = 480;
    internal const int MinWidth = 640;
    internal const int MaxWidth = 1120;

    internal static DisplayLayout Instance { get; private set; }
    internal event Action WindowResized;
    internal int Width { get; private set; } = MinWidth;
    internal int Scale { get; private set; } = 1;
    internal bool EdgePortraits { get; private set; }
    internal int CanvasWidth { get; private set; }
    internal float OffsetX => (Width - 640) / 2f;
    internal Rect2 VisibleBattleRect => new(-OffsetX, 0, Width, LogicalHeight);
    
    [Export] private CanvasLayer[] Layers;
    [Export] private Node Party;
    [Export] private Control[] AddActorControls;
    [Export] private BattlebackDisplayComponent[] Battlebacks;
    [Export] private ColorRect Backdrop;
    [Export] private ColorRect Photograph;
    [Export] private ColorRect ScreenTint;
    [Export] private SubViewport PerfectheartViewport;
    [Export] private Node2D PerfectheartOverlayParent;
    [Export] private Sprite2D PerfectheartOverlay;
    [Export] private Sprite2D PerfectheartOverlayDarken;
    [Export] private Control StatusBar;
    [Export] private Parallax2D TitleParallax;

    private float StatusBarLeft;
    private float StatusBarRight;
    // window size the layout was last computed for
    private Vector2I WindowSize;

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        StatusBarLeft = StatusBar.OffsetLeft;
        StatusBarRight = StatusBar.OffsetRight;
        GetWindow().MinSize = new Vector2I(MinWidth, LogicalHeight);
        if (!Refresh())
            Apply();
    }
    
    public override void _Process(double delta)
    {
        if (GetWindow().Size == WindowSize)
            return;
        Refresh();
        WindowResized?.Invoke();
    }

    // picks the canvas width and the largest whole-number scale at which it fits the window
    internal bool Refresh()
    {
        Window window = GetWindow();
        Vector2I size = window.Size;
        WindowSize = size;
        if (size.X <= 0 || size.Y <= 0)
            return false;
        int width;
        if (CanvasWidth > 0)
        {
            // a chosen canvas keeps its width whatever the window shows (never wider than the window)
            width = Math.Min(CanvasWidth, size.X);
            width -= width % 2;
            Scale = Math.Max(1, Math.Min(size.X / width, size.Y / LogicalHeight));
        }
        else
        {
            // the largest scale at which 640x480 fits, then as much width as the window shows at it
            Scale = Math.Max(1, Math.Min(size.X / MinWidth, size.Y / LogicalHeight));
            width = Math.Clamp(size.X / Scale, MinWidth, MaxWidth);
            // even widths keep the centring offset whole
            width -= width % 2;
        }
        Vector2I canvas = new(width, LogicalHeight);
        if (window.ContentScaleSize != canvas)
            window.ContentScaleSize = canvas;
        if (width == Width)
            return false;
        Width = width;
        Apply();
        return true;
    }

    public void SetCanvasWidth(int width)
    {
        CanvasWidth = Math.Clamp(width, 0, MaxWidth);
        if (IsNodeReady())
            Refresh();
    }

    public void SetEdgePortraits(bool enabled)
    {
        EdgePortraits = enabled;
        Apply();
    }

    internal Vector2 PortraitPosition(int slot)
    {
        float x = slot < 2 ? 14 : 512;
        if (EdgePortraits)
            x += slot < 2 ? -OffsetX : OffsetX;
        return new Vector2(x, slot % 2 == 0 ? 305 : 5);
    }

    internal void PlacePortrait(Control card, int slot)
    {
        card.SetAnchorsPreset(Control.LayoutPreset.TopLeft);
        card.Position = PortraitPosition(slot);
    }

    private void Apply()
    {
        if (!IsNodeReady())
            return;

        foreach (CanvasLayer layer in Layers)
            layer.Offset = new Vector2(OffsetX, 0);

        foreach (Node child in Party.GetChildren())
        {
            if (child is not Control card || card.IsQueuedForDeletion())
                continue;
            PartyMemberComponent member = card.GetChildren().OfType<PartyMemberComponent>().FirstOrDefault();
            if (member == null)
                continue;
            PlacePortrait(card, member.Position);
            member.RefreshCenterPoint();
        }

        for (int slot = 0; slot < AddActorControls.Length; slot++)
            AddActorControls[slot].Position = PortraitPosition(slot);

        foreach (BattlebackDisplayComponent battleback in Battlebacks)
            battleback.ConfigureDisplay(VisibleBattleRect);

        Rect2 coverage = VisibleBattleRect.Grow(32);
        Fill(Backdrop, coverage);
        Fill(Photograph, coverage);
        Fill(ScreenTint, coverage);

        PerfectheartViewport.Size = new Vector2I(Width, LogicalHeight);
        PerfectheartOverlayParent.Position = new Vector2(OffsetX, 0);
        PerfectheartOverlay.Position = new Vector2(-OffsetX, 0);
        PerfectheartOverlayDarken.Position = new Vector2(-OffsetX, 0);

        StatusBar.OffsetLeft = StatusBarLeft - OffsetX;
        StatusBar.OffsetRight = StatusBarRight - OffsetX;
        TitleParallax.RepeatTimes = Math.Max(2, Mathf.CeilToInt(Width / 960f) + 1);
    }

    private static void Fill(Control control, Rect2 rect)
    {
        control.SetAnchorsPreset(Control.LayoutPreset.TopLeft);
        control.Position = rect.Position;
        control.Size = rect.Size;
        control.MouseFilter = Control.MouseFilterEnum.Ignore;
    }

    public override void _ExitTree()
    {
        if (Instance == this)
            Instance = null;
    }
}
