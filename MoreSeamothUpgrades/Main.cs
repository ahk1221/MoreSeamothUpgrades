using System.Reflection;
using UnityEngine;
using SMLHelper.V2.Handlers;
using Harmony;
using System;
using MoreSeamothUpgrades.Modules;
using System.IO;

namespace MoreSeamothUpgrades
{
    public class Main
    {
        public static AnimationCurve ExosuitThermalReactorCharge;
        public static FMOD_CustomLoopingEmitter DrillLoop;
        public static FMOD_CustomLoopingEmitter DrillLoopHit;
        public static float DrillNodeHealth = 200f;

        private static MethodInfo GetArmPrefabMethod =
            typeof(Exosuit).GetMethod("GetArmPrefab", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Patch()
        {
            try
            {
                var harmony = HarmonyInstance.Create("com.ahk1221.moreseamothupgrades");
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                LanguageHandler.SetLanguageLine("Tooltip_VehicleHullModule3", "Enhances diving depth. Does not stack"); // To update conflicts about the maximity.

                var exosuit = Resources.Load<GameObject>("WorldEntities/Tools/Exosuit").GetComponent<Exosuit>();
                ExosuitThermalReactorCharge = exosuit.thermalReactorCharge;

                var exosuitDrillArmGO = (GameObject)GetArmPrefabMethod.Invoke(exosuit, new object[] { TechType.ExosuitDrillArmModule });
                var exosuitDrillArm = exosuitDrillArmGO.GetComponent<ExosuitDrillArm>();
                DrillLoopHit = exosuitDrillArm.loopHit;
                DrillLoop = exosuitDrillArm.loop;

                var path = @"./QMods/QuickMiner/mod.json";
                if (!File.Exists(path))
                {
                    Console.WriteLine("[MoreSeamothUpgrades] Quick Miner not installed; node health set to default");
                }
                else
                {
                    Console.WriteLine("[MoreSeamothUpgrades] Quick Miner IS installed; node health quartered");
                    DrillNodeHealth = 50f;
                }

                PrefabHandler.RegisterPrefab(new SeamothHullModule4());
                PrefabHandler.RegisterPrefab(new SeamothHullModule5());
                PrefabHandler.RegisterPrefab(new SeamothClawModule());
                PrefabHandler.RegisterPrefab(new SeamothDrillModule());
                PrefabHandler.RegisterPrefab(new SeamothThermalModule());

                Console.WriteLine("[MoreSeamothUpgrades] Succesfully patched!");
            }
            catch(Exception e)
            {
                Console.WriteLine("[MoreSeamothUpgrades] Caught exception! " + e.InnerException.Message);
                Console.WriteLine(e.InnerException.StackTrace);
            }
        }
    }
}
