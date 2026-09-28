using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using GK2RustyToolDisposal.Configuration;
using HarmonyLib;

namespace GK2RustyToolDisposal;

[BepInPlugin(Guid, Name, Version)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string Guid = "de.w00dst0ckOo.gk2.rustytooldisposal";
    public const string Name = "Rusty Tool Disposal";
    public const string Version = "0.2.1";

    private static readonly Dictionary<string, string> DebugStates = new Dictionary<string, string>(
        StringComparer.Ordinal
    );

    private Harmony harmony;

    internal static bool Ready { get; private set; }

    internal static ManualLogSource Log { get; private set; }

    internal static bool DebugEnabled =>
        ModConfig.DebugLogging != null && ModConfig.DebugLogging.Value;

    // Logs only when the message for this key changed.
    // Call only after checking DebugEnabled.
    internal static void DebugState(string key, string message)
    {
        if (DebugStates.TryGetValue(key, out var previous) && previous == message)
        {
            return;
        }

        DebugStates[key] = message;

        Log?.LogInfo("[Debug] " + message);
    }

    private void Awake()
    {
        Log = Logger;

        var legacy = ConfigMigration.ReadLegacy(Config.ConfigFilePath);

        ModConfig.Initialize(Config, legacy);

        if (DebugEnabled)
        {
            Logger.LogInfo("Debug logging enabled.");
        }

        if (legacy.Found)
        {
            ConfigMigration.RemoveLegacySections(Config.ConfigFilePath);
        }

        try
        {
            harmony = new Harmony(Guid);

            harmony.PatchAll();

            Ready = true;

            Logger.LogInfo($"{Name} {Version} loaded.");
        }
        catch (Exception ex)
        {
            Ready = false;

            harmony?.UnpatchSelf();

            Logger.LogError(
                "Could not install discard patches; " + "vanilla protection retained. " + ex
            );
        }
    }

    private void OnDestroy()
    {
        Ready = false;

        harmony?.UnpatchSelf();

        DebugStates.Clear();
    }
}
