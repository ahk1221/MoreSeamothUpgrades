using Harmony;
using System.Reflection;
using System.Collections.Generic;
using MoreSeamothUpgrades.MonoBehaviours;
using UnityEngine;

namespace MoreSeamothUpgrades.Patches
{
    public class SeamothUtility
    {
        // Copy-paste of SeaMoth._slotIDs.
        public static readonly string[] slotIDs = new string[]
        {
            "SeamothModule1",
            "SeamothModule2",
            "SeamothModule3",
            "SeamothModule4"
        };
    }

    [HarmonyPatch(typeof(SeaMoth))]
    [HarmonyPatch("OnUpgradeModuleToggle")]
    public class Seamoth_OnUpgradeModuleToggle_Patch
    {
        static void Postfix(SeaMoth __instance, int slotID, bool active)
        {
            // Find the TechType in the toggled slot.
            var techType = __instance.modules.GetTechTypeInSlot(SeamothUtility.slotIDs[slotID]);

            // If its the SeamothDrillModule
            if (techType == Main.SeamothDrillModule)
            { 
                // Get the SeamothDrill component from the SeaMoth object.
                var seamothDrillModule = __instance.GetComponent<SeamothDrill>();

                // If its not null
                if(seamothDrillModule != null)
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
            var count = __instance.modules.GetCount(Main.SeamothThermalModule);
            if (count > 0)
            {
                // Evaluate the energy to add based on temperature
                var temperature = __instance.GetTemperature();
                var energyToAdd = Main.ExosuitThermalReactorCharge.Evaluate(temperature);

                // Add the energy by invoking private method using Reflection.
                AddEnergyMethod.Invoke(__instance, new object[] { energyToAdd * UnityEngine.Time.deltaTime });
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
                    Main.SeamothHullModule4,
                    1100f
                },
                {
                    Main.SeamothHullModule5,
                    1500f
                }
            };

            // Depth upgrade to add.
            var depthUpgrade = 0f;

            // Loop through all slots.
            for (int i = 0; i < SeamothUtility.slotIDs.Length; i++)
            {
                // Get the slot and the TechType
                var slot = SeamothUtility.slotIDs[i];
                var techTypeInSlot = __instance.modules.GetTechTypeInSlot(slot);

                // If its one of the depth modules
                if (dictionary.ContainsKey(techTypeInSlot))
                {
                    // Get the depth upgrade for that TechType
                    var currentIteration = dictionary[techTypeInSlot];

                    // If the upgrade is more than the one currently selected
                    if (currentIteration > depthUpgrade)
                    {
                        // Select this one!
                        depthUpgrade = currentIteration;
                    }
                }
            }

            // Add the upgrade.
            __instance.crushDamage.SetExtraCrushDepth(depthUpgrade);
        }
    }
}
