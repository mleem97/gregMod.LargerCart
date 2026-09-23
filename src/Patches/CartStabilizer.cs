using System;
using System.Collections.Generic;
using MelonLoader;
using UnityEngine;

namespace greg.Mods.LargerCart.Patches;

/// <summary>
/// Macht den Trolley schwerer und taumel-resistenter, damit er beladen nicht
/// durch die Gegend fliegt. Ansatzpunkt ist der Rigidbody des Trolleys
/// (ueber TrolleyLoadingBay.carController, Fallback: Namenssuche).
/// Multiplikatoren statt Absolutwerten: Vanilla-Basis bleibt Referenz.
/// Pro Szene einmalig pro Rigidbody (Pointer-Set gegen Doppel-Anwendung).
/// </summary>
internal static class CartStabilizer
{
    internal static bool Enabled = true;
    internal static float MassMultiplier = 5f;
    internal static float AngularDragMultiplier = 10f;

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
            catch { /* fallback unten */ }

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
                            how = $"Namenssuche '{n}'";
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Warning($"[LargerCart] Trolley-Suche fehlgeschlagen: {ex.Message}");
                    return;
                }
            }

            if (body == null)
            {
                MelonLogger.Warning("[LargerCart] Kein Trolley-Rigidbody gefunden (Stabilisierung uebersprungen).");
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
                MelonLogger.Warning($"[LargerCart] Masse setzen fehlgeschlagen: {ex.Message}");
            }
            try { body.angularDrag = Math.Max(0f, angBefore * AngularDragMultiplier); } catch (Exception ex)
            {
                MelonLogger.Warning($"[LargerCart] AngularDrag setzen fehlgeschlagen: {ex.Message}");
            }

            float massAfter = massBefore, angAfter = angBefore;
            try { massAfter = body.mass; } catch { }
            try { angAfter = body.angularDrag; } catch { }
            MelonLogger.Msg($"[LargerCart] Cart stabilisiert ({how}): " +
                            $"Masse {massBefore:0.##} -> {massAfter:0.##}, " +
                            $"AngularDrag {angBefore:0.###} -> {angAfter:0.###}.");
        }
        catch (Exception ex)
        {
            MelonLogger.Warning($"[LargerCart] Stabilisierung fehlgeschlagen: {ex.GetBaseException().Message}");
        }
    }
}
