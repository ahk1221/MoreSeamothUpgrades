using Harmony;
using UnityEngine;
using MoreSeamothUpgrades.MonoBehaviours;

namespace MoreSeamothUpgrades.Patches
{
    [HarmonyPatch(typeof(Drillable))]
    [HarmonyPatch("Start")]
    public class Drillable_Start_Patch 
    {
        static void Prefix(Drillable __instance)
        {
            var betterDrillable = __instance.gameObject.AddComponent<BetterDrillable>();
            betterDrillable.drillable = __instance;

            betterDrillable.resources = __instance.resources;
            betterDrillable.breakFX = __instance.breakFX;
            betterDrillable.breakAllFX = __instance.breakAllFX;
            betterDrillable.primaryTooltip = __instance.primaryTooltip;
            betterDrillable.secondaryTooltip = __instance.secondaryTooltip;
            betterDrillable.deleteWhenDrilled = __instance.deleteWhenDrilled;
            betterDrillable.modelRoot = __instance.modelRoot;
            betterDrillable.minResourcesToSpawn = __instance.minResourcesToSpawn;
            betterDrillable.maxResourcesToSpawn = __instance.maxResourcesToSpawn;
            betterDrillable.lootPinataOnSpawn = __instance.lootPinataOnSpawn;
            betterDrillable.health = __instance.health;
        }
    }

    [HarmonyPatch(typeof(Drillable))]
    [HarmonyPatch("OnDrill")]
    public class Drillable_OnDrill_Patch
    {
        static bool Prefix(Drillable __instance, Vector3 position, Exosuit exo, out GameObject hitObject)
        {
            var betterDrillable = __instance.GetComponent<BetterDrillable>();
            betterDrillable.OnDrill(position, exo, out hitObject);

            return false;
        }
    }

    [HarmonyPatch(typeof(Drillable))]
    [HarmonyPatch("HoverDrillable")]
    public class Drillable_HoverDrillable_Patch
    {
        static bool Prefix(Drillable __instance)
        {
            var betterDrillable = __instance.GetComponent<BetterDrillable>();
            betterDrillable.HoverDrillable();

            return false;
        }
    }

    [HarmonyPatch(typeof(Drillable))]
    [HarmonyPatch("Restore")]
    public class Drillable_Restore_Patch
    {
        static bool Prefix(Drillable __instance)
        {
            var betterDrillable = __instance.GetComponent<BetterDrillable>();
            betterDrillable.Restore();

            return false;
        }
    }

    [HarmonyPatch(typeof(Drillable))]
    [HarmonyPatch("ManagedUpdate")]
    public class Drillable_ManagedUpdate_Patch
    {
        static bool Prefix(Drillable __instance)
        {
            var betterDrillable = __instance.GetComponent<BetterDrillable>();
            betterDrillable.ManagedUpdate();

            return false;
        }
    }
}
