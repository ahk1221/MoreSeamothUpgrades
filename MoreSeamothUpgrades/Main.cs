using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using SMLHelper;
using SMLHelper.Patchers;
using Harmony;
using MoreSeamothUpgrades.MonoBehaviours;
using System;

namespace MoreSeamothUpgrades
{
    public class Main
    {
        public static TechType SeamothHullModule4;
        public static TechType SeamothHullModule5;
        public static TechType SeamothThermalModule;
        public static TechType SeamothDrillModule;

        public static AnimationCurve ExosuitThermalReactorCharge;
        public static FMOD_CustomLoopingEmitter DrillLoop;
        public static FMOD_CustomLoopingEmitter DrillLoopHit;

        private static MethodInfo GetArmPrefabMethod =
            typeof(Exosuit).GetMethod("GetArmPrefab", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Patch()
        {
            try
            {
                #region Patching

                var harmony = HarmonyInstance.Create("com.ahk1221.moreseamothupgrades");
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                LanguagePatcher.customLines.Add("Tooltip_VehicleHullModule3", "Enhances diving depth. Does not stack"); // To update conflicts about the maximity.

                #endregion

                #region Extract From Exosuit

                var exosuit = Resources.Load<GameObject>("WorldEntities/Tools/Exosuit").GetComponent<Exosuit>();
                ExosuitThermalReactorCharge = exosuit.thermalReactorCharge;

                var exosuitDrillArmGO = (GameObject)GetArmPrefabMethod.Invoke(exosuit, new object[] { TechType.ExosuitDrillArmModule });
                var exosuitDrillArm = exosuitDrillArmGO.GetComponent<ExosuitDrillArm>();
                DrillLoopHit = exosuitDrillArm.loopHit;
                DrillLoop = exosuitDrillArm.loop;

                #endregion

                #region Add TechTypes

                SeamothHullModule4 = TechTypePatcher.AddTechType("SeamothHullModule4", "Seamoth depth module MK4", "Enhances diving depth. Does not stack.");
                SeamothHullModule5 = TechTypePatcher.AddTechType("SeamothHullModule5", "Seamoth depth module MK5", "Enhances diving depth to maximum. Does not stack.");
                SeamothThermalModule = TechTypePatcher.AddTechType("SeamothThermalModule", "Seamoth thermal reactor", "Recharges power cells in hot areas. Doesn't stack.");
                SeamothDrillModule = TechTypePatcher.AddTechType("SeamothDrillModule", "Seamoth drill module", "Enables the Seamoth to mine resources like the PRAWN Drill Arm.");

                #endregion

                #region Add Recipes

                var seamothHullModule4Recipe = new TechDataHelper()
                {
                    _techType = SeamothHullModule4,
                    _ingredients = new List<IngredientHelper>()
                {
                    new IngredientHelper(TechType.VehicleHullModule3, 1),
                    new IngredientHelper(TechType.PlasteelIngot, 1),
                    new IngredientHelper(TechType.Nickel, 2),
                    new IngredientHelper(TechType.AluminumOxide, 3)
                },
                    _craftAmount = 1
                };

                var seamothHullModule5Recipe = new TechDataHelper()
                {
                    _techType = SeamothHullModule5,
                    _ingredients = new List<IngredientHelper>()
                {
                    new IngredientHelper(SeamothHullModule4, 1),
                    new IngredientHelper(TechType.Titanium, 5),
                    new IngredientHelper(TechType.Lithium, 2),
                    new IngredientHelper(TechType.Kyanite, 3)
                },
                    _craftAmount = 1
                };

                var seamothThermalModuleRecipe = new TechDataHelper()
                {
                    _techType = SeamothThermalModule,
                    _craftAmount = 1,
                    _ingredients = new List<IngredientHelper>()
                {
                    new IngredientHelper(TechType.Kyanite, 1),
                    new IngredientHelper(TechType.Polyaniline, 2),
                    new IngredientHelper(TechType.WiringKit, 1)
                }
                };

                var seamothDrillModuleRecipe = new TechDataHelper()
                {
                    _techType = SeamothDrillModule,
                    _craftAmount = 1,
                    _ingredients = new List<IngredientHelper>()
                {
                    new IngredientHelper(TechType.ExosuitDrillArmModule, 1),
                    new IngredientHelper(TechType.Diamond, 2),
                    new IngredientHelper(TechType.Titanium, 2)
                }
                };

                CraftDataPatcher.customTechData[SeamothHullModule4] = seamothHullModule4Recipe;
                CraftDataPatcher.customTechData[SeamothHullModule5] = seamothHullModule5Recipe;
                CraftDataPatcher.customTechData[SeamothThermalModule] = seamothThermalModuleRecipe;
                CraftDataPatcher.customTechData[SeamothDrillModule] = seamothDrillModuleRecipe;

                CraftTreePatcher.customNodes.Add(new CustomCraftNode(SeamothHullModule4, CraftTree.Type.Workbench, "SeamothMenu/SeamothHullModule4"));
                CraftTreePatcher.customNodes.Add(new CustomCraftNode(SeamothHullModule5, CraftTree.Type.Workbench, "SeamothMenu/SeamothHullModule5"));
                CraftTreePatcher.customNodes.Add(new CustomCraftNode(SeamothThermalModule, CraftTree.Type.SeamothUpgrades, "SeamothModules/SeamothThermalModule"));
                CraftTreePatcher.customNodes.Add(new CustomCraftNode(SeamothDrillModule, CraftTree.Type.SeamothUpgrades, "SeamothModules/SeamothDrillModule"));

                #endregion

                #region Equipment Types

                CraftDataPatcher.customEquipmentTypes[SeamothHullModule4] = EquipmentType.SeamothModule;
                CraftDataPatcher.customEquipmentTypes[SeamothHullModule5] = EquipmentType.SeamothModule;
                CraftDataPatcher.customEquipmentTypes[SeamothThermalModule] = EquipmentType.SeamothModule;
                CraftDataPatcher.customEquipmentTypes[SeamothDrillModule] = EquipmentType.SeamothModule;

                //TODO: SMLHelper: Add CraftDataPatcher.customSlotTypes.
                var slotTypes = (Dictionary<TechType, QuickSlotType>)typeof(CraftData).GetField("slotTypes", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
                slotTypes[SeamothDrillModule] = QuickSlotType.Selectable;

                #endregion

                #region Add Prefabs

                  CustomPrefabHandler.customPrefabs.Add(new CustomPrefab(
                    "SeamothHullModule4",
                    "WorldEntities/Tools/SeamothHullModule4",
                    SeamothHullModule4,
                    GetSeamothHullModuleIV));

                CustomPrefabHandler.customPrefabs.Add(new CustomPrefab(
                    "SeamothHullModule5",
                    "WorldEntities/Tools/SeamothHullModule5",
                    SeamothHullModule5,
                    GetSeamothHullModuleV));

                CustomPrefabHandler.customPrefabs.Add(new CustomPrefab(
                    "SeamothThermalModule",
                    "WorldEntities/Tools/SeamothThermalModule",
                    SeamothThermalModule,
                    GetSeamothThermalModule));

                CustomPrefabHandler.customPrefabs.Add(new CustomPrefab(
                    "SeamothDrillModule",
                    "WorldEntities/Tools/SeamothDrillModule",
                    SeamothDrillModule,
                    GetSeamothDrillModule));

                #endregion

                #region Add Sprites

                var depthSprite = SpriteManager.Get(TechType.VehicleHullModule3);

                CustomSpriteHandler.customSprites.Add(new CustomSprite(SeamothHullModule4, depthSprite));
                CustomSpriteHandler.customSprites.Add(new CustomSprite(SeamothHullModule5, depthSprite));

                var thermalReactorSprite = SpriteManager.Get(TechType.ExosuitThermalReactorModule);

                CustomSpriteHandler.customSprites.Add(new CustomSprite(SeamothThermalModule, thermalReactorSprite));

                var assetBundle = AssetBundle.LoadFromFile(@"./QMods/MoreSeamothUpgrades/moreseamothupgrades.assets");
                var drillSprite = assetBundle.LoadAsset<Sprite>("SeamothDrill");

                CustomSpriteHandler.customSprites.Add(new CustomSprite(SeamothDrillModule, drillSprite));

                #endregion

                Console.WriteLine("[MoreSeamothUpgrades] Succesfully patched!");
            }
            catch(Exception e)
            {
                Console.WriteLine("[MoreSeamothUpgrades] Caught exception! " + e.InnerException.Message);
                Console.WriteLine(e.InnerException.StackTrace);
            }

        }

        public static GameObject GetSeamothDrillModule()
        {
            return GetSeamothUpgrade(SeamothDrillModule, "SeamothDrillModule").AddComponent<SeamothDrill>().gameObject;
        }

        public static GameObject GetSeamothThermalModule()
        {
            return GetSeamothUpgrade(SeamothThermalModule, "SeamothThermalModule");
        }

        public static GameObject GetSeamothHullModuleIV()
        {
            return GetSeamothUpgrade(SeamothHullModule4, "SeamothHullModule4");
        }

        public static GameObject GetSeamothHullModuleV()
        {
            return GetSeamothUpgrade(SeamothHullModule5, "SeamothHullModule5");
        }

        public static GameObject GetSeamothUpgrade(TechType techType, string classId)
        {
            // Get the ElectricalDefense module prefab and instantiate it
            var path = "WorldEntities/Tools/SeamothElectricalDefense";
            var prefab = Resources.Load<GameObject>(path);
            var obj = GameObject.Instantiate(prefab);

            // Get the TechTags and PrefabIdentifiers
            var techTag = obj.GetComponent<TechTag>();
            var prefabIdentifier = obj.GetComponent<PrefabIdentifier>();

            // Change them so they fit to our requirements.
            techTag.type = techType;
            prefabIdentifier.ClassId = classId;

            return obj;
        }
    }
}
