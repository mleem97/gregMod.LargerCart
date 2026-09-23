using System;
using MelonLoader;
using UnityEngine;

namespace greg.Mods.LargerCart.Patches;

/// <summary>
/// Klapp-Tisch auf dem Trolley: eine Ablageplatte mit Beinen, per Taste
/// ein-/ausklappbar (nur Optik, kein Collider — blockiert keine Klicks).
/// Geometrie wird aus den Trolley-Bounds abgeleitet (passt sich an),
/// Material vom Trolley uebernommen (garantiert gueltiger Shader).
/// </summary>
internal static class CartTable
{
    internal static bool Enabled = true;

    private static GameObject _table;

    internal static bool IsVisible => _table != null && _table.activeInHierarchy;

    internal static bool TraySlots = true;
    internal const int TrayCols = 4;
    internal const int TrayRows = 3;

    private static float _plateTopY;
    private static float _plateCx, _plateCz, _plateW, _plateD;
    private static bool _slotsAdded;

    /// <summary>Liveness-Check: Szenenwechsel zerstoert den Trolley mitsamt
    /// Kind-Tisch (kein DontDestroyOnLoad) — dann neu bauen.</summary>
    /// <summary>
    /// Haengt ein Tray-Raster (Modulboxen) auf Plattenhoehe in
    /// positionsOnTrolley/usedPositions ein. Idempotent pro Tisch.
    /// </summary>
    internal static void EnsureTraySlots(global::Il2Cpp.TrolleyLoadingBay bay)
    {
        try
        {
            if (!TraySlots || _table == null || _slotsAdded || bay == null) return;
            var positions = bay.positionsOnTrolley;
            var used = bay.usedPositions;
            if (positions == null || used == null) return;

            int count = TrayCols * TrayRows;
            var slots = new System.Collections.Generic.List<UnityEngine.Transform>();
            // Slots NICHT unter den Tisch (der ist per Taste ausblendbar),
            // sondern unter den Trolley: abgestellte Trays bleiben sichtbar,
            // auch wenn die Platte eingeklappt ist. Positionen sind statisch,
            // da der Tisch starr am Trolley haengt.
            UnityEngine.Transform slotParent = null;
            try { slotParent = _table.transform.parent ?? _table.transform; } catch { slotParent = _table.transform; }
            Quaternion rot;
            try { rot = _table.transform.rotation; } catch { rot = UnityEngine.Quaternion.identity; }
            for (int r = 0; r < TrayRows; r++)
            for (int c = 0; c < TrayCols; c++)
            {
                float x = _plateCx + (c - (TrayCols - 1) * 0.5f) * (_plateW / TrayCols);
                float z = _plateCz + (r - (TrayRows - 1) * 0.5f) * (_plateD / TrayRows);
                var go = new UnityEngine.GameObject($"GregCart_TraySlot_{r}_{c}");
                go.transform.SetParent(slotParent, false);
                go.transform.position = new UnityEngine.Vector3(x, _plateTopY + 0.02f, z);
                go.transform.rotation = rot;
                slots.Add(go.transform);
            }

            var grownPos = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<UnityEngine.Transform>(positions.Count + count);
            for (int i = 0; i < positions.Count; i++) grownPos[i] = positions[i];
            for (int i = 0; i < count; i++) grownPos[positions.Count + i] = slots[i];
            var grownUsed = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<int>(used.Count + count);
            for (int i = 0; i < used.Count; i++) grownUsed[i] = used[i];
            for (int i = used.Count; i < used.Count + count; i++) grownUsed[i] = 0;

            bay.positionsOnTrolley = grownPos;
            bay.usedPositions = grownUsed;
            _slotsAdded = true;
            MelonLogger.Msg($"[LargerCart] {count} Tray-Slots auf Tischhoehe ({_plateTopY:0.00}m) eingehängt.");
        }
        catch (Exception ex)
        {
            MelonLogger.Warning($"[LargerCart] Tray-Slots fehlgeschlagen: {ex.GetBaseException().Message}");
        }
    }

    private static void DropIfDead()
    {
        if (_table == null) return;
        try { var _ = _table.transform; }
        catch { _table = null; }
    }

    internal static void Toggle()
    {
        try
        {
            if (_table == null) return;
            _table.SetActive(!_table.activeSelf);
            MelonLogger.Msg($"[LargerCart] Tisch {(_table.activeSelf ? "ausgeklappt." : "eingeklappt.")}");
        }
        catch (Exception ex)
        {
            MelonLogger.Warning($"[LargerCart] Tisch-Toggle fehlgeschlagen: {ex.GetBaseException().Message}");
        }
    }

