namespace OmoriSandbox.Animation;

internal sealed class RPGMakerAnimation
{
    public int Id { get; set; }
    public int Animation1Hue { get; set; }
    public string Animation1Name { get; set; }
    public int Animation2Hue { get; set; }
    public string Animation2Name { get; set; }
    public float[][][] Frames { get; set; }
    public string Name { get; set; }
    public int Position { get; set; }
    public Timings[] Timings { get; set; }
}

internal sealed class Timings
{
    public int[] FlashColor { get; set; }
    public int FlashDuration { get; set; }
    public int FlashScope { get; set; }
    public int Frame { get; set; }
    public Se Se { get; set; }
}

internal sealed class Se
{
    public string Name { get; set; }
    public int Pan { get; set; }
    public int Pitch { get; set; }
    public int Volume { get; set; }
}

