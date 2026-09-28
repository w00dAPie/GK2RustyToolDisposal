using System.Collections.Generic;
using System.IO;

namespace GK2RustyToolDisposal.Configuration;

internal sealed class LegacyConfig
{
    internal bool Found;
    internal bool DebugLogging;
}

internal static class ConfigMigration
{
    private const string LegacyDebugSection = "Debug";
    private const string LegacyDebugKey = "EnableDebugLogging";

    internal static LegacyConfig ReadLegacy(string path)
    {
        var result = new LegacyConfig();
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            return result;

        try
        {
            var section = string.Empty;
            foreach (var rawLine in File.ReadAllLines(path))
            {
                var line = rawLine.Trim();
                if (line.Length == 0 || line.StartsWith("#"))
                    continue;

                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    section = line.Substring(1, line.Length - 2);
                    continue;
                }

                var separator = line.IndexOf('=');
                if (separator < 0)
                    continue;

                var key = line.Substring(0, separator).Trim();
                var value = line.Substring(separator + 1).Trim();

                if (
                    section == LegacyDebugSection
                    && key == LegacyDebugKey
                    && bool.TryParse(value, out var debug)
                )
                {
                    result.DebugLogging = debug;
                    result.Found = true;
                }
            }
        }
        catch
        {
            // Unreadable legacy file: ignore it and use defaults.
            return new LegacyConfig();
        }
        return result;
    }

    internal static void RemoveLegacySections(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            return;

        try
        {
            var output = new List<string>();
            var section = string.Empty;
            foreach (var rawLine in File.ReadAllLines(path))
            {
                var trimmed = rawLine.Trim();
                if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                    section = trimmed.Substring(1, trimmed.Length - 2);

                if (section == LegacyDebugSection)
                    continue;
                output.Add(rawLine);
            }
            File.WriteAllLines(path, output);
        }
        catch
        {
            // Leaving the old section in place is harmless.
        }
    }
}