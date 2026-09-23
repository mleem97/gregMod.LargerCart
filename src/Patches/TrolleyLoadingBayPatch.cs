using System;
using HarmonyLib;
using MelonLoader;
using greg.Mods.LargerCart.Interop;

namespace greg.Mods.LargerCart.Patches;

/// <summary>
/// Erweitert die Trolley-Arrays direkt nach dem Spiel-Start.
/// </summary>
[HarmonyPatch(typeof(global::Il2Cpp.TrolleyLoadingBay), nameof(global::Il2Cpp.TrolleyLoadingBay.Start))]
internal static class TrolleyLoadingBayPatch
{
    internal static int TargetCount = 240;

    [HarmonyPostfix]
    private static void Postfix(global::Il2Cpp.TrolleyLoadingBay __instance)
    {
        try
        {
            if (__instance == null)
                return;
            TrolleyArray.EnsureCapacity(__instance, TargetCount);
            CartStabilizer.Apply(__instance);
        }
        catch (Exception ex)
        {
            MelonLogger.Warning($"[LargerCart] Postfix-Fehler: {ex.GetBaseException().Message}");
        }
    }
}
