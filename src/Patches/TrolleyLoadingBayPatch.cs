using System;
using HarmonyLib;
using MelonLoader;
using greg.Mods.LargerCart.Interop;

namespace greg.Mods.LargerCart.Patches;

/// <summary>
/// Extends trolley arrays right after game start.
/// </summary>
[HarmonyPatch(typeof(global::Il2Cpp.TrolleyLoadingBay), nameof(global::Il2Cpp.TrolleyLoadingBay.Start))]
internal static class TrolleyLoadingBayPatch
{
    internal static int TargetCount = 240;
    internal static float TableHeight = 0.35f;

    [HarmonyPostfix]
    private static void Postfix(global::Il2Cpp.TrolleyLoadingBay __instance)
    {
        try
        {
            if (__instance == null)
                return;
            TrolleyArray.EnsureCapacity(__instance, TargetCount);
            CartStabilizer.Apply(__instance);
            try
            {
                LargerCartMod.NoteBay(__instance);
                CartTable.TryEnsure(__instance, TableHeight);
                CartTable.EnsureTraySlots(__instance);
            }
            catch { /* table build best-effort */ }
        }
        catch (Exception ex)
        {
            MelonLogger.Warning($"[LargerCart] Postfix error: {ex.GetBaseException().Message}");
        }
    }
}
