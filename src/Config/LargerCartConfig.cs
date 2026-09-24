using System;
using System.IO;
using System.Text.Json;
using MelonLoader;

namespace greg.Mods.LargerCart.Config;

/// <summary>
/// JSON-based config. Stays deliberately standalone (no gregCore,
/// no MelonPreferences), so the mod runs without framework.
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

    /// <summary>Master switch: heavier/sluggish trolley so loaded
    /// cart stays put.</summary>
    public bool StabilizeCart { get; set; } = true;

    /// <summary>Rigidbody mass multiplier (1 = vanilla).</summary>
    public float CartMassMultiplier { get; set; } = 2f;

    /// <summary>AngularDrag multiplier against tipping (1 = vanilla).</summary>
    public float CartAngularDragMultiplier { get; set; } = 4f;

    /// <summary>Build folding table on trolley (toggle via key).</summary>
    public bool TableEnabled { get; set; } = true;

    /// <summary>Key to open/close (e.g. "T").</summary>
    public string TableToggleKey { get; set; } = "T";

    /// <summary>Plate height above trolley top edge, meters.</summary>
    public float TableHeightAboveTop { get; set; } = 0.35f;

    /// <summary>4x3 tray grid (module boxes) at table height as slots.</summary>
    public bool TableTraySlots { get; set; } = true;

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
                MelonLogger.Msg($"[LargerCart] Default config created: {path}");
                return fresh;
            }

            var config = JsonSerializer.Deserialize<LargerCartConfig>(File.ReadAllText(path));
            if (config == null)
                throw new InvalidDataException("Config is empty.");

            return config.WithValidatedCounts(path);
        }
        catch (Exception ex)
        {
            MelonLogger.Warning($"[LargerCart] Config error, using defaults: {ex.Message}");
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
            MelonLogger.Msg($"[LargerCart] Config migrated from {legacyPath}.");
            return migrated;
        }
        catch (Exception ex)
        {
            MelonLogger.Warning($"[LargerCart] Migration failed: {ex.Message}");
            return null;
        }
    }

    private LargerCartConfig WithValidatedCounts(string path)
    {
        bool dirty = false;
        int clamped = Math.Clamp(TargetPositionCount, MinPositionCount, MaxPositionCount);
        if (clamped != TargetPositionCount)
        {
            MelonLogger.Warning(
                $"[LargerCart] TargetPositionCount {TargetPositionCount} out of range " +
                $"[{MinPositionCount},{MaxPositionCount}], using {clamped}.");
            TargetPositionCount = clamped;
            dirty = true;
        }
        float mass = Math.Clamp(CartMassMultiplier, 1f, 50f);
        if (mass != CartMassMultiplier)
        {
            MelonLogger.Warning($"[LargerCart] CartMassMultiplier out of range [1,50], using {mass}.");
            CartMassMultiplier = mass;
            dirty = true;
        }
        float ang = Math.Clamp(CartAngularDragMultiplier, 1f, 100f);
        if (ang != CartAngularDragMultiplier)
        {
            MelonLogger.Warning($"[LargerCart] CartAngularDragMultiplier out of range [1,100], using {ang}.");
            CartAngularDragMultiplier = ang;
            dirty = true;
        }
        if (dirty) Save(path, this);
        return this;
    }

    private static void Save(string path, LargerCartConfig config)
    {
        string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
    }
}
