using System;
using HarmonyLib;
using MelonLoader;
using UnityEngine.InputSystem;
using greg.Mods.LargerCart.Config;
using greg.Mods.LargerCart.Patches;

[assembly: MelonInfo(typeof(greg.Mods.LargerCart.LargerCartMod), "gregMod.LargerCart", "2.1.0", "teamGregModding / mleem97 & BigTexasJerky")]
[assembly: MelonGame("Waseku", "Data Center")]

namespace greg.Mods.LargerCart;

public class LargerCartMod : MelonMod
{
    private const string HarmonyId = "com.gregmod.largercart";
    private const string LegacyHarmonyId = "bigtexasjerky.datacenter.texassizedtrolley";

    private static Key _tableKey = Key.T;
    private static float _tableHeight = 0.35f;
    private static global::Il2Cpp.TrolleyLoadingBay _lastBay;

    public override void OnInitializeMelon()
    {
        var config = LargerCartConfig.Load();
        TrolleyLoadingBayPatch.TargetCount = config.TargetPositionCount;
        TrolleyLoadingBayPatch.TableHeight = Math.Max(0.1f, config.TableHeightAboveTop);
        CartStabilizer.Enabled = config.StabilizeCart;
        CartStabilizer.MassMultiplier = config.CartMassMultiplier;
        CartStabilizer.AngularDragMultiplier = config.CartAngularDragMultiplier;
        CartTable.Enabled = config.TableEnabled;
        CartTable.TraySlots = config.TableTraySlots;
        _tableHeight = Math.Max(0.1f, config.TableHeightAboveTop);
        _tableKey = ParseKey(config.TableToggleKey, Key.T);
        MelonLogger.Msg($"[LargerCart] v2.1.0 loaded. TargetPositionCount = {config.TargetPositionCount}, " +
                        $"StabilizeCart = {config.StabilizeCart} " +
                        $"(mass x{config.CartMassMultiplier:0.#}, angularDrag x{config.CartAngularDragMultiplier:0.#}), " +
                        $"Table = {config.TableEnabled} (key {_tableKey}).");

        try
        {
            HarmonyLib.Harmony.UnpatchID(LegacyHarmonyId);
        }
        catch (Exception ex)
        {
            MelonLogger.Warning($"[LargerCart] Legacy unpatch failed: {ex.Message}");
        }

        try
        {
            var harmony = new HarmonyLib.Harmony(HarmonyId);
            harmony.PatchAll(typeof(TrolleyLoadingBayPatch).Assembly);
            MelonLogger.Msg("[LargerCart] Il2Cpp.TrolleyLoadingBay.Start() patched.");
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"[LargerCart] Patch failed: {ex.GetBaseException().Message}");
        }
    }

    internal static void NoteBay(global::Il2Cpp.TrolleyLoadingBay bay)
    {
        _lastBay = bay;
    }

    public override void OnUpdate()
    {
        // Rebuild table if trolley missing at bay start.
        if (CartTable.Enabled && _lastBay != null)
        {
            try
            {
                CartTable.TryEnsure(_lastBay, _tableHeight);
                CartTable.EnsureTraySlots(_lastBay);
            }
            catch { /* best-effort */ }
        }
        // Table key (only if no pause/system menu open — cheap
        // name check as usual, no hard hook).
        if (!CartTable.Enabled) return;
        try
        {
            var kb = Keyboard.current;
            if (kb == null) return;
            var ctrl = kb[_tableKey];
            if (ctrl != null && ctrl.wasPressedThisFrame)
                CartTable.Toggle();
        }
        catch { /* best-effort */ }
    }

    private static Key ParseKey(string raw, Key fallback)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(raw) &&
                Enum.TryParse<Key>(raw.Trim(), true, out var k) && k != Key.None)
                return k;
            MelonLogger.Warning($"[LargerCart] Unknown TableToggleKey '{raw}', using {fallback}.");
        }
        catch { }
        return fallback;
    }
}
