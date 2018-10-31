using System.Reflection;
using UnityEngine;
using SMLHelper.V2.Handlers;
using Harmony;
using System;
using MoreSeamothUpgrades.Modules;
using System.IO;
using System.Text.RegularExpressions;

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
                    Console.WriteLine("[MoreSeamothUpgrades] Quick Miner IS installed; reading config...");
                    var qmConfigJson = File.ReadAllText(path);
                    string nodeHealthPattern = "\"NodeHealth\"\\s*:\\s*(\\d+\\.?\\d*)";
                    Match match = Regex.Match(qmConfigJson, nodeHealthPattern);
                    if (match.Success)
                    {
                        GroupCollection iAmGroup = match.Groups;
                        float qmNodeHealth = 0;
                        if (float.TryParse(iAmGroup[1].Value, out qmNodeHealth))
                        {
                            Console.WriteLine("[MoreSeamothUpgrades] New node health is " + qmNodeHealth + ", based on QM config.");
                            DrillNodeHealth = qmNodeHealth;
                        }
                        else
                        {
                            Console.WriteLine("[MoreSeamothUpgrades] Read QM config, but couldn't get the value! Using the default value of 50.");
                            DrillNodeHealth = 50f;
                        }
                    }
                    else
                    {
                        Console.WriteLine("[MoreSeamothUpgrades] Couldn't find the node health config! Using the default value of 50.");
                        DrillNodeHealth = 50f;
                    }
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
