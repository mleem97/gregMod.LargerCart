using System;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MelonLoader;
using UnityEngine;

namespace greg.Mods.LargerCart.Interop;

/// <summary>
/// Extends internal trolley arrays to configured capacity.
/// Typed directly against Il2Cpp dummy DLL (no TypeByName).
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
                MelonLogger.Warning("[LargerCart] positionsOnTrolley/usedPositions is null, skipping.");
                return;
            }

            if (positions.Count >= targetCount && used.Count >= targetCount)
                return;

            MelonLogger.Msg($"[LargerCart] Extending trolley: {positions.Count}/{used.Count} -> {targetCount}");

            trolley.positionsOnTrolley = ExpandPositions(positions, targetCount);
            trolley.usedPositions = ExpandUsed(used, targetCount);
        }
        catch (Exception ex)
        {
            MelonLogger.Warning($"[LargerCart] Extension failed: {ex.GetBaseException().Message}");
        }
    }

    private static Il2CppReferenceArray<Transform> ExpandPositions(
        Il2CppReferenceArray<Transform> oldArray, int targetLen)
    {
        var fresh = new Il2CppReferenceArray<Transform>(targetLen);
        int copyLen = Math.Min(oldArray.Count, targetLen);

        for (int i = 0; i < copyLen; i++)
            fresh[i] = oldArray[i];

        // Same as before: new slots share the last position,
        // until game fills them.
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
