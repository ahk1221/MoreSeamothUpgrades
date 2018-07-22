using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Handlers;
using UnityEngine;

namespace MoreSeamothUpgrades
{
    public abstract class SeamothModule : ModPrefab
    {
        public static TechType SeamothHullModule4 { get; protected set; }
        public static TechType SeamothHullModule5 { get; protected set; }
        public static TechType SeamothDrillModule { get; protected set; }
        public static TechType SeamothThermalModule { get; protected set; }

        public readonly string ID;
        public readonly string DisplayName;
        public readonly string Tooltip;
        public readonly TechType RequiredForUnlock;
        public readonly CraftTree.Type Fabricator;
        public readonly string[] StepsToTab;
        public readonly Atlas.Sprite Sprite;

        protected SeamothModule(string id, string displayName, string tooltip, CraftTree.Type fabricator, string[] stepsToTab, TechType requiredToUnlock = TechType.None, Atlas.Sprite sprite = null) : base(id, $"WorldEntities/Tools/{id}", TechType.None)
        {
            ID = id;
            DisplayName = displayName;
            Tooltip = tooltip;
            Fabricator = fabricator;
            RequiredForUnlock = requiredToUnlock;
            StepsToTab = stepsToTab;
            Sprite = sprite;

            Patch();
        }

        public void Patch()
        {
            TechType = TechTypeHandler.AddTechType(ID, DisplayName, Tooltip, RequiredForUnlock == TechType.None);

            if (RequiredForUnlock != TechType.None)
                KnownTechHandler.SetAnalysisTechEntry(RequiredForUnlock, new TechType[] { TechType });

            if (Sprite == null)
                SpriteHandler.RegisterSprite(TechType, $"./QMods/MoreSeamothUpgrades/Assets/{ID}.png");
            else
                SpriteHandler.RegisterSprite(TechType, Sprite);

            CraftDataHandler.SetEquipmentType(TechType, EquipmentType.SeamothModule);
            CraftDataHandler.SetTechData(TechType, GetTechData());

            CraftTreeHandler.AddCraftingNode(Fabricator, TechType, StepsToTab);
        }

        public override GameObject GetGameObject()
        {
            // Get the ElectricalDefense module prefab and instantiate it
            var path = "WorldEntities/Tools/SeamothElectricalDefense";
            var prefab = Resources.Load<GameObject>(path);
            var obj = GameObject.Instantiate(prefab);

            // Get the TechTags and PrefabIdentifiers
            var techTag = obj.GetComponent<TechTag>();
            var prefabIdentifier = obj.GetComponent<PrefabIdentifier>();

            // Change them so they fit to our requirements.
            techTag.type = TechType;
            prefabIdentifier.ClassId = ClassID;

            return obj;
        }

        public abstract TechData GetTechData();
    }
}
