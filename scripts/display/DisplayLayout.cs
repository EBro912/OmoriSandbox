using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using OmoriSandbox.Animation;

namespace OmoriSandbox;

internal partial class DisplayLayout : Node
{
    internal const int LogicalHeight = 480;
    internal const int MinWidth = 640;
    internal const int MaxWidth = 1120;
    private static readonly float[] FractionalScales = [1.5f, 2.25f];

    internal static DisplayLayout Instance { get; private set; }
    internal int Width { get; private set; } = MinWidth;
    internal float Scale { get; private set; } = 1f;
    internal Vector2 Margin { get; private set; }
    internal bool EdgePortraits { get; private set; }
    internal int CanvasWidth { get; private set; }
    internal bool IntegerScaling { get; private set; }
    internal float OffsetX => (Width - 640) / 2f;
    internal Vector2 LayerOffset => Margin + new Vector2(OffsetX, 0);
    internal Rect2 VisibleBattleRect => new(-OffsetX, 0, Width, LogicalHeight);
    
    internal int ExtraLayerCount => ExtraLayers.Count;

    [Export] private CanvasLayer[] Layers;
    [Export] private Node Party;
    [Export] private Control[] AddActorControls;
    [Export] private BattlebackDisplayComponent[] Battlebacks;
    [Export] private BattlebackLayers BattlebackLayers;
    [Export] private ColorRect Backdrop;
    [Export] private ColorRect ScreenTint;
    [Export] private SubViewport PerfectheartViewport;
    [Export] private Node2D PerfectheartOverlayParent;
    [Export] private Sprite2D PerfectheartOverlay;
    [Export] private Sprite2D PerfectheartOverlayDarken;
    [Export] private Control StatusBar;
    [Export] private ColorRect[] Letterbox;

