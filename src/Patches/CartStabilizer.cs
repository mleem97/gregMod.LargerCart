using System;
using System.Collections.Generic;
using MelonLoader;
using UnityEngine;

namespace greg.Mods.LargerCart.Patches;

/// <summary>
/// Makes trolley heavier and tip-resistant so loaded
/// cart stays put. Hooks the trolley Rigidbody
/// (via TrolleyLoadingBay.carController, fallback: name search).
/// Multipliers instead of absolutes: vanilla base stays reference.
/// Once per scene per Rigidbody (pointer set against double apply).
/// </summary>
internal static class CartStabilizer
{
    internal static bool Enabled = true;
    internal static float MassMultiplier = 2f;
    internal static float AngularDragMultiplier = 4f;

    private static readonly HashSet<IntPtr> Stabilized = new HashSet<IntPtr>();

    internal static void Apply(global::Il2Cpp.TrolleyLoadingBay bay)
    {
        try
        {
            if (!Enabled) return;
            if (Stabilized.Count > 64) Stabilized.Clear();

            Rigidbody body = null;
            string how = "?";
            try
            {
                var controller = bay != null ? bay.carController : null;
                var go = controller != null ? controller.gameObject : null;
                if (go != null)
                {
                    body = go.GetComponent<Rigidbody>();
                    if (body != null) how = $"carController '{go.name}'";
                }
            }
            catch { /* fallback below */ }

            if (body == null)
            {
                try
                {
                    foreach (var rb in UnityEngine.Object.FindObjectsOfType<Rigidbody>())
                    {
                        if (rb == null || rb.gameObject == null) continue;
                        string n = rb.gameObject.name ?? "";
                        if (n.IndexOf("trolley", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            n.IndexOf("trolly", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            n.IndexOf("cart", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            body = rb;
                            how = $"name search '{n}'";
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Warning($"[LargerCart] Trolley search failed: {ex.Message}");
                    return;
                }
            }

            if (body == null)
            {
                MelonLogger.Warning("[LargerCart] No trolley Rigidbody found (stabilization skipped).");
                return;
            }

            IntPtr ptr = IntPtr.Zero;
            try { ptr = body.Pointer; } catch { return; }
            if (ptr == IntPtr.Zero || !Stabilized.Add(ptr)) return;

            float massBefore = 1f, angBefore = 0f;
            try { massBefore = body.mass; } catch { }
            try { angBefore = body.angularDrag; } catch { }

            try { body.mass = Math.Max(0.1f, massBefore * MassMultiplier); } catch (Exception ex)
            {
                MelonLogger.Warning($"[LargerCart] Setting mass failed: {ex.Message}");
            }
            try { body.angularDrag = Math.Max(0f, angBefore * AngularDragMultiplier); } catch (Exception ex)
            {
                MelonLogger.Warning($"[LargerCart] Setting angularDrag failed: {ex.Message}");
            }

            float massAfter = massBefore, angAfter = angBefore;
            try { massAfter = body.mass; } catch { }
            try { angAfter = body.angularDrag; } catch { }
            MelonLogger.Msg($"[LargerCart] Cart stabilized ({how}): " +
                            $"mass {massBefore:0.##} -> {massAfter:0.##}, " +
                            $"angularDrag {angBefore:0.###} -> {angAfter:0.###}.");
        }
        catch (Exception ex)
        {
            MelonLogger.Warning($"[LargerCart] Stabilization failed: {ex.GetBaseException().Message}");
        }
    }
}
