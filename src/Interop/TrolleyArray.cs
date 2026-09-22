using System;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MelonLoader;
using UnityEngine;

namespace greg.Mods.LargerCart.Interop;

/// <summary>
/// Erweitert die internen Trolley-Arrays auf die konfigurierte Kapazitaet.
/// Direkt typisiert gegen die Il2Cpp-Dummy-DLL (kein TypeByName).
/// </summary>
internal static class TrolleyArray
{
    internal static void EnsureCapacity(global::Il2Cpp.TrolleyLoadingBay trolley, int targetCount)
    {
        if (trolley == null || trolley.Pointer == IntPtr.Zero)
            return;

        try
        {
            var positions = trolley.positionsOnTrolley;
            var used = trolley.usedPositions;

            if (positions == null || used == null)
            {
                MelonLogger.Warning("[LargerCart] positionsOnTrolley/usedPositions ist null, ueberspringe.");
                return;
            }

            if (positions.Count >= targetCount && used.Count >= targetCount)
                return;

            MelonLogger.Msg($"[LargerCart] Erweitere Trolley: {positions.Count}/{used.Count} -> {targetCount}");

            trolley.positionsOnTrolley = ExpandPositions(positions, targetCount);
            trolley.usedPositions = ExpandUsed(used, targetCount);
        }
        catch (Exception ex)
        {
            MelonLogger.Warning($"[LargerCart] Erweiterung fehlgeschlagen: {ex.GetBaseException().Message}");
        }
    }

    private static Il2CppReferenceArray<Transform> ExpandPositions(
        Il2CppReferenceArray<Transform> oldArray, int targetLen)
    {
        var fresh = new Il2CppReferenceArray<Transform>(targetLen);
        int copyLen = Math.Min(oldArray.Count, targetLen);

        for (int i = 0; i < copyLen; i++)
            fresh[i] = oldArray[i];

        // Verhalten wie bisher: neue Slots teilen sich die letzte Position,
        // bis das Spiel sie belegt.
        Transform last = copyLen > 0 ? oldArray[copyLen - 1] : null;
        for (int i = copyLen; i < targetLen; i++)
            fresh[i] = last;

        return fresh;
    }

    private static Il2CppStructArray<int> ExpandUsed(Il2CppStructArray<int> oldArray, int targetLen)
    {
        var fresh = new Il2CppStructArray<int>(targetLen);
        int copyLen = Math.Min(oldArray.Count, targetLen);

        for (int i = 0; i < copyLen; i++)
            fresh[i] = oldArray[i];

        for (int i = copyLen; i < targetLen; i++)
            fresh[i] = 0;

        return fresh;
    }
}
