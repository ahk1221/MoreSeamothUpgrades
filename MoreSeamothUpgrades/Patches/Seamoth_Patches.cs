using Harmony;
using System.Reflection;
using System.Collections.Generic;
using MoreSeamothUpgrades.MonoBehaviours;
using UnityEngine;

namespace MoreSeamothUpgrades.Patches
{
    [HarmonyPatch(typeof(SeaMoth))]
    [HarmonyPatch("OnUpgradeModuleToggle")]
    public class Seamoth_OnUpgradeModuleToggle_Patch
    {
        static void Postfix(SeaMoth __instance, int slotID, bool active)
        {
            // Find the TechType in the toggled slot.
            // Valid inputs would be along the lines of: SeamothModule1, SeamothModule2, etc
            // slotID is 0-based, so an addition of 1 is required.
            var techType = __instance.modules.GetTechTypeInSlot($"SeamothModule{slotID + 1}");

            // If its the SeamothDrillModule
            if (techType == SeamothModule.SeamothDrillModule)
            {
                // Get the SeamothDrill component from the SeaMoth object.
                var seamothDrillModule = __instance.GetComponent<SeamothDrill>();

                // If its not null
                if (seamothDrillModule != null)
                {
                    // Set its toggle!
                    seamothDrillModule.toggle = active;
                }
            }
        }
    }

    [HarmonyPatch(typeof(SeaMoth))]
    [HarmonyPatch("Start")]
    public class Seamoth_Start_Patch
    {
        static void Prefix(SeaMoth __instance)
        {
            // Add the SeamothDrill component to the Seamoth on start.
            __instance.gameObject.AddComponent<SeamothDrill>();

            // Get TemperatureDamage class from SeaMoth
            var tempDamage = __instance.GetComponent<TemperatureDamage>();

            // Set the different fields
            // No need to check for depth because the SeaMoth would already 
            // be dead if you don't have the depth modules.
            tempDamage.baseDamagePerSecond = 0.2f;
            tempDamage.onlyLavaDamage = true;
            tempDamage.minDamageTemperature = 50;
        }
    }

    [HarmonyPatch(typeof(SeaMoth))]
    [HarmonyPatch("Update")]
    public class Seamoth_Update_Patch
    {
        // Reflection Method: AddEnergy
        static MethodInfo AddEnergyMethod = 
            typeof(Vehicle).GetMethod("AddEnergy", BindingFlags.NonPublic | BindingFlags.Instance);

        static void Prefix(SeaMoth __instance)
        {
            // If we have the SeamothThermalModule equipped.
            var count = __instance.modules.GetCount(SeamothModule.SeamothThermalModule);
            if (count > 0)
            {
                // Evaluate the energy to add based on temperature
                var temperature = __instance.GetTemperature();
                var energyToAdd = Main.ExosuitThermalReactorCharge.Evaluate(temperature);

                // Add the energy by invoking private method using Reflection.
                AddEnergyMethod.Invoke(__instance, new object[] { energyToAdd * Time.deltaTime });
            }
        }
    }

    [HarmonyPatch(typeof(SeaMoth))]
    [HarmonyPatch("OnUpgradeModuleChange")]
    public class Seamoth_OnUpgradeModuleChange_Patch
    { 
        static void Postfix(SeaMoth __instance, TechType techType)
        {
            // Dictionary of TechTypes and their depth additions.
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
                    SeamothModule.SeamothHullModule4,
                    1100f
                },
                {
                    SeamothModule.SeamothHullModule5,
                    1500f
                }
            };

            // Depth upgrade to add.
            var depthUpgrade = 0f;

            // Loop through available depth module upgrades
            foreach (var entry in dictionary)
            {
                TechType depthTechType = entry.Key;
                float depthAddition = entry.Value;

                int count = __instance.modules.GetCount(depthTechType);

                // If you have at least 1 such depth module
                if(count > 0)
                {
                    if(depthAddition > depthUpgrade)
                    {
                        depthUpgrade = depthAddition;
                    }
                }
            }

            // Add the upgrade.
            __instance.crushDamage.SetExtraCrushDepth(depthUpgrade);
        }
    }
}
