using System;
using System.IO;
using System.Text.Json;
using MelonLoader;

namespace greg.Mods.LargerCart.Config;

/// <summary>
/// JSON-basierte Konfiguration. Bleibt bewusst standalone (kein gregCore,
/// keine MelonPreferences), damit der Mod ohne Framework laeuft.
/// </summary>
internal sealed class LargerCartConfig
{
    public const int MinPositionCount = 26;
    public const int MaxPositionCount = 512;
    public const int DefaultPositionCount = 240;

    private const string NewFolderName = "gregMod.LargerCart";
    private const string LegacyFolderName = "TexasSizedTrolley";
    private const string ConfigFileName = "config.json";

    public int TargetPositionCount { get; set; } = DefaultPositionCount;

    internal static LargerCartConfig Load()
    {
        string dir = Path.Combine(MelonLoader.Utils.MelonEnvironment.ModsDirectory, NewFolderName);
        string path = Path.Combine(dir, ConfigFileName);

        try
        {
            Directory.CreateDirectory(dir);

            if (!File.Exists(path))
            {
                var migrated = TryMigrateLegacyConfig(path);
                if (migrated != null)
                    return migrated;

                var fresh = new LargerCartConfig();
                Save(path, fresh);
                MelonLogger.Msg($"[LargerCart] Default-Config erstellt: {path}");
                return fresh;
            }

            var config = JsonSerializer.Deserialize<LargerCartConfig>(File.ReadAllText(path));
            if (config == null)
                throw new InvalidDataException("Config ist leer.");

            return config.WithValidatedCounts(path);
        }
        catch (Exception ex)
        {
            MelonLogger.Warning($"[LargerCart] Config-Fehler, nutze Defaults: {ex.Message}");
            return new LargerCartConfig();
        }
    }

    private static LargerCartConfig TryMigrateLegacyConfig(string newPath)
    {
        try
        {
            string legacyPath = Path.Combine(
                MelonLoader.Utils.MelonEnvironment.ModsDirectory, LegacyFolderName, ConfigFileName);
            if (!File.Exists(legacyPath))
                return null;

            var legacy = JsonSerializer.Deserialize<LargerCartConfig>(File.ReadAllText(legacyPath));
            if (legacy == null)
                return null;

            var migrated = legacy.WithValidatedCounts(newPath);
            Save(newPath, migrated);
            MelonLogger.Msg($"[LargerCart] Config aus {legacyPath} migriert.");
            return migrated;
        }
        catch (Exception ex)
        {
            MelonLogger.Warning($"[LargerCart] Migration fehlgeschlagen: {ex.Message}");
            return null;
        }
    }

    private LargerCartConfig WithValidatedCounts(string path)
    {
        int clamped = Math.Clamp(TargetPositionCount, MinPositionCount, MaxPositionCount);
        if (clamped != TargetPositionCount)
        {
            MelonLogger.Warning(
                $"[LargerCart] TargetPositionCount {TargetPositionCount} ausserhalb " +
                $"[{MinPositionCount},{MaxPositionCount}], nutze {clamped}.");
            TargetPositionCount = clamped;
            Save(path, this);
        }
        return this;
    }

    private static void Save(string path, LargerCartConfig config)
    {
        string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
    }
}
