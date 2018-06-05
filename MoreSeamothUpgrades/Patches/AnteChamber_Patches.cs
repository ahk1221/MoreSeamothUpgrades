using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoreSeamothUpgrades.MonoBehaviours;
using Harmony;

namespace MoreSeamothUpgrades.Patches
{
    [HarmonyPatch(typeof(AnteChamber))]
    [HarmonyPatch("Start")]
    public class AnteChamber_Start_Patch
    {
        static void Postfix(AnteChamber __instance)
        {
            __instance.drillable.onDrilled -= __instance.OnDrilled;
            __instance.drillable.GetComponent<BetterDrillable>().onDrilled += __instance.OnDrilled;
        }
    }
}
