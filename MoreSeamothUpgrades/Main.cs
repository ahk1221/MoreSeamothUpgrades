using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using UnityEngine;
using SMLHelper;
using SMLHelper.Patchers;
using Harmony;

namespace MoreSeamothUpgrades
{
    public class Main
    {
        public static TechType SeamothHullModule4;
        public static TechType SeamothHullModule5;
        public static TechType SeamothDrillModule;

        public static void Patch()
        {
            #region Patching

            var harmony = HarmonyInstance.Create("com.ahk1221.moreseamothupgrades");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            LanguagePatcher.customLines.Add("Tooltip_VehicleHullModule3", "Enhances diving depth. Does not stack"); // To update conflicts about the maximity.

            #endregion

            #region Add TechTypes

            SeamothHullModule4 = TechTypePatcher.AddTechType("SeamothHullModule4", "Seamoth depth module MK4", "Enhances diving depth. Does not stack.");
            SeamothHullModule5 = TechTypePatcher.AddTechType("SeamothHullModule5", "Seamoth depth module MK5", "Enhances diving depth to maximum. Does not stack.");
            SeamothDrillModule = TechTypePatcher.AddTechType("SeamothDrillModule", "Seamoth drill module", "Allows the Seamoth to drill resource nodes like the PRAWN Drill Arm.");

            #endregion

            #region Equipment Types

            CraftDataPatcher.customEquipmentTypes[SeamothHullModule4] = EquipmentType.SeamothModule;
            CraftDataPatcher.customEquipmentTypes[SeamothHullModule5] = EquipmentType.SeamothModule;
            CraftDataPatcher.customEquipmentTypes[SeamothDrillModule] = EquipmentType.SeamothModule;

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

            #endregion

            #region Add Sprites
            var sprite = SpriteManager.Get(TechType.VehicleHullModule3);

            CustomSpriteHandler.customSprites.Add(new CustomSprite(SeamothHullModule4, sprite));
            CustomSpriteHandler.customSprites.Add(new CustomSprite(SeamothHullModule5, sprite));
            
            #endregion
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
            var path = "WorldEntities/Tools/SeamothElectricalDefense";
            var prefab = Resources.Load<GameObject>(path);
            var obj = GameObject.Instantiate(prefab);

            var techTag = obj.GetComponent<TechTag>();
            var prefabIdentifier = obj.GetComponent<PrefabIdentifier>();

            techTag.type = techType;
            prefabIdentifier.ClassId = classId;

            return obj;
        }
    }
}