    private readonly List<CanvasLayer> ExtraLayers = [];
    private float StatusBarLeft;
    private float StatusBarRight;
    // window size the layout was last computed for
    private Vector2I WindowSize;
    private Vector2 VisibleSize;

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
    }

    // picks the canvas width and the largest scale step at which it fits the window
    internal bool Refresh()
    {
        Window window = GetWindow();
        Vector2I size = window.Size;
        WindowSize = size;
        if (size.X <= 0 || size.Y <= 0)
            return false;
        int width;
        float scale;
        if (CanvasWidth > 0)
        {
            // a chosen canvas keeps its width whatever the window shows (never wider than the window)
            width = Math.Min(CanvasWidth, size.X);
            width -= width % 2;
            scale = FitScale(width, size);
        }
        else
        {
            // the largest scale at which 640x480 fits, then as much width as the window shows at it
            scale = FitScale(MinWidth, size);
            width = Math.Clamp((int)(size.X / scale), MinWidth, MaxWidth);
            // even widths keep the centring offset whole
            width -= width % 2;
        }
        while (width * scale % 1f != 0f)
            width -= 2;
        Vector2 shown = new(width * scale, LogicalHeight * scale);
        Vector2 margin = ApplyStretch(window, size, width, scale, shown);
        Vector2 visible = window.GetVisibleRect().Size;
        bool resized = width != Width;
        // the scales checked here do not suffer from floating point issues
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (!resized && scale == Scale && margin == Margin && visible == VisibleSize)
            return false;
        Width = width;
        Scale = scale;
        Margin = margin;
        VisibleSize = visible;
        if (!resized)
        {
            PlaceLayers();
            return false;
        }
        Apply();
        return true;
    }

    private static Vector2 ApplyStretch(Window window, Vector2I size, int width, float scale, Vector2 shown)
    {
        bool whole = scale % 1f == 0f;
        if (whole && (shown.X < size.X || shown.Y < size.Y) && (size.Y - (int)shown.Y) % 2 == 0)
        {
            SetStretch(window, Window.ContentScaleModeEnum.CanvasItems, Window.ContentScaleAspectEnum.Ignore, Window.ContentScaleStretchEnum.Integer, new Vector2I(width, LogicalHeight), 1f);
            if (IsExactScale(window, scale))
                return Vector2.Zero;
        }
        else if (!whole && shown.X < size.X)
        {
            SetStretch(window, Window.ContentScaleModeEnum.CanvasItems, Window.ContentScaleAspectEnum.Keep, Window.ContentScaleStretchEnum.Fractional, new Vector2I((int)shown.X, size.Y), scale);
            if (IsExactScale(window, scale))
                return new Vector2(0, Mathf.Floor((size.Y - shown.Y) / 2f) / scale);
        }
        SetStretch(window, Window.ContentScaleModeEnum.Disabled, Window.ContentScaleAspectEnum.Keep, Window.ContentScaleStretchEnum.Fractional, new Vector2I(MinWidth, LogicalHeight), scale);
        return ((new Vector2(size.X, size.Y) - shown) / 2f).Floor().Max(Vector2.Zero) / scale;
    }

    private static void SetStretch(Window window, Window.ContentScaleModeEnum mode, Window.ContentScaleAspectEnum aspect, Window.ContentScaleStretchEnum stretch, Vector2I contentSize, float factor)
    {
        if (window.ContentScaleFactor > factor)
            window.ContentScaleFactor = factor;
        bool settled = window.ContentScaleStretch == stretch && window.ContentScaleAspect == aspect && window.ContentScaleSize == contentSize;
        // avoid unnecessary viewport updates by checking these individually
        if (!settled && window.ContentScaleMode != Window.ContentScaleModeEnum.Disabled)
            window.ContentScaleMode = Window.ContentScaleModeEnum.Disabled;
        if (window.ContentScaleStretch != stretch)
            window.ContentScaleStretch = stretch;
        if (window.ContentScaleAspect != aspect)
            window.ContentScaleAspect = aspect;
        if (window.ContentScaleSize != contentSize)
            window.ContentScaleSize = contentSize;
        // the scales checked here do not suffer from floating point issues
        // at worst one redundant refresh
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (window.ContentScaleFactor != factor)
            window.ContentScaleFactor = factor;
        if (window.ContentScaleMode != mode)
            window.ContentScaleMode = mode;
    }

    private static bool IsExactScale(Window window, float scale)
    {
        Vector2 actual = window.GetFinalTransform().Scale;
        return Mathf.Abs(actual.X - scale) < 0.0001f && Mathf.Abs(actual.Y - scale) < 0.0001f;
    }

    private float FitScale(int width, Vector2I size)
    {
        float scale = Math.Max(1, Math.Min(size.X / width, size.Y / LogicalHeight));
        if (IntegerScaling)
            return scale;
        foreach (float step in FractionalScales)
        {
            if (step > scale && width * step <= size.X && LogicalHeight * step <= size.Y)
                scale = step;
        }
        return scale;
    }

    public void SetCanvasWidth(int width)
    {
        CanvasWidth = Math.Clamp(width, 0, MaxWidth);
        if (IsNodeReady())
            Refresh();
    }

    public void SetIntegerScaling(bool enabled)
    {
        IntegerScaling = enabled;
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

    internal void RegisterLayer(CanvasLayer layer)
    {
        if (!IsInstanceValid(layer) || ExtraLayers.Contains(layer))
            return;
        ExtraLayers.Add(layer);
        layer.TreeExiting += () => ExtraLayers.Remove(layer);
        layer.Offset = LayerOffset;
    }

    private void PlaceLayers()
    {
        if (!IsNodeReady())
            return;

        foreach (CanvasLayer layer in Layers)
            layer.Offset = LayerOffset;

        ExtraLayers.RemoveAll(layer => !IsInstanceValid(layer));
        foreach (CanvasLayer layer in ExtraLayers)
            layer.Offset = LayerOffset;

        Vector2 visible = GetWindow().GetVisibleRect().Size;
        float right = Margin.X + Width;
        float bottom = Margin.Y + LogicalHeight;
        Fill(Letterbox[0], new Rect2(0, 0, visible.X, Margin.Y));
        Fill(Letterbox[1], new Rect2(0, bottom, visible.X, visible.Y - bottom));
        Fill(Letterbox[2], new Rect2(0, Margin.Y, Margin.X, LogicalHeight));
        Fill(Letterbox[3], new Rect2(right, Margin.Y, visible.X - right, LogicalHeight));
    }

    private void Apply()
    {
        if (!IsNodeReady())
            return;

        PlaceLayers();

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
        BattlebackLayers.ConfigureDisplay(VisibleBattleRect);

        Rect2 coverage = VisibleBattleRect.Grow(32);
        Fill(Backdrop, coverage);
        ColorRect photograph = AnimationManager.Instance?.LivePhotograph;
        if (photograph != null)
            Fill(photograph, coverage);
        Fill(ScreenTint, coverage);

        PerfectheartViewport.Size = new Vector2I(Width, LogicalHeight);
        PerfectheartOverlayParent.Position = new Vector2(OffsetX, 0);
        PerfectheartOverlay.Position = new Vector2(-OffsetX, 0);
        PerfectheartOverlayDarken.Position = new Vector2(-OffsetX, 0);

        StatusBar.OffsetLeft = StatusBarLeft - OffsetX;
        StatusBar.OffsetRight = StatusBarRight - OffsetX;
    }

    internal void FillCoverage(Control control)
    {
        Fill(control, VisibleBattleRect.Grow(32));
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
