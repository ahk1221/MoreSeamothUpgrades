using Harmony;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace MoreSeamothUpgrades.Patches
{
    [HarmonyPatch(typeof(ExosuitDrillArm))]
    [HarmonyPatch("OnHit")]
    public class ExosuitDrillArm_OnHit_Patch
    {
        static void Prefix()
        {
            ErrorMessage.AddDebug("OnHit called!!");
        }
    }

    [HarmonyPatch(typeof(Vehicle))]
    [HarmonyPatch("FixedUpdate")]
    public class Vehicle_FixedUpdate_Patch
    {
        static void Prefix(Vehicle __instance)
        {
            if (!__instance.GetType().Equals(typeof(SeaMoth))) return;

            var seamoth = (SeaMoth)__instance;
            var count = seamoth.modules.GetCount(Main.SeamothDrillModule);

            if (count <= 0) return;

            //if (!GameInput.GetButtonHeld(GameInput.Button.LeftHand) || Player.main.GetVehicle() != seamoth) return;

            var pos = Vector3.zero;
            var hitObj = default(GameObject);

            UWE.Utils.TraceFPSTargetPosition(seamoth.gameObject, 5f, ref hitObj, ref pos, true);

            if (hitObj == null)
            {
                var component = Player.main.gameObject.GetComponent<InteractionVolumeUser>();
                if (component != null && component.GetMostRecent() != null)
                {
                    hitObj = component.GetMostRecent().gameObject;
                }
            }

            if (hitObj == null) return;

            var drillable = hitObj.FindAncestor<Drillable>();

            if (drillable)
            {
                drillable.OnDrill(drillable.transform.position, null, out GameObject hitObject);
                drillable.GetComponent<DrillableInfo>().drillingVehicle = seamoth;
            }
            else
            {
                LiveMixin liveMixin = hitObj.FindAncestor<LiveMixin>();
                if (liveMixin)
                {
                    bool flag = liveMixin.IsAlive();
                    liveMixin.TakeDamage(4f, pos, DamageType.Drill, null);
                }

                hitObj.SendMessage("BashHit", seamoth, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

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
