using BepInEx.Configuration;

namespace GK2RustyToolDisposal.Configuration;

internal static class ModConfig
{
    private const string SectionDebug = "90 - Debug";

    private static ConfigFile configFile;

    internal static ConfigEntry<bool> DebugLogging { get; private set; }

    internal static void Initialize(ConfigFile config, LegacyConfig legacy)
    {
        configFile = config;

        DebugLogging = config.Bind(
            SectionDebug,
            "EnableDebugLogging",
            false,
            "Enables detailed diagnostic logging explaining why rusty equipment is protected or allowed."
        );

        if (legacy == null || !legacy.Found)
            return;

        DebugLogging.Value = legacy.DebugLogging;
        SaveIfAutoSaveDisabled();
    }

    internal static void SaveIfAutoSaveDisabled()
    {
        if (configFile != null && !configFile.SaveOnConfigSet)
            configFile.Save();
    }
}
