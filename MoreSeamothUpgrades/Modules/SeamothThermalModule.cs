﻿using System.Collections.Generic;
using SMLHelper.V2.Crafting;

namespace MoreSeamothUpgrades.Modules
{
    public class SeamothThermalModule : SeamothModule
    {
        public SeamothThermalModule() : 
            base("SeamothThermalModule", 
                "Seamoth thermal reactor", 
                "Recharges power cells in hot areas. Doesn't stack.", 
                CraftTree.Type.SeamothUpgrades, 
                new string[1] { "SeamothModules" }, 
                TechType.ExosuitThermalReactorModule, 
                TechType.ExosuitThermalReactorModule, 
                SpriteManager.Get(TechType.ExosuitThermalReactorModule))
        {
            SeamothThermalModule = TechType;
        }

        public override TechData GetTechData()
        {
            return new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(TechType.Kyanite, 1),
                    new Ingredient(TechType.Polyaniline, 2),
                    new Ingredient(TechType.WiringKit, 1)
                }
            };
        }
    }
}
