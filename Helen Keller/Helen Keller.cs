using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[BepInPlugin("com.Domiyaa.HelenKeller", "Helen Keller Mod", "0.1.0")]
public class HelenKellerPlugin : BaseUnityPlugin
{
    public static ConfigEntry<bool> ModEnabled;
    public static BepInEx.Logging.ManualLogSource Log;

    private void Awake()
    {
        Log = Logger;

        ModEnabled = Config.Bind(
            "Settings",
            "Enabled",
            true,
            "yea or na"
        );

        var harmony = new Harmony("com.Domiyaa.HelenKeller");
        harmony.PatchAll();

        Logger.LogInfo("Helen Keller Mod loaded");
    }
}

[HarmonyPatch(typeof(NightVision), "Start")]
public class HelenKeller_Start_Patch
{
    static void Postfix(NightVision __instance)
    {
        Volume postProcessing = HelenKeller_Update_Patch.PostProcessingRef(__instance);

        if (postProcessing == null)
            return;

        ColorAdjustments colorAdjustments = HelenKeller_Update_Patch.ColorAdjustmentsRef(__instance);

        if (colorAdjustments == null)
            return;

        HelenKeller_Update_Patch.origSaturation = colorAdjustments.saturation.value;
        HelenKeller_Update_Patch.origContrast = colorAdjustments.contrast.value;
        HelenKeller_Update_Patch.origColorFilter = colorAdjustments.colorFilter.value;
        HelenKeller_Update_Patch.origHueShift = colorAdjustments.hueShift.value;
        HelenKeller_Update_Patch.origAudioVolume = AudioListener.volume;
        HelenKeller_Update_Patch.cached = true;
    }
}

[HarmonyPatch(typeof(NightVision), "Update")]
public class HelenKeller_Update_Patch
{
    public static readonly AccessTools.FieldRef<NightVision, bool> NightVisActiveRef =
        AccessTools.FieldRefAccess<NightVision, bool>("nightVisActive");

    public static readonly AccessTools.FieldRef<NightVision, Volume> PostProcessingRef =
        AccessTools.FieldRefAccess<NightVision, Volume>("postProcessing");

    public static readonly AccessTools.FieldRef<NightVision, ColorAdjustments> ColorAdjustmentsRef =
        AccessTools.FieldRefAccess<NightVision, ColorAdjustments>("colorAdjustments");

    public static bool cached = false;
    public static float origSaturation;
    public static float origContrast;
    public static Color origColorFilter;
    public static float origHueShift;
    public static float origAudioVolume;

    private static bool lastNightVisActive = false;

    static void Postfix(NightVision __instance)
    {
        if (!cached || !HelenKellerPlugin.ModEnabled.Value)
            return;

        bool nightVisActive = NightVisActiveRef(__instance);

        if (nightVisActive == lastNightVisActive)
            return;

        lastNightVisActive = nightVisActive;

        ColorAdjustments colorAdjustments = ColorAdjustmentsRef(__instance);

        if (colorAdjustments == null)
            return;

        if (nightVisActive)
        {
            colorAdjustments.saturation.value = origSaturation;
            colorAdjustments.contrast.value = origContrast;
            colorAdjustments.colorFilter.value = Color.black;
            colorAdjustments.hueShift.value = origHueShift;

            origAudioVolume = AudioListener.volume;
            AudioListener.volume = 0f;
        }
        else
        {
            colorAdjustments.saturation.value = origSaturation;
            colorAdjustments.contrast.value = origContrast;
            colorAdjustments.colorFilter.value = origColorFilter;
            colorAdjustments.hueShift.value = origHueShift;

            AudioListener.volume = origAudioVolume;
        }
    }
}