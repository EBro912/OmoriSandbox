using System.Globalization;
using System.Linq;
using Godot;

namespace OmoriSandbox.Editor;

internal partial class StatCurveGenerator : Control
{
	[Export] private OptionButton StatSelector;
	[Export] private SpinBox LevelsSpinBox;
	[Export] private SpinBox FirstSpinBox;
	[Export] private Label LastLabel;
	[Export] private SpinBox LastSpinBox;
	[Export] private HSlider GrowthSlider;
	[Export] private Label GrowthValueLabel;
	[Export] private Control Preview;
	[Export] private TextEdit Output;
	[Export] private Button CopyButton;

	private static readonly int[] Limits = [9999, 9999, 999, 999, 999];

	// default values for each stat
	private readonly int[][] Dials =
	[
		[34, 376, 0],
		[19, 241, -10],
		[5, 86, 4],
		[5, 76, 4],
		[4, 85, 4],
	];

	private int CurrentStat;
	private bool Loading;
	private int[] Values = [];

	public override void _Ready()
	{
		FirstSpinBox.ValueChanged += value => SetDial(0, (int)value);
		LastSpinBox.ValueChanged += value => SetDial(1, (int)value);
		GrowthSlider.ValueChanged += value => SetDial(2, (int)value);
		LevelsSpinBox.ValueChanged += _ => Refresh();
		StatSelector.ItemSelected += index => LoadStat((int)index);
		Preview.Draw += DrawPreview;
		CopyButton.Pressed += () => DisplayServer.ClipboardSet(Output.Text);
		LoadStat(0);
	}

	private void SetDial(int dial, int value)
	{
		if (Loading)
			return;
		Dials[CurrentStat][dial] = value;
		Refresh();
	}

	private void LoadStat(int stat)
	{
		CurrentStat = stat;
		int[] dials = Dials[stat];
		Loading = true;
		FirstSpinBox.MaxValue = Limits[stat];
		LastSpinBox.MaxValue = Limits[stat];
		FirstSpinBox.Value = dials[0];
		LastSpinBox.Value = dials[1];
		GrowthSlider.Value = dials[2];
		Loading = false;
		Refresh();
	}

	private void Refresh()
	{
		int[] dials = Dials[CurrentStat];
		int levelCount = (int)LevelsSpinBox.Value;
		LastLabel.Text = $"Level {levelCount}";
		GrowthValueLabel.Text = (dials[2] / 10.0).ToString("0.0", CultureInfo.InvariantCulture);
		Values = Generate(dials[0], dials[1], dials[2], levelCount);
		Output.Text = "[" + string.Join(", ", Values) + "]";
		Preview.QueueRedraw();
	}

	private void DrawPreview()
	{
		Vector2[] points = PreviewPoints(Values, Preview.Size);
		if (points.Length >= 2)
			Preview.DrawPolyline(points, Colors.White, 2);
	}

	internal static int[] Generate(int first, int last, int growthTenths, int levelCount)
	{
		if (levelCount == 1)
			return [first];

		int n = levelCount - 1;
		int[] values = new int[levelCount];
		for (int x = 0; x <= n; x++)
			values[x] = first + CeilDiv((last - first) * ((10 - growthTenths) * x * n + growthTenths * x * x), 10 * n * n);
		return values;
	}

	internal static Vector2[] PreviewPoints(int[] values, Vector2 size)
	{
		int count = values.Length;
		int min = values.Min();
		int max = values.Max();
		Vector2[] points = new Vector2[count];
		for (int i = 0; i < count; i++)
		{
			float x = count == 1 ? 0 : i * size.X / (count - 1);
			float y = max == min ? size.Y / 2 : size.Y - (values[i] - min) * size.Y / (max - min);
			points[i] = new Vector2(x, y);
		}
		return points;
	}

	private static int CeilDiv(int a, int b) => a >= 0 ? (a + b - 1) / b : a / b;
}