    internal static void TryEnsure(global::Il2Cpp.TrolleyLoadingBay bay, float heightAboveTop)
    {
        try
        {
            if (!Enabled) return;
            DropIfDead();
            if (_table != null) return;

            GameObject trolley = FindTrolley(bay);
            if (trolley == null) return; // spaeter erneut versuchen (OnUpdate)

            Bounds bounds = Measure(trolley);
            if (bounds.size.magnitude <= 0.01f)
            {
                MelonLogger.Warning("[LargerCart] Trolley-Bounds leer — Tisch uebersprungen.");
                return;
            }

            Material mat = StealMaterial(trolley);

            _table = new GameObject("GregCart_Table");
            _table.transform.SetParent(trolley.transform, true);
            _table.SetActive(false);

            float plateW = Math.Max(0.3f, bounds.size.x * 0.9f);
            float plateD = Math.Max(0.3f, bounds.size.z * 0.9f);
            const float plateH = 0.05f;
            float plateY = bounds.max.y + Math.Max(0.1f, heightAboveTop);

            AddSlab(_table.transform, mat,
                new Vector3(bounds.center.x, plateY, bounds.center.z),
                new Vector3(plateW, plateH, plateD));

            const float legT = 0.06f;
            float legH = Math.Max(0.1f, plateY - plateH * 0.5f - bounds.min.y);
            float legY = plateY - plateH * 0.5f - legH * 0.5f;
            float ix = plateW * 0.5f - legT, iz = plateD * 0.5f - legT;
            foreach (var (ox, oz) in new[] { (-ix, -iz), (ix, -iz), (-ix, iz), (ix, iz) })
            {
                AddSlab(_table.transform, mat,
                    new Vector3(bounds.center.x + ox, legY, bounds.center.z + oz),
                    new Vector3(legT, legH, legT));
            }

            // Kein DontDestroyOnLoad: stirbt mit dem Trolley beim Szenenwechsel,
            // DropIfDead() + TryEnsure bauen ihn neu.
            _plateTopY = plateY + plateH * 0.5f;
            _plateCx = bounds.center.x; _plateCz = bounds.center.z;
            _plateW = plateW; _plateD = plateD;
            _slotsAdded = false;
            MelonLogger.Msg($"[LargerCart] Tisch gebaut ({plateW:0.00}x{plateD:0.00}m, {_table.transform.childCount} Teile). Taste zum Ausklappen.");
        }
        catch (Exception ex)
        {
            MelonLogger.Warning($"[LargerCart] Tisch-Bau fehlgeschlagen: {ex.GetBaseException().Message}");
        }
    }

    private static void AddSlab(Transform parent, Material mat, Vector3 worldPos, Vector3 worldScale)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        try
        {
            var col = cube.GetComponent<Collider>();
            if (col != null) UnityEngine.Object.Destroy(col);
        }
        catch { }
        cube.name = "GregCart_TablePart";
        cube.transform.SetParent(parent, false);
        cube.transform.position = worldPos;
        cube.transform.localScale = worldScale;
        if (mat != null)
        {
            try
            {
                var rend = cube.GetComponent<Renderer>();
                if (rend != null) rend.material = mat;
            }
            catch { }
        }
        cube.SetActive(true);
    }

    private static GameObject FindTrolley(global::Il2Cpp.TrolleyLoadingBay bay)
    {
        try
        {
            var controller = bay != null ? bay.carController : null;
            var go = controller != null ? controller.gameObject : null;
            if (go != null) return go;
        }
        catch { }
        try
        {
            foreach (var rb in UnityEngine.Object.FindObjectsOfType<Rigidbody>())
            {
                if (rb == null || rb.gameObject == null) continue;
                string n = rb.gameObject.name ?? "";
                if (n.IndexOf("trolley", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    n.IndexOf("trolly", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    n.IndexOf("cart", StringComparison.OrdinalIgnoreCase) >= 0)
                    return rb.gameObject;
            }
        }
        catch { }
        return null;
    }

    private static Bounds Measure(GameObject go)
    {
        var bounds = new Bounds();
        bool has = false;
        try
        {
            foreach (var rend in go.GetComponentsInChildren<Renderer>(true))
            {
                if (rend == null) continue;
                try
                {
                    if (!has) { bounds = rend.bounds; has = true; }
                    else bounds.Encapsulate(rend.bounds);
                }
                catch { }
            }
        }
        catch { }
        return has ? bounds : default;
    }

    private static Material StealMaterial(GameObject go)
    {
        try
        {
            foreach (var rend in go.GetComponentsInChildren<Renderer>(true))
            {
                if (rend == null) continue;
                try
                {
                    var m = rend.sharedMaterial;
                    if (m != null && m.shader != null) return m;
                }
                catch { }
            }
        }
        catch { }
        return null;
    }
}
