using Harmony;
using System.Reflection;
using System.Collections.Generic;

namespace MoreSeamothUpgrades.Patches
{
    [HarmonyPatch(typeof(SeaMoth))]
    [HarmonyPatch("Update")]
    public class Seamoth_Update_Patch
    {
        static MethodInfo AddEnergyMethod = 
            typeof(Vehicle).GetMethod("AddEnergy", BindingFlags.NonPublic | BindingFlags.Instance);

        static void Prefix(SeaMoth __instance)
        {
            var count = __instance.modules.GetCount(Main.SeamothThermalModule);
            if (count > 0)
            {
                var temperature = __instance.GetTemperature();
                var energyToAdd = Main.ExosuitThermalReactorCharge.Evaluate(temperature);

                AddEnergyMethod.Invoke(__instance, new object[] { energyToAdd * UnityEngine.Time.deltaTime });
            }
        }
    }

    [HarmonyPatch(typeof(SeaMoth))]
    [HarmonyPatch("OnUpgradeModuleChange")]
    public class Seamoth_OnUpgradeModuleChange_Patch
    {
        static readonly string[] _slotIDs = new string[]
        {
            "SeamothModule1",
            "SeamothModule2",
            "SeamothModule3",
            "SeamothModule4"
        };

        static void Postfix(SeaMoth __instance, TechType techType)
        {
            var dictionary = new Dictionary<TechType, float>
            {
                {
                    TechType.SeamothReinforcementModule,
                    800f
                },
                {
                    TechType.VehicleHullModule1,
                    100f
                },
                {
                    TechType.VehicleHullModule2,
                    300f
                },
                {
                    TechType.VehicleHullModule3,
                    700f
                },
                {
                    Main.SeamothHullModule4,
                    1100f
                },
                {
                    Main.SeamothHullModule5,
                    1500f
                }
            };

            var depthUpgrade = 0f;
            for (int i = 0; i < _slotIDs.Length; i++)
            {
                var slot = _slotIDs[i];
                var techTypeInSlot = __instance.modules.GetTechTypeInSlot(slot);

                if (dictionary.ContainsKey(techTypeInSlot))
                {
                    var currentIteration = dictionary[techTypeInSlot];
                    if (currentIteration > depthUpgrade)
                    {
                        depthUpgrade = currentIteration;
                    }
                }
            }

            __instance.crushDamage.SetExtraCrushDepth(depthUpgrade);
        }
    }
}
