using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Harmony;
using UnityEngine;

namespace MoreSeamothUpgrades.Patches
{
    [HarmonyPatch(typeof(Drillable))]
    [HarmonyPatch("Start")]
    public class Drillable_Start_Patch 
    {
        static void Prefix(Drillable __instance)
        {
            __instance.gameObject.AddComponent<DrillableInfo>();
        }
    }

    [HarmonyPatch(typeof(Drillable))]
    [HarmonyPatch("OnDrill")]
    public class Drillable_OnDrill_Patch
    {
        static FieldInfo LootPinataObjects =
            typeof(Drillable).GetField("lootPinataObjects", BindingFlags.NonPublic | BindingFlags.Instance);

        static FieldInfo TimeLastDrilled =
            typeof(Drillable).GetField("timeLastDrilled", BindingFlags.NonPublic | BindingFlags.Instance);

        static FieldInfo Renderers =
            typeof(Drillable).GetField("renderers", BindingFlags.NonPublic | BindingFlags.Instance);

        static MethodInfo FindClosestMesh =
            typeof(Drillable).GetMethod("FindClosestMesh", BindingFlags.NonPublic | BindingFlags.Instance);

        static MethodInfo SpawnFX =
            typeof(Drillable).GetMethod("SpawnFX", BindingFlags.NonPublic | BindingFlags.Instance);

        static MethodInfo SpawnLoot =
            typeof(Drillable).GetMethod("SpawnLoot", BindingFlags.Instance | BindingFlags.NonPublic);

        static void Prefix(Drillable __instance, Vector3 position, Exosuit exo, out GameObject hitObject)
        {
            var timeLastDrilled = (float)TimeLastDrilled.GetValue(__instance);
            var renderers = (MeshRenderer[])Renderers.GetValue(__instance);

            float num = 0f;
            for (int i = 0; i < __instance.health.Length; i++)
            {
                num += __instance.health[i];
            }

            if(exo != null)
                __instance.GetComponent<DrillableInfo>().drillingVehicle = exo;

            var pos = Vector3.zero;
            var meshIndex = (int)FindClosestMesh.Invoke(__instance, new object[] { position, pos });
            hitObject = renderers[meshIndex].gameObject;

            TimeLastDrilled.SetValue(__instance, Time.time);
            if (num > 0f)
            {
                float num3 = __instance.health[meshIndex];
                __instance.health[meshIndex] = Mathf.Max(0f, __instance.health[meshIndex] - 5f);
                num -= num3 - __instance.health[meshIndex];
                if (num3 > 0f && __instance.health[meshIndex] <= 0f)
                {
                    renderers[meshIndex].gameObject.SetActive(false);
                    SpawnFX.Invoke(__instance, new object[] { __instance.breakFX, pos });
                    if (UnityEngine.Random.value < __instance.kChanceToSpawnResources)
                    {
                        SpawnLoot.Invoke(__instance, new object[] { pos });
                    }
                }
                if (num <= 0f)
                {
                    SpawnFX.Invoke(__instance, new object[] { __instance.breakAllFX, pos });
                    //__instance.onDrilled?.Invoke(__instance);
                    if (__instance.deleteWhenDrilled)
                    {
                        float time = (!__instance.lootPinataOnSpawn) ? 0f : 6f;
                        __instance.Invoke("DestroySelf", time);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Drillable))]
    [HarmonyPatch("ManagedUpdate")]
    public class Drillable_ManagedUpdate_Patch
    {
        static FieldInfo LootPinataObjects =
            typeof(Drillable).GetField("lootPinataObjects", BindingFlags.NonPublic | BindingFlags.Instance);

        static FieldInfo TimeLastDrilled =
            typeof(Drillable).GetField("timeLastDrilled", BindingFlags.NonPublic | BindingFlags.Instance);

        static bool Prefix(Drillable __instance)
        {
            var timeLastDrilled = (float)TimeLastDrilled.GetValue(__instance);
            var lootPinataObjects = (List<GameObject>)LootPinataObjects.GetValue(__instance);
            var drillingVehicle = __instance.GetComponent<DrillableInfo>().drillingVehicle;

            if (timeLastDrilled + 0.5f > Time.time)
            {
                __instance.modelRoot.transform.position = __instance.transform.position + new Vector3(Mathf.Sin(Time.time * 60f), Mathf.Cos(Time.time * 58f + 0.5f), Mathf.Cos(Time.time * 64f + 2f)) * 0.011f;
            }
            if (lootPinataObjects.Count > 0 && drillingVehicle)
            {
                List<GameObject> list = new List<GameObject>();
                foreach (GameObject gameObject in lootPinataObjects)
                {
                    if (gameObject == null)
                    {
                        list.Add(gameObject);
                    }
                    else
                    {
                        Vector3 b = drillingVehicle.transform.position + new Vector3(0f, 0.8f, 0f);
                        gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, b, Time.deltaTime * 5f);
                        float num = Vector3.Distance(gameObject.transform.position, b);
                        if (num < 3f)
                        {
                            Pickupable pickupable = gameObject.GetComponentInChildren<Pickupable>();
                            if (pickupable)
                            {
                                if (!Player.main.HasInventoryRoom(pickupable))
                                {
                                    if (Player.main.GetVehicle() == drillingVehicle)
                                    {
                                        ErrorMessage.AddMessage(Language.main.Get("ContainerCantFit"));
                                    }
                                }
                                else
                                {
                                    string arg = Language.main.Get(pickupable.GetTechName());
                                    ErrorMessage.AddMessage(Language.main.GetFormat<string>("VehicleAddedToStorage", arg));
                                    uGUI_IconNotifier.main.Play(pickupable.GetTechType(), uGUI_IconNotifier.AnimationType.From, null);
                                    pickupable = pickupable.Initialize();
                                    Inventory.main.Pickup(pickupable);
                                    pickupable.PlayPickupSound();
                                }
                                list.Add(gameObject);
                            }
                        }
                    }
                }
                if (list.Count > 0)
                {
                    foreach (GameObject item2 in list)
                    {
                        lootPinataObjects.Remove(item2);
                    }
                }
            }

            return false;
        }
    }
}
