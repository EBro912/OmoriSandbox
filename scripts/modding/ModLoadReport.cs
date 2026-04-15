using Godot;
using System.Collections.Generic;
using System.Text;

namespace OmoriSandbox.Modding;

internal sealed class ModLoadReport
{
	public string ModName { get; }
	public string ModVersion { get; }
	private readonly List<string> Errors = [];
	private readonly List<string> Warnings = [];
	public int Loaded { get; private set; }
	public int Skipped { get; private set; }

	public ModLoadReport(string modName, string modVersion)
	{
		ModName = modName;
		ModVersion = modVersion;
	}

	public bool HasErrors => Errors.Count > 0;

	public void Error(string category, string item, string message)
	{
		string formatted = $"[{ModName}] {category}/{item}: {message}";
		Errors.Add(formatted);
		GD.PushError(formatted);
	}

	public void Warn(string category, string item, string message)
	{
		string formatted = $"[{ModName}] {category}/{item}: {message}";
		Warnings.Add(formatted);
		GD.PushWarning(formatted);
	}

	public void CountLoaded() => Loaded++;
	public void CountSkipped() => Skipped++;

	public void PrintSummary()
	{
		StringBuilder sb = new();
		sb.AppendLine($"--- Mod \"{ModName} (v{ModVersion})\" ---");
		sb.AppendLine($"  Loaded: {Loaded} assets | Skipped: {Skipped} assets");
		
		if (Errors.Count > 0)
		{
			sb.AppendLine("  Errors:");
			foreach (string error in Errors)
				sb.AppendLine($"    {error}");
		}

		if (Warnings.Count > 0)
		{
			sb.AppendLine("  Warnings:");
			foreach (string warning in Warnings)
				sb.AppendLine($"    {warning}");
		}

		GD.Print(sb.ToString().TrimEnd());
	}
}
