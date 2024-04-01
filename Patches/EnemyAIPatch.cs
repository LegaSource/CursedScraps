using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CursedScraps.Patches
{
    internal class EnemyAIPatch
    {
        public static bool isShadow = false;

        [HarmonyPatch(typeof(EnemyAI), "UpdateEnemyPositionClientRpc")]
        [HarmonyPostfix]
        private static void UpdateMeshWithShadowCurse(ref EnemyAI __instance)
        {
            if (isShadow)
            {
                ScanNodeProperties componentInChildren = ((Component)(object)__instance).gameObject.GetComponentInChildren<ScanNodeProperties>();
                if (componentInChildren != null && HUDManager.Instance != null)
                {
                    FieldInfo scanNodes = AccessTools.Field(typeof(HUDManager), "scanNodes");
                    if (((Dictionary<RectTransform, ScanNodeProperties>)scanNodes.GetValue(HUDManager.Instance)).ContainsValue(componentInChildren))
                    {
                        __instance.EnableEnemyMesh(false);
                    }
                    else
                    {
                        __instance.EnableEnemyMesh(true);
                    }
                }
            }
            else
            {
                __instance.EnableEnemyMesh(true);
            }
        }
    }
}
