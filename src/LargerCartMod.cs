using System;
using HarmonyLib;
using MelonLoader;
using greg.Mods.LargerCart.Config;
using greg.Mods.LargerCart.Patches;

[assembly: MelonInfo(typeof(greg.Mods.LargerCart.LargerCartMod), "gregMod.LargerCart", "2.0.0", "teamGregModding / mleem97 & BigTexasJerky")]
[assembly: MelonGame("Waseku", "Data Center")]

namespace greg.Mods.LargerCart;

public class LargerCartMod : MelonMod
{
    private const string HarmonyId = "com.gregmod.largercart";
    private const string LegacyHarmonyId = "bigtexasjerky.datacenter.texassizedtrolley";

    public override void OnInitializeMelon()
    {
        var config = LargerCartConfig.Load();
        TrolleyLoadingBayPatch.TargetCount = config.TargetPositionCount;
        CartStabilizer.Enabled = config.StabilizeCart;
        CartStabilizer.MassMultiplier = config.CartMassMultiplier;
        CartStabilizer.AngularDragMultiplier = config.CartAngularDragMultiplier;
        MelonLogger.Msg($"[LargerCart] v2.0.0 geladen. TargetPositionCount = {config.TargetPositionCount}, " +
                        $"StabilizeCart = {config.StabilizeCart} " +
                        $"(Masse x{config.CartMassMultiplier:0.#}, AngularDrag x{config.CartAngularDragMultiplier:0.#}).");

        try
        {
            HarmonyLib.Harmony.UnpatchID(LegacyHarmonyId);
        }
        catch (Exception ex)
        {
            MelonLogger.Warning($"[LargerCart] Legacy-Unpatch fehlgeschlagen: {ex.Message}");
        }

        try
        {
            var harmony = new HarmonyLib.Harmony(HarmonyId);
            harmony.PatchAll(typeof(TrolleyLoadingBayPatch).Assembly);
            MelonLogger.Msg("[LargerCart] Il2Cpp.TrolleyLoadingBay.Start() gepatcht.");
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"[LargerCart] Patch fehlgeschlagen: {ex.GetBaseException().Message}");
        }
    }
}
